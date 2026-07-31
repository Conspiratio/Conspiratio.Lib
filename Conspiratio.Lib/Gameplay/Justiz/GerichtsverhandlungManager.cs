using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Einstellungen;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    /// <summary>
    /// Kapselt eine Gerichtsverhandlung (Migration von CheckGerichtsVerhandlungen/GerichtsverhandlungDurchfuehren
    /// aus dem WinForms-Client). Der Ablauf ist schrittweise angelegt, damit der Client die Anzeige steuern
    /// und menschliche Richter interaktiv abstimmen lassen kann: Verhandlung starten (Delikte ermitteln),
    /// die Vorwürfe je Gesetzeskategorie abfragen, die drei Richter abstimmen lassen und die Strafe auswerten.
    /// </summary>
    public class GerichtsverhandlungManager
    {
        public const int RichterAnzahl = 3;

        // Gewicht eines Beweisstücks (tatsächlich begangenes Delikt bzw. vom Kläger erspähtes Delikt)
        // im Urteilsfaktor. Ein einzelnes Delikt wiegt so schwer wie ein guter Teil der Richter-
        // Sympathie (zufällig 20–80), damit echte Beweise im Schnitt zu ~80 % zu einer Verurteilung
        // führen (2–3 Delikte ≈ 63–84 %, ein einzelnes ≈ 31 %, keine ≈ 5 %; siehe BerechneKiUrteil).
        private const int BeweisGewicht = 10;

        private Gerichtsverhandlung _verhandlung;
        private int _summeVerbrechen;
        private int _deliktpunkte;
        private int _beweise;
        private int _aussageUrteilsBonus;
        private double _strafFaktor;
        private int[] _delikte;
        private readonly bool[] _schuldig = new bool[RichterAnzahl];

        /// <summary>Liefert die Indizes der Verhandlungen, an denen der aktive Spieler beteiligt ist (Angeklagter, Kläger oder Richter).</summary>
        public IEnumerable<int> GetVerhandlungenMitAktivemSpieler()
        {
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            for (int i = 0; i < SW.Statisch.GetmaxAnzahlGerichtsverhandlungen(); i++)
            {
                var v = SW.Dynamisch.GetGerichtsverhandlungX(i);

                if (v.GetAngeklagterID() == aktiverSpieler || v.GetKlaegerID() == aktiverSpieler ||
                    v.GetRichterXID(0) == aktiverSpieler || v.GetRichterXID(1) == aktiverSpieler || v.GetRichterXID(2) == aktiverSpieler)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Startet die Verhandlung mit dem angegebenen Index: ermittelt die Delikte des Angeklagten
        /// (KI zufällig, Mensch aus den begangenen Verbrechen) und liefert die Eröffnungsinformationen.
        /// </summary>
        public VerhandlungInfo StarteVerhandlung(int index)
        {
            _verhandlung = SW.Dynamisch.GetGerichtsverhandlungX(index);
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            var angeklagter = SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID());
            var klaeger = SW.Dynamisch.GetSpWithID(_verhandlung.GetKlaegerID());

            var info = new VerhandlungInfo
            {
                KlaegerName = klaeger.GetKompletterName(),
                AngeklagterName = angeklagter.GetKompletterName(),
                AngeklagterMaennlich = angeklagter.GetMaennlich()
            };

            if (_verhandlung.GetAngeklagterID() == aktiverSpieler)
                info.RollenText = "Ihr müsst Euch nun für das Euch Vorgeworfene vor\nGericht verantworten.";
            else if (_verhandlung.GetKlaegerID() == aktiverSpieler)
                info.RollenText = angeklagter.GetMaennlich()
                    ? "Ihr müsst das Gericht nun von der Schuld des\nAngeklagten überzeugen."
                    : "Ihr müsst das Gericht nun von der Schuld der\nAngeklagten überzeugen.";
            else
                info.RollenText = "";

            // Delikte ermitteln
            _summeVerbrechen = 0;
            _deliktpunkte = angeklagter.GetDeliktpunkte();
            _aussageUrteilsBonus = 0;
            _strafFaktor = 1.0;
            _delikte = new int[SW.Statisch.GetMaxGesetze()];

            // Vom (menschlichen) Kläger über seine Spione gegen den KI-Angeklagten gesammelte Beweise.
            // Sie fließen in die Entscheidung der Richter ein (siehe BerechneKiUrteil).
            _beweise = 0;
            if (_verhandlung.GetAngeklagterID() >= SW.Statisch.GetMinKIID()
                && _verhandlung.GetKlaegerID() < SW.Statisch.GetMinKIID())
            {
                _beweise = SW.Dynamisch.GetHumWithID(_verhandlung.GetKlaegerID())
                    .GetAktiveSpionage(_verhandlung.GetAngeklagterID()).GetDelikte();
            }

            // Die tatsächlich begangenen Verbrechen des Angeklagten werden herangezogen und mit der
            // Verhandlung gesühnt – für menschliche wie für KI-Angeklagte gleichermaßen (KI begehen ihre
            // Delikte zum Rundenende, siehe RundenEndeManager.FuehreKiStraftatenDurch).
            for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
            {
                int begangen = angeklagter.GetBegingVerbrechenX(i);

                if (begangen > 0)
                {
                    _summeVerbrechen += begangen;
                    _delikte[i] = begangen;
                    angeklagter.SetBegingVerbrechenX(i, 0);
                }
            }

            info.FinanzVorwuerfe = SammleVorwuerfe(0, SW.Statisch.GetGesetzgrenzeFinanz(), "Finanzgesetz", "Finanzgesetze", angeklagter.GetMaennlich());
            info.StrafVorwuerfe = SammleVorwuerfe(SW.Statisch.GetGesetzgrenzeFinanz(), SW.Statisch.GetGesetzgrenzeStraf(), "Strafgesetz", "Strafgesetze", angeklagter.GetMaennlich());
            info.KirchVorwuerfe = SammleVorwuerfe(SW.Statisch.GetGesetzgrenzeStraf(), SW.Statisch.GetGesetzgrenzeKirche(), "Kirchgesetz", "Kirchgesetze", angeklagter.GetMaennlich());

            return info;
        }

        private VorwurfKategorie SammleVorwuerfe(int von, int bis, string bezeichnungSingular, string bezeichnungPlural, bool maennlich)
        {
            var kategorie = new VorwurfKategorie();

            for (int i = von; i < bis; i++)
            {
                if (_delikte[i] != 0)
                    kategorie.Vorwuerfe.Add(SW.Statisch.GetGerichtsGesetzesvorwurf()[i]);
            }

            if (kategorie.Vorwuerfe.Count > 0)
            {
                string einleitung = maennlich
                    ? "Man beschuldigt den Angeklagten, dass er gegen "
                    : "Man beschuldigt die Angeklagte, dass sie gegen ";

                einleitung += kategorie.Vorwuerfe.Count == 1 ? "folgendes " + bezeichnungSingular + "\n" : "folgende " + bezeichnungPlural + "\n";
                einleitung += "verstoßen hat:\n";

                kategorie.Ueberschrift = einleitung;
            }

            return kategorie;
        }

        /// <summary>Die Verteidigungsformel des Angeklagten ("Nichts von all dem habe ich getan!").</summary>
        public string GetVerteidigung()
        {
            return SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID()).GetMaennlich()
                ? "Angeklagter: \"Nichts von all dem habe ich getan!\""
                : "Angeklagte: \"Nichts von all dem habe ich getan!\"";
        }

        /// <summary>Ob der aktive Spieler in dieser Verhandlung der Angeklagte ist (und daher aussagen darf).</summary>
        public bool IstAngeklagterAktiverSpieler() => _verhandlung.GetAngeklagterID() == SW.Dynamisch.GetAktiverSpieler();

        /// <summary>
        /// Die Aussage-Möglichkeiten des Angeklagten (Issue #18): von der Leugnung bis zum Geständnis, jeweils
        /// mit dem gesprochenen Satz. Die Wirkung setzt <see cref="SetzeAussage"/>.
        /// </summary>
        public List<AussageOption> GetAussageOptionen()
        {
            string sprecher = SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID()).GetMaennlich() ? "Angeklagter" : "Angeklagte";

            return new List<AussageOption>
            {
                new AussageOption(EnumAussage.Leugnen, "Leugnen", sprecher + ": \"Nichts von all dem habe ich getan!\""),
                new AussageOption(EnumAussage.EmpoertLeugnen, "Empört leugnen", sprecher + ": \"Das sind alles Lügen und Verleumdungen!\""),
                new AussageOption(EnumAussage.Teilgestaendnis, "Teilgeständnis", sprecher + ": \"Ich räume einen Teil der Vorwürfe ein und bereue.\""),
                new AussageOption(EnumAussage.Gestaendnis, "Geständnis", sprecher + ": \"Ich bekenne mich in vollem Umfang schuldig.\"")
            };
        }

        /// <summary>
        /// Verarbeitet die Aussage des Angeklagten. Die Wirkung ist gestaffelt und hängt von der Beweislage ab
        /// (Schwere der tatsächlichen Verbrechen plus gesammelte Beweise):
        /// Ein Geständnis führt eher zur Verurteilung, senkt aber die Strafe deutlich; ein Teilgeständnis
        /// mildert moderat. Leugnen hilft nur bei schwacher Beweislage und ist bei starker Lage wirkungslos;
        /// empörtes Leugnen hilft bei schwacher Lage stärker, schlägt bei starker Lage aber ins Gegenteil um
        /// (die Richter nehmen die dreiste Lüge übel und strafen härter).
        /// </summary>
        public void SetzeAussage(EnumAussage aussage)
        {
            int staerke = _summeVerbrechen + _beweise;   // Anzahl der Beweise (Delikte + Spionage)

            switch (aussage)
            {
                case EnumAussage.Gestaendnis:
                    // Führt fast sicher zur Verurteilung, senkt aber die Strafe deutlich.
                    _aussageUrteilsBonus = 18;
                    _strafFaktor = 0.4;
                    break;

                case EnumAussage.Teilgestaendnis:
                    _aussageUrteilsBonus = 8;
                    _strafFaktor = 0.7;
                    break;

                case EnumAussage.Leugnen:
                    // Hilft nur bei schwacher Beweislage (0–1 Delikte), sonst wirkungslos; nie nachteilig.
                    _aussageUrteilsBonus = -Math.Max(0, (2 - staerke) * 9);
                    _strafFaktor = 1.0;
                    break;

                case EnumAussage.EmpoertLeugnen:
                    // Riskant: bei schwacher Lage überzeugender als bloßes Leugnen, bei starker Lage
                    // nehmen die Richter die dreiste Lüge übel – härtere Strafe und eher Verurteilung.
                    if (staerke <= 1)
                    {
                        _aussageUrteilsBonus = -(24 - staerke * 10);   // E=0: -24, E=1: -14
                        _strafFaktor = 1.0;
                    }
                    else
                    {
                        _aussageUrteilsBonus = 12;
                        _strafFaktor = 1.3;
                    }
                    break;
            }
        }

        /// <summary>Stärke der vom Kläger gegen den Angeklagten gesammelten Beweise (0, wenn keine vorliegen).</summary>
        public int GetBeweise() => _beweise;

        public int GetRichterId(int i) => _verhandlung.GetRichterXID(i);

        public string GetRichterName(int i) => SW.Dynamisch.GetSpWithID(GetRichterId(i)).GetKompletterName();

        public bool IstRichterMensch(int i) => GetRichterId(i) < SW.Statisch.GetMinKIID();

        /// <summary>Berechnet das Urteil eines KI-Richters (true = schuldig) anhand seiner Beziehung zum Angeklagten und dem Schwierigkeitsgrad.</summary>
        public bool BerechneKiUrteil(int i)
        {
            int sympathie = SW.Dynamisch.GetKIwithID(GetRichterId(i)).GetBeziehungZuKIX(_verhandlung.GetAngeklagterID());

            // Neben der Anzahl der tatsächlichen Verbrechen zählen die vom Kläger gesammelten Beweise
            // (mehr Beweise -> eher "schuldig") sowie die Aussage des Angeklagten (Geständnis erhöht,
            // Leugnen senkt die Verurteilungsneigung – siehe SetzeAussage). Jedes Beweisstück wird mit
            // BeweisGewicht gewichtet, damit echte Beweise im Schnitt zur Verurteilung führen.
            int faktor = (_summeVerbrechen + _beweise) * BeweisGewicht + _aussageUrteilsBonus;

            switch (SW.Dynamisch.Spielstand.Einstellungen.AggressivitaetKISpieler)
            {
                case EnumSchwierigkeitsgrad.Niedrig:
                    faktor -= 2;
                    break;
                case EnumSchwierigkeitsgrad.Mittel:
                    faktor += 5;
                    break;
                case EnumSchwierigkeitsgrad.Hoch:
                    faktor += 10;
                    break;
            }

            // Rnd.Next verlangt eine positive Obergrenze; bei nicht-positiver Sympathie neigt der Richter zu "schuldig".
            int obergrenze = Math.Max(1, sympathie);

            return !(SW.Statisch.Rnd.Next(0, obergrenze) > faktor);
        }

        /// <summary>Legt das Urteil des Richters i fest (true = schuldig).</summary>
        public void SetzeUrteil(int i, bool schuldig) => _schuldig[i] = schuldig;

        public string GetUrteilText(int i) => _schuldig[i] ? "Schuldig!" : "Nicht schuldig";

        /// <summary>
        /// Wertet die Verhandlung aus: bei mehr als einem Freispruch wird der Angeklagte freigesprochen, sonst
        /// verurteilt (eine zufällige Strafart wird ausgeführt und die Strafmeldung geliefert).
        /// </summary>
        public VerhandlungsErgebnis WerteAus()
        {
            var ergebnis = new VerhandlungsErgebnis();
            var angeklagter = SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID());

            int unschuldig = 0;

            for (int i = 0; i < RichterAnzahl; i++)
            {
                if (!_schuldig[i])
                    unschuldig++;
            }

            if (unschuldig > 1)
            {
                ergebnis.Freigesprochen = true;
                ergebnis.ErgebnisText = angeklagter.GetMaennlich()
                    ? "Damit wird der Angeklagte freigesprochen!"
                    : "Damit wird die Angeklagte freigesprochen!";
            }
            else
            {
                ergebnis.Freigesprochen = false;
                ergebnis.ErgebnisText = angeklagter.GetMaennlich()
                    ? "Damit ist der Angeklagte schuldig!"
                    : "Damit ist die Angeklagte schuldig!";

                int strafIndex = SW.Statisch.Rnd.Next(0, SW.Statisch.GetMaxAnzahlStrafen());
                var strafart = SW.Statisch.GetStrafartX(strafIndex);

                ergebnis.UrteilText = SW.Dynamisch.GetSpWithID(_verhandlung.GetRichterXID(0)).GetKompletterName() +
                                      " entscheidet sich für folgendes Urteil: " + strafart.Name;

                // Die Aussage beeinflusst die Höhe der Strafe (Geständnis mildert, dreiste Lüge verschärft).
                int strafDeliktpunkte = Math.Max(1, (int)Math.Round(_deliktpunkte * _strafFaktor));
                ergebnis.StrafeText = strafart.StrafeExecute(_verhandlung.GetAngeklagterID(), strafDeliktpunkte);
            }

            return ergebnis;
        }

        /// <summary>Schließt die Verhandlung ab und setzt ihren Datensatz zurück.</summary>
        public void SchliesseVerhandlung() => _verhandlung.SetToZero();
    }

    /// <summary>Die Eröffnungsinformationen einer Verhandlung samt der Vorwürfe je Gesetzeskategorie.</summary>
    public class VerhandlungInfo
    {
        public string KlaegerName { get; set; }
        public string AngeklagterName { get; set; }
        public bool AngeklagterMaennlich { get; set; }
        public string RollenText { get; set; }
        public VorwurfKategorie FinanzVorwuerfe { get; set; }
        public VorwurfKategorie StrafVorwuerfe { get; set; }
        public VorwurfKategorie KirchVorwuerfe { get; set; }
    }

    /// <summary>Die Vorwürfe einer Gesetzeskategorie mit ihrer Überschrift (leer, wenn es keine gibt).</summary>
    public class VorwurfKategorie
    {
        public string Ueberschrift { get; set; }
        public List<string> Vorwuerfe { get; } = new List<string>();
    }

    /// <summary>Das Ergebnis einer Verhandlung (Freispruch oder Urteil mit Strafe).</summary>
    public class VerhandlungsErgebnis
    {
        public bool Freigesprochen { get; set; }
        public string ErgebnisText { get; set; }
        public string UrteilText { get; set; }
        public string StrafeText { get; set; }
    }

    /// <summary>Die möglichen Aussagen des Angeklagten vor Gericht (Issue #18).</summary>
    public enum EnumAussage
    {
        Leugnen,
        EmpoertLeugnen,
        Teilgestaendnis,
        Gestaendnis
    }

    /// <summary>Eine wählbare Aussage samt kurzer Schaltflächen-Beschriftung und dem gesprochenen Satz.</summary>
    public class AussageOption
    {
        public EnumAussage Typ { get; }
        public string ButtonText { get; }
        public string Spruch { get; }

        public AussageOption(EnumAussage typ, string buttonText, string spruch)
        {
            Typ = typ;
            ButtonText = buttonText;
            Spruch = spruch;
        }
    }
}
