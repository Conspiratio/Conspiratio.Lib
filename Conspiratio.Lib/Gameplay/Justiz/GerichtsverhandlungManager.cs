using System;
using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
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

        // Bestechung (Issue #18): Um einen Richter (bzw. später Zeugen) sicher auf die Seite des Bestechers
        // zu ziehen, muss der auf ihn entfallende Anteil seine Schwelle erreichen; darunter überzeugt die
        // Bestechung nur anteilig. Als Schwelle dient sein halbes Barvermögen (mind. MinBestechungsSchwelle) –
        // reiche Richter sind teurer zu kaufen.
        private const double BestechungsSchwelleFaktor = 0.5;
        private const int MinBestechungsSchwelle = 3000;

        // Zeugen (Issue #18): Anzahl der geladenen Zeugen und ihr Gewicht im Urteilsfaktor. Ein Zeuge sagt
        // je nach Verhältnis zu Angeklagtem und Kläger für oder gegen den Angeklagten aus – überzeugend (großer
        // Beziehungsunterschied) oder schwach; Bestechung zieht ihn auf die Seite des Bestechers.
        private const int ZeugenAnzahlMax = 2;
        private const int ZeugeUeberzeugend = 8;
        private const int ZeugeSchwach = 4;
        private const int ZeugeUeberzeugungsGrenze = 20;

        // Plädoyers (Issue #18): Das Anklageplädoyer stellt die Beweislast dar (nur Text), das
        // Verteidigungsplädoyer gewichtet das Ansehen des Angeklagten – hohes Ansehen zieht die Richter
        // etwas Richtung Freispruch (_plaedoyerBonus, negativ = weniger schuldig).
        private const int AnsehenHoch = 80;
        private const int AnsehenMittel = 30;
        private const int PlaedoyerBonusHoch = -6;
        private const int PlaedoyerBonusMittel = -3;

        private Gerichtsverhandlung _verhandlung;
        private int _summeVerbrechen;
        private int _deliktpunkte;
        private int _beweise;
        private int _aussageUrteilsBonus;
        private double _strafFaktor;
        private int _bestechungRichter;
        private int _bestechungZeugen;
        private bool _aktivIstAngeklagter;
        private bool _aktivIstKlaeger;
        private List<int> _zeugen = new List<int>();
        private int _zeugenBonus;
        private int _plaedoyerBonus;
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

            // Statistik (Issue #19): Ist der Angeklagte ein menschlicher Spieler, zählt der Prozess als „angeklagt".
            if (_verhandlung.GetAngeklagterID() < SW.Statisch.GetMinKIID())
                SW.Dynamisch.GetHumWithID(_verhandlung.GetAngeklagterID()).GetSpielerStatistik().Soangeklagt++;

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
            _bestechungRichter = 0;
            _bestechungZeugen = 0;
            _aktivIstAngeklagter = _verhandlung.GetAngeklagterID() == aktiverSpieler;
            _aktivIstKlaeger = _verhandlung.GetKlaegerID() == aktiverSpieler;
            _zeugenBonus = 0;
            WaehleZeugen();

            // Verteidigungsplädoyer: hohes Ansehen des Angeklagten zieht die Richter Richtung Freispruch.
            int ansehen = angeklagter.GetAnsehen();
            _plaedoyerBonus = ansehen >= AnsehenHoch ? PlaedoyerBonusHoch : ansehen >= AnsehenMittel ? PlaedoyerBonusMittel : 0;

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

        #region Bestechung (Issue #18)

        /// <summary>Ob der aktive Spieler als Partei (Angeklagter oder Kläger) bestechen darf.</summary>
        public bool KannBestechen() => _aktivIstAngeklagter || _aktivIstKlaeger;

        /// <summary>Anzahl der in dieser Verhandlung aussagenden Zeugen.</summary>
        public int GetZeugenAnzahl() => _zeugen.Count;

        private int RichterSchwelle(int richterIndex)
        {
            int taler = SW.Dynamisch.GetSpWithID(_verhandlung.GetRichterXID(richterIndex)).GetTaler();
            return Math.Max(MinBestechungsSchwelle, (int)(taler * BestechungsSchwelleFaktor));
        }

        /// <summary>Betrag, der alle drei Richter sicher überzeugt (Summe ihrer Einzelschwellen).</summary>
        public int GetRichterBestechungSchwelleGesamt()
        {
            int summe = 0;

            for (int i = 0; i < RichterAnzahl; i++)
                summe += RichterSchwelle(i);

            return summe;
        }

        /// <summary>
        /// Die wählbaren Bestechungsstufen für die Richter, gestaffelt bis zum sicheren Betrag und auf das
        /// Barvermögen des Spielers begrenzt (nur bezahlbare Stufen erscheinen). "Nicht bestechen" ist immer dabei.
        /// </summary>
        public List<BestechungOption> GetRichterBestechungsOptionen()
        {
            return ErzeugeBestechungsOptionen(GetRichterBestechungSchwelleGesamt());
        }

        private int ZeugeSchwelle(int zeugeIndex)
        {
            int taler = SW.Dynamisch.GetKIwithID(_zeugen[zeugeIndex]).GetTaler();
            return Math.Max(MinBestechungsSchwelle, (int)(taler * BestechungsSchwelleFaktor));
        }

        /// <summary>Betrag, der alle Zeugen sicher überzeugt (Summe ihrer Einzelschwellen).</summary>
        public int GetZeugenBestechungSchwelleGesamt()
        {
            int summe = 0;

            for (int i = 0; i < _zeugen.Count; i++)
                summe += ZeugeSchwelle(i);

            return summe;
        }

        /// <summary>Die wählbaren Bestechungsstufen für die Zeugen (siehe <see cref="GetRichterBestechungsOptionen"/>).</summary>
        public List<BestechungOption> GetZeugenBestechungsOptionen()
        {
            return ErzeugeBestechungsOptionen(GetZeugenBestechungSchwelleGesamt());
        }

        private List<BestechungOption> ErzeugeBestechungsOptionen(int sichererBetrag)
        {
            var optionen = new List<BestechungOption> { new BestechungOption("Nicht bestechen", 0) };
            int taler = SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetAktiverSpieler()).GetTaler();

            AddBestechungsStufe(optionen, "Gering", (int)Math.Round(sichererBetrag * 0.35), taler);
            AddBestechungsStufe(optionen, "Mittel", (int)Math.Round(sichererBetrag * 0.70), taler);
            AddBestechungsStufe(optionen, "Hoch", sichererBetrag, taler);

            return optionen;
        }

        private static void AddBestechungsStufe(List<BestechungOption> optionen, string name, int betrag, int taler)
        {
            if (betrag > 0 && betrag <= taler)
                optionen.Add(new BestechungOption(name + " (" + betrag.ToStringGeld() + ")", betrag));
        }

        /// <summary>Bezahlt die Richter-Bestechung (Betrag wird sofort vom Spieler abgezogen, kein Rückgeld).</summary>
        public void SetzeRichterBestechung(int betrag) => _bestechungRichter = ZieheBestechungAb(betrag);

        /// <summary>Bezahlt die Zeugen-Bestechung (Betrag wird sofort vom Spieler abgezogen, kein Rückgeld).</summary>
        public void SetzeZeugenBestechung(int betrag) => _bestechungZeugen = ZieheBestechungAb(betrag);

        private int ZieheBestechungAb(int betrag)
        {
            if (betrag <= 0)
                return 0;

            var spieler = SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetAktiverSpieler());
            betrag = Math.Min(betrag, spieler.GetTaler());
            spieler.ErhoeheTaler(-betrag);

            return betrag;
        }

        /// <summary>Ob in diesem Verfahren bestochen wurde (für die Offenlegung vor dem Urteil).</summary>
        public bool WurdeBestochen() => _bestechungRichter > 0 || _bestechungZeugen > 0;

        /// <summary>Die Offenlegung vor dem Urteil, dass in diesem Verfahren Bestechungsgelder geflossen sind.</summary>
        public string GetBestechungsOffenlegung()
            => "Hinter vorgehaltener Hand munkelt man, dass in diesem\nVerfahren Bestechungsgelder geflossen sind.";

        #endregion

        #region Zeugen (Issue #18)

        /// <summary>Wählt bis zu <see cref="ZeugenAnzahlMax"/> KI-Zeugen aus (weder Partei noch Richter).</summary>
        private void WaehleZeugen()
        {
            _zeugen = new List<int>();

            for (int id = SW.Statisch.GetMinKIID(); id < SW.Statisch.GetMaxKIID() && _zeugen.Count < ZeugenAnzahlMax; id++)
            {
                if (id == _verhandlung.GetAngeklagterID() || id == _verhandlung.GetKlaegerID())
                    continue;

                if (id == _verhandlung.GetRichterXID(0) || id == _verhandlung.GetRichterXID(1) || id == _verhandlung.GetRichterXID(2))
                    continue;

                if (SW.Dynamisch.GetKIwithID(id) == null)
                    continue;

                _zeugen.Add(id);
            }
        }

        /// <summary>
        /// Ermittelt die Aussagen aller Zeugen und legt damit ihren Beitrag zum Urteil (<c>_zeugenBonus</c>) fest:
        /// Jeder Zeuge sagt je nach Verhältnis für den (ihm näheren) Angeklagten oder gegen ihn aus – überzeugend
        /// bei großem Beziehungsunterschied, sonst schwach. Eine Bestechung zieht ihn (ab Schwelle sicher) auf die
        /// Seite des Bestechers und lässt ihn überzeugend auftreten. Muss nach der Bestechung aufgerufen werden.
        /// </summary>
        public List<ZeugenAussage> ErmittleZeugenAussagen()
        {
            var aussagen = new List<ZeugenAussage>();
            bool maennlich = SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID()).GetMaennlich();

            _zeugenBonus = 0;

            for (int i = 0; i < _zeugen.Count; i++)
            {
                var zeuge = SW.Dynamisch.GetKIwithID(_zeugen[i]);

                int zuAngeklagter = zeuge.GetBeziehungZuKIX(_verhandlung.GetAngeklagterID());
                int zuKlaeger = zeuge.GetBeziehungZuKIX(_verhandlung.GetKlaegerID());
                int differenz = zuAngeklagter - zuKlaeger;

                bool fuerAngeklagten = differenz >= 0;
                bool ueberzeugend = Math.Abs(differenz) >= ZeugeUeberzeugungsGrenze;

                // Bestechung zieht den Zeugen auf die Seite des Bestechers (ab Schwelle sicher).
                if (_bestechungZeugen > 0)
                {
                    int anteil = _bestechungZeugen / _zeugen.Count;
                    double wahrscheinlichkeit = Math.Min(1.0, (double)anteil / ZeugeSchwelle(i));

                    if (SW.Statisch.Rnd.NextDouble() < wahrscheinlichkeit)
                    {
                        fuerAngeklagten = _aktivIstAngeklagter;   // Bestecher-Seite
                        ueberzeugend = true;
                    }
                }

                int staerke = ueberzeugend ? ZeugeUeberzeugend : ZeugeSchwach;
                _zeugenBonus += fuerAngeklagten ? -staerke : staerke;   // für Angeklagten -> weniger schuldig

                aussagen.Add(new ZeugenAussage(ZeugenText(zeuge.GetKompletterName(), fuerAngeklagten, ueberzeugend, maennlich)));
            }

            return aussagen;
        }

        // Gesprochene Zeugen-Zitate für etwas Atmosphäre, je nach Richtung und Überzeugungskraft. Platzhalter
        // {er}/{ihn}/{ihm} werden je nach Geschlecht des Angeklagten ersetzt (siehe FuellePronomen).
        private static readonly string[] ZitateFuerUeberzeugend =
        {
            "Ich schwöre, dass {er} nicht {der_taeter} ist!",
            "Zur fraglichen Zeit war {er} nachweislich bei mir.",
            "Einen ehrbareren Menschen gibt es in der ganzen Stadt nicht!",
            "Diese Anschuldigungen sind frei erfunden, das sage ich Euch!"
        };

        private static readonly string[] ZitateFuerSchwach =
        {
            "Ich… ich glaube kaum, dass {er} so etwas täte.",
            "Mir schien {er} stets rechtschaffen, doch wer weiß…",
            "So recht vorstellen kann ich mir die Tat bei {ihm} nicht."
        };

        private static readonly string[] ZitateGegenUeberzeugend =
        {
            "Ich habe {ihn} genau gesehen!",
            "Mit eigenen Augen sah ich die Tat geschehen!",
            "Ein reines Gewissen hat {er} gewiss nicht, das sieht man {ihm} an!"
        };

        private static readonly string[] ZitateGegenSchwach =
        {
            "Ja, und mit einer schwarzen Katze habe ich {ihn} auch schon gesehen!",
            "Ganz geheuer war {er} mir noch nie…",
            "Man munkelt so einiges über {ihn}."
        };

        private static string ZeugenText(string name, bool fuerAngeklagten, bool ueberzeugend, bool maennlich)
        {
            string aussage;
            string[] zitate;

            if (fuerAngeklagten)
            {
                string desAngeklagten = maennlich ? "des Angeklagten" : "der Angeklagten";

                aussage = ueberzeugend
                    ? "beteuert mit Nachdruck die Unschuld " + desAngeklagten + "."
                    : "spricht zögerlich zugunsten " + desAngeklagten + ".";
                zitate = ueberzeugend ? ZitateFuerUeberzeugend : ZitateFuerSchwach;
            }
            else
            {
                string denAngeklagten = maennlich ? "den Angeklagten" : "die Angeklagte";
                string demAngeklagten = maennlich ? "dem Angeklagten" : "der Angeklagten";

                aussage = ueberzeugend
                    ? "belastet " + denAngeklagten + " mit einer schweren Aussage."
                    : "äußert vage Zweifel an " + demAngeklagten + ".";
                zitate = ueberzeugend ? ZitateGegenUeberzeugend : ZitateGegenSchwach;
            }

            string zitat = FuellePronomen(zitate[SW.Statisch.Rnd.Next(zitate.Length)], maennlich);

            return name + "\n" + aussage + "\n„" + zitat + "“";
        }

        private static string FuellePronomen(string zitat, bool maennlich)
        {
            return zitat
                .Replace("{er}", maennlich ? "er" : "sie")
                .Replace("{ihn}", maennlich ? "ihn" : "sie")
                .Replace("{ihm}", maennlich ? "ihm" : "ihr")
                .Replace("{der_taeter}", maennlich ? "der Täter" : "die Täterin");
        }

        #endregion

        #region Plädoyers (Issue #18)

        /// <summary>
        /// Das Schlussplädoyer der Anklage. Der Ton richtet sich nach der Beweislast (tatsächliche Delikte
        /// plus gesammelte Beweise): von erdrückend über deutlich und dünn bis haltlos. Reine Darstellung.
        /// </summary>
        public string GetAnklageplaedoyer()
        {
            int staerke = _summeVerbrechen + _beweise;

            if (staerke >= 5)
                return "Der Ankläger erhebt sich:\n\"Die Beweise sind erdrückend! Solches Treiben\nschreit zum Himmel und verlangt eine harte Strafe!\"";

            if (staerke >= 2)
                return "Der Ankläger führt aus:\n\"Die Vorwürfe wiegen schwer, und die Indizien\nsprechen eine deutliche Sprache.\"";

            if (staerke >= 1)
                return "Der Ankläger mahnt:\n\"Auch wenn die Beweislage dünn ist, darf ein\nsolcher Verdacht nicht ungeprüft bleiben.\"";

            return "Der Ankläger windet sich:\n\"Handfeste Beweise fehlen wohl, doch mein\nGefühl trügt mich nur selten...\"";
        }

        /// <summary>
        /// Das Schlussplädoyer der Verteidigung. Der Ton richtet sich nach dem Ansehen des Angeklagten;
        /// ein hohes Ansehen wurde beim Start der Verhandlung bereits als <c>_plaedoyerBonus</c> zugunsten
        /// des Angeklagten verbucht (siehe StarteVerhandlung).
        /// </summary>
        public string GetVerteidigungsplaedoyer()
        {
            var angeklagter = SW.Dynamisch.GetSpWithID(_verhandlung.GetAngeklagterID());
            bool maennlich = angeklagter.GetMaennlich();
            int ansehen = angeklagter.GetAnsehen();

            string meinMandant = maennlich ? "meines Mandanten" : "meiner Mandantin";
            string derMandant = maennlich ? "Mein Mandant" : "Meine Mandantin";

            if (ansehen >= AnsehenHoch)
                return "Der Verteidiger erhebt die Stimme:\n\"Seht das makellose Ansehen " + meinMandant + "!\nWer solchen Leumund genießt, tut derlei nicht.\"";

            if (ansehen >= AnsehenMittel)
            {
                string geachtet = maennlich ? "ein geachteter Bürger" : "eine geachtete Bürgerin";
                return "Der Verteidiger bittet:\n\"" + derMandant + " ist " + geachtet + " –\ngebt dem guten Ruf das gebührende Gewicht.\"";
            }

            return "Der Verteidiger beteuert:\n\"" + derMandant + " beteuert die Unschuld.\nVerurteilt nicht auf bloßen Verdacht hin.\"";
        }

        #endregion

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
            int faktor = (_summeVerbrechen + _beweise) * BeweisGewicht + _aussageUrteilsBonus + _zeugenBonus + _plaedoyerBonus;

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

            bool schuldig = !(SW.Statisch.Rnd.Next(0, obergrenze) > faktor);

            // Bestechung (Issue #18): Mit der Wahrscheinlichkeit Anteil/Schwelle stimmt der Richter im Sinne
            // des Bestechers (ab der Schwelle sicher). Der Angeklagte besticht auf Freispruch, der Kläger auf
            // Verurteilung. Die Bestechung überschreibt nur zugunsten des Bestechers – nie zu seinem Nachteil.
            if (_bestechungRichter > 0)
            {
                int anteil = _bestechungRichter / RichterAnzahl;
                double wahrscheinlichkeit = Math.Min(1.0, (double)anteil / RichterSchwelle(i));

                if (SW.Statisch.Rnd.NextDouble() < wahrscheinlichkeit)
                    return !_aktivIstAngeklagter;   // Angeklagter -> nicht schuldig (false), Kläger -> schuldig (true)
            }

            return schuldig;
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

    /// <summary>Eine wählbare Bestechungsstufe mit Anzeigetext (inkl. Betrag) und dem Taler-Betrag.</summary>
    public class BestechungOption
    {
        public string ButtonText { get; }
        public int Betrag { get; }

        public BestechungOption(string buttonText, int betrag)
        {
            ButtonText = buttonText;
            Betrag = betrag;
        }
    }

    /// <summary>Die vor Gericht vorgetragene Aussage eines Zeugen (fertig formulierter Anzeigetext).</summary>
    public class ZeugenAussage
    {
        public string Text { get; }

        public ZeugenAussage(string text)
        {
            Text = text;
        }
    }
}
