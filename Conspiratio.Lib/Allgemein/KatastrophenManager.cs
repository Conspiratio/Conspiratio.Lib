using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Art einer Katastrophe. Die Reihenfolge entspricht den Anfälligkeiten je Stadt.</summary>
    public enum EnumKatastrophe
    {
        Sturm = 0,
        Flut = 1,
        Brand = 2,
        Erdbeben = 3,
        Pest = 4
    }

    /// <summary>Wie weit eine Katastrophe reicht.</summary>
    public enum EnumKatastrophenumfang
    {
        Stadt,
        Grafschaft,
        Reich
    }

    /// <summary>Ergebnis der Katastrophenprüfung eines Jahres.</summary>
    public class KatastrophenErgebnis
    {
        /// <summary>Ist überhaupt etwas passiert? In den meisten Jahren nicht.</summary>
        public bool Eingetreten { get; set; }

        public EnumKatastrophe Art { get; set; }

        public EnumKatastrophenumfang Umfang { get; set; }

        /// <summary>Die betroffenen Städte.</summary>
        public List<int> BetroffeneStaedte { get; set; } = new List<int>();

        /// <summary>Erzählende Meldung für die Rundenende-Ereignisse.</summary>
        public string Meldung { get; set; }

        /// <summary>Was die Katastrophe den menschlichen Spielern gekostet hat (je Spieler eine Zeile).</summary>
        public List<string> SpielerMeldungen { get; set; } = new List<string>();
    }

    /// <summary>
    /// Katastrophen (WinForms-Issue #37, Vorbild „Die Fugger 2"): Sturm, Flut, Brand, Erdbeben und Pest
    /// suchen die Städte heim. Sie treten selten auf, wiegen dann aber schwer – sie kosten Einwohner und
    /// Reichtum, vernichten Warenvorräte und treiben in der Folge die Preise, und sie greifen auch den
    /// Besitz der Spieler an.
    ///
    /// Welche Stadt welche Katastrophe treffen kann, steht seit jeher in den Spieldaten: Jede Stadt trägt
    /// je Art eine Anfälligkeit von 0–100 (<see cref="Gameplay.Gebiete.Stadt.GetKatastrophen"/>). Eine
    /// Stadt mit 0 wird von dieser Art nie heimgesucht – Hafenstädte ersaufen, Bergstädte beben.
    /// </summary>
    public class KatastrophenManager
    {
        /// <summary>Wahrscheinlichkeit je Jahr in Prozent – im Schnitt etwa alle vier bis fünf Jahre.</summary>
        public const int JahresChance = 22;

        // Wie weit die Katastrophe reicht, sobald eine eintritt (Rest = Stadt).
        private const int ChanceReich = 15;
        private const int ChanceGrafschaft = 35;

        // Auswirkungen auf eine betroffene Stadt (Prozent bzw. absolute Punkte).
        private const int EinwohnerverlustMin = 8;
        private const int EinwohnerverlustMax = 21;   // Rnd-Obergrenze (exklusiv) → 8..20 %
        private const int VorratsverlustMin = 20;
        private const int VorratsverlustMax = 41;     // → 20..40 %
        private const int PreisanstiegMin = 2;
        private const int PreisanstiegMax = 5;        // → 2..4 je Ware
        private const int ReichtumsverlustMax = 2;    // 1..2 Punkte

        // Persönliche Folgen für Spieler mit Besitz in einer betroffenen Stadt.
        private const int HauschadenMin = 10;
        private const int HauschadenMax = 26;         // → 10..25 Prozentpunkte Zustand
        private const int PestGesundheitMin = 5;
        private const int PestGesundheitMax = 16;     // → 5..15

        /// <summary>Die Pest wütet unter den Menschen stärker als unter den Waren.</summary>
        private const int PestEinwohnerFaktor = 2;

        /// <summary>
        /// Prüft, ob in diesem Jahr eine Katastrophe eintritt, und wickelt sie gegebenenfalls ab.
        /// Wird zum Rundenende aufgerufen.
        /// </summary>
        [PublicAPI]
        public KatastrophenErgebnis FuehreKatastrophenDurch()
        {
            if (SW.Statisch.Rnd.Next(0, 100) >= JahresChance)
                return new KatastrophenErgebnis { Eingetreten = false };

            EnumKatastrophe art = WaehleArt();
            EnumKatastrophenumfang umfang = WaehleUmfang();

            var ergebnis = new KatastrophenErgebnis { Eingetreten = true, Art = art, Umfang = umfang };
            string ort = BestimmeBetroffeneStaedte(art, umfang, ergebnis.BetroffeneStaedte);

            // Keine Stadt ist für diese Art anfällig – dann bleibt es dieses Jahr ruhig.
            if (ergebnis.BetroffeneStaedte.Count == 0)
                return new KatastrophenErgebnis { Eingetreten = false };

            foreach (int stadtId in ergebnis.BetroffeneStaedte)
                TrifftStadt(stadtId, art, ergebnis.SpielerMeldungen);

            ergebnis.Meldung = BaueMeldung(art, umfang, ort, ergebnis.BetroffeneStaedte.Count);

            return ergebnis;
        }

        /// <summary>Wählt die Art gewichtet nach der Anfälligkeit aller Städte.</summary>
        private static EnumKatastrophe WaehleArt()
        {
            int anzahlArten = SW.Statisch.GetMaxKatastrohpen();
            var gewichte = new int[anzahlArten];
            int summe = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                int[] anfaelligkeit = SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen();

                for (int art = 0; art < anzahlArten; art++)
                {
                    gewichte[art] += anfaelligkeit[art];
                    summe += anfaelligkeit[art];
                }
            }

            if (summe <= 0)
                return EnumKatastrophe.Sturm;

            int wurf = SW.Statisch.Rnd.Next(0, summe);

            for (int art = 0; art < anzahlArten; art++)
            {
                wurf -= gewichte[art];
                if (wurf < 0)
                    return (EnumKatastrophe)art;
            }

            return EnumKatastrophe.Sturm;
        }

        private static EnumKatastrophenumfang WaehleUmfang()
        {
            int wurf = SW.Statisch.Rnd.Next(0, 100);

            if (wurf < ChanceReich)
                return EnumKatastrophenumfang.Reich;
            if (wurf < ChanceReich + ChanceGrafschaft)
                return EnumKatastrophenumfang.Grafschaft;

            return EnumKatastrophenumfang.Stadt;
        }

        /// <summary>
        /// Sammelt die betroffenen Städte und liefert die Ortsbezeichnung für die Meldung. Nur Städte mit
        /// einer Anfälligkeit größer 0 werden heimgesucht – auch bei Grafschaft und Reich.
        /// </summary>
        private static string BestimmeBetroffeneStaedte(EnumKatastrophe art, EnumKatastrophenumfang umfang, List<int> betroffene)
        {
            var anfaellige = new List<int>();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                if (SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen()[(int)art] > 0)
                    anfaellige.Add(stadtId);
            }

            if (anfaellige.Count == 0)
                return "";

            if (umfang == EnumKatastrophenumfang.Reich)
            {
                betroffene.AddRange(anfaellige);
                return "im ganzen Reich";
            }

            // Eine anfällige Stadt auslosen, gewichtet nach ihrer Anfälligkeit.
            int gewaehlt = WaehleStadtGewichtet(anfaellige, art);

            if (umfang == EnumKatastrophenumfang.Stadt)
            {
                betroffene.Add(gewaehlt);
                return SW.Dynamisch.GetStadtwithID(gewaehlt).GetGebietsName();
            }

            // Grafschaft: alle anfälligen Städte des Landes, in dem die ausgeloste Stadt liegt.
            int landId = SW.Dynamisch.GetStadtwithID(gewaehlt).GetLandID();

            foreach (int stadtId in anfaellige)
            {
                if (SW.Dynamisch.GetStadtwithID(stadtId).GetLandID() == landId)
                    betroffene.Add(stadtId);
            }

            return "in der Grafschaft " + SW.Dynamisch.GetLandWithID(landId).GetGebietsName();
        }

        private static int WaehleStadtGewichtet(List<int> anfaellige, EnumKatastrophe art)
        {
            int summe = 0;

            foreach (int stadtId in anfaellige)
                summe += SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen()[(int)art];

            int wurf = SW.Statisch.Rnd.Next(0, summe);

            foreach (int stadtId in anfaellige)
            {
                wurf -= SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen()[(int)art];
                if (wurf < 0)
                    return stadtId;
            }

            return anfaellige[0];
        }

        /// <summary>Wendet die Folgen auf eine Stadt und auf den dortigen Besitz der Spieler an.</summary>
        private static void TrifftStadt(int stadtId, EnumKatastrophe art, List<string> spielerMeldungen)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);

            // Einwohner: Die Pest fordert die meisten Opfer.
            int einwohnerverlust = SW.Statisch.Rnd.Next(EinwohnerverlustMin, EinwohnerverlustMax);
            if (art == EnumKatastrophe.Pest)
                einwohnerverlust *= PestEinwohnerFaktor;

            int neueEinwohner = stadt.GetEinwohner() - stadt.GetEinwohner() * einwohnerverlust / 100;
            stadt.SetEinwohnerAufX(neueEinwohner < 0 ? 0 : neueEinwohner);

            // Reichtum: Wiederaufbau kostet.
            int neuerReichtum = stadt.GetReichtum() - SW.Statisch.Rnd.Next(1, ReichtumsverlustMax + 1);
            stadt.SetReichtumToX(neuerReichtum < 0 ? 0 : neuerReichtum);

            // Warenvorräte werden vernichtet, und die Knappheit treibt die Preise.
            int vorratsverlust = SW.Statisch.Rnd.Next(VorratsverlustMin, VorratsverlustMax);

            for (int rohId = 1; rohId < SW.Statisch.GetMaxRohID(); rohId++)
            {
                stadt.ErhoeheRohstoffVorratWithIDXByY(rohId, -(stadt.GetRohstoffIDXVorrat(rohId) * vorratsverlust / 100));
                stadt.ErhoeheRohstoffPreisVonIDXByY(rohId, SW.Statisch.Rnd.Next(PreisanstiegMin, PreisanstiegMax));
            }

            TrifftSpieler(stadtId, art, vorratsverlust, spielerMeldungen);
        }

        /// <summary>Schädigt den Besitz der menschlichen Spieler in der betroffenen Stadt.</summary>
        private static void TrifftSpieler(int stadtId, EnumKatastrophe art, int vorratsverlust, List<string> spielerMeldungen)
        {
            string stadtName = SW.Dynamisch.GetStadtwithID(stadtId).GetGebietsName();

            for (int spielerId = 1; spielerId < SW.Statisch.GetMinKIID(); spielerId++)
            {
                var spieler = SW.Dynamisch.GetHumWithID(spielerId);

                if (spieler == null)
                    continue;

                var verluste = new List<string>();

                // Eingelagerte Waren
                int vernichteteWaren = 0;

                for (int rohId = 1; rohId < SW.Statisch.GetMaxRohID(); rohId++)
                {
                    int bestand = spieler.GetStadtRohstoffAnzahl(stadtId, rohId);
                    int verlust = bestand * vorratsverlust / 100;

                    if (verlust > 0)
                    {
                        spieler.VeraenderStadtRohstoffAnzahl(stadtId, rohId, -verlust);
                        vernichteteWaren += verlust;
                    }
                }

                if (vernichteteWaren > 0)
                    verluste.Add(vernichteteWaren + " Einheiten Eurer eingelagerten Waren");

                // Wohnsitze nehmen Schaden. Das Hausarray ist nach Stadt-ID indiziert – je Stadt höchstens
                // ein Wohnsitz, daher genügt ein direkter Zugriff.
                var haus = spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId);
                bool hatWohnsitz = haus != null && haus.GetHausID() != 0;

                if (hatWohnsitz)
                {
                    int schaden = SW.Statisch.Rnd.Next(HauschadenMin, HauschadenMax);
                    int neuerZustand = haus.ZustandInProzent - schaden;
                    haus.ZustandInProzent = neuerZustand < 0 ? 0 : neuerZustand;

                    verluste.Add("Euer Wohnsitz wurde beschädigt");
                }

                // Die Pest greift den an, der vor Ort einen Wohnsitz hat.
                if (art == EnumKatastrophe.Pest && hatWohnsitz)
                {
                    spieler.ErhoeheGesundheit(-SW.Statisch.Rnd.Next(PestGesundheitMin, PestGesundheitMax));
                    verluste.Add("die Seuche hat auch Euch geschwächt");
                }

                if (verluste.Count > 0)
                    spielerMeldungen.Add(spieler.GetName() + " verliert in " + stadtName + ": " + string.Join(", ", verluste) + ".");
            }
        }

        private static string BaueMeldung(EnumKatastrophe art, EnumKatastrophenumfang umfang, string ort, int anzahlStaedte)
        {
            string einleitung;

            switch (art)
            {
                case EnumKatastrophe.Sturm:
                    einleitung = "Ein verheerender Sturm fegt";
                    break;
                case EnumKatastrophe.Flut:
                    einleitung = "Eine gewaltige Flut überschwemmt";
                    break;
                case EnumKatastrophe.Brand:
                    einleitung = "Ein Großfeuer wütet";
                    break;
                case EnumKatastrophe.Erdbeben:
                    einleitung = "Ein schweres Erdbeben erschüttert";
                    break;
                default:
                    einleitung = "Die Pest wütet";
                    break;
            }

            string wo = umfang == EnumKatastrophenumfang.Stadt ? "über " + ort : ort;

            string folgen = anzahlStaedte == 1
                ? "\n\nDie Stadt verliert Einwohner und Wohlstand, Vorräte sind vernichtet, und die Preise ziehen an."
                : "\n\n" + anzahlStaedte + " Städte verlieren Einwohner und Wohlstand, Vorräte sind vernichtet, und die Preise ziehen an.";

            return einleitung + " " + wo + "." + folgen;
        }
    }
}
