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

        private Gerichtsverhandlung _verhandlung;
        private int _summeVerbrechen;
        private int _deliktpunkte;
        private int _beweise;
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

            if (_verhandlung.GetAngeklagterID() >= SW.Statisch.GetMinKIID())
            {
                // KI-Angeklagter: die Delikte werden zufällig ermittelt.
                for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
                {
                    string vorwurf = SW.Statisch.GetGerichtsGesetzesvorwurf()[i];

                    if (!string.IsNullOrEmpty(vorwurf) && SW.Statisch.Rnd.Next(0, 10) > 3)
                    {
                        _delikte[i] = 1;
                        _deliktpunkte -= 2;

                        if (_deliktpunkte <= 0)
                            break;
                    }
                }
            }
            else
            {
                // Menschlicher Angeklagter: die tatsächlich begangenen Verbrechen werden herangezogen und gesühnt.
                var humAngeklagter = SW.Dynamisch.GetHumWithID(_verhandlung.GetAngeklagterID());

                for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
                {
                    int begangen = humAngeklagter.GetBegingVerbrechenX(i);

                    if (begangen > 0)
                    {
                        _summeVerbrechen += begangen;
                        _delikte[i] = begangen;
                        humAngeklagter.SetBegingVerbrechenX(i, 0);
                    }
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

        /// <summary>Stärke der vom Kläger gegen den Angeklagten gesammelten Beweise (0, wenn keine vorliegen).</summary>
        public int GetBeweise() => _beweise;

        public int GetRichterId(int i) => _verhandlung.GetRichterXID(i);

        public string GetRichterName(int i) => SW.Dynamisch.GetSpWithID(GetRichterId(i)).GetKompletterName();

        public bool IstRichterMensch(int i) => GetRichterId(i) < SW.Statisch.GetMinKIID();

        /// <summary>Berechnet das Urteil eines KI-Richters (true = schuldig) anhand seiner Beziehung zum Angeklagten und dem Schwierigkeitsgrad.</summary>
        public bool BerechneKiUrteil(int i)
        {
            int sympathie = SW.Dynamisch.GetKIwithID(GetRichterId(i)).GetBeziehungZuKIX(_verhandlung.GetAngeklagterID());

            // Neben der Schwere der tatsächlichen Verbrechen zählen die vom Kläger gesammelten Beweise:
            // Je mehr Beweise vorliegen, desto eher entscheidet ein Richter auf "schuldig".
            int faktor = _summeVerbrechen + _beweise;

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
                ergebnis.StrafeText = strafart.StrafeExecute(_verhandlung.GetAngeklagterID(), _deliktpunkte);
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
}
