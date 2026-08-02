using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Die komplexeren Cheats aus der WinForms-Cheatbox, die mehr als einen einfachen Setter benötigen:
    /// ein Amt einer KI abnehmen, ein Haus bauen und sich selbst verklagen lassen. Die einfachen Cheats
    /// (Taler, Ansehen, Handelsrechte, Kind, Altern, Amtsniederlegung) laufen direkt über die Spieler-Setter.
    /// Alle Aktionen betreffen den aktiven menschlichen Spieler.
    /// </summary>
    public class CheatManager
    {
        #region Combobox-Daten

        /// <summary>Die drei Amtsstufen (Stadt, Land, Reich).</summary>
        [PublicAPI]
        public List<string> GetAmtsstufen()
        {
            var stufen = new List<string>();

            for (int i = 0; i < 3; i++)
                stufen.Add(SW.Statisch.GetStufenNameX(i));

            return stufen;
        }

        /// <summary>Die Gebiete der angegebenen Amtsstufe (Index 0 = erstes Gebiet, Gebiets-ID = Index + 1).</summary>
        [PublicAPI]
        public List<string> GetGebiete(int stufe)
        {
            var gebiete = new List<string>();
            int max = GetMaxGebietID(stufe);

            for (int i = 1; i < max; i++)
                gebiete.Add(SW.Dynamisch.GetGebietwithID(i, stufe).GetGebietsName());

            return gebiete;
        }

        /// <summary>Die Ämter der angegebenen Amtsstufe (Index 0 = erstes Amt der Stufe).</summary>
        [PublicAPI]
        public List<string> GetAemter(int stufe)
        {
            var aemter = new List<string>();
            GetAmtIdBereich(stufe, out int minId, out int maxId);

            for (int i = minId; i < maxId; i++)
                aemter.Add(SW.Statisch.GetAmtwithID(i).GetAmtsname(true));

            return aemter;
        }

        /// <summary>Die Städte (Index 0 = erste Stadt, Stadt-ID = Index + 1).</summary>
        [PublicAPI]
        public List<string> GetStaedte()
        {
            var staedte = new List<string>();

            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
                staedte.Add(SW.Dynamisch.GetStadtwithID(i).GetGebietsName());

            return staedte;
        }

        /// <summary>Die verfügbaren Haustypen (Index 0 = erster Haustyp, Haus-ID = Index + 1).</summary>
        [PublicAPI]
        public List<string> GetHaustypen()
        {
            var haeuser = new List<string>();

            for (int i = 1; i < SW.Statisch.GetMaxHausID(); i++)
                haeuser.Add(SW.Statisch.GetHaus(i).Name);

            return haeuser;
        }

        private static int GetMaxGebietID(int stufe)
        {
            if (stufe == 1)
                return SW.Statisch.GetMaxLandID();

            if (stufe == 2)
                return SW.Statisch.GetMaxReichID();

            return SW.Statisch.GetMaxStadtID();
        }

        private static void GetAmtIdBereich(int stufe, out int minId, out int maxId)
        {
            if (stufe == 1)
            {
                minId = SW.Statisch.GetMaxAmtStadtID();
                maxId = SW.Statisch.GetMaxAmtLandID();
            }
            else if (stufe == 2)
            {
                minId = SW.Statisch.GetMaxAmtLandID();
                maxId = SW.Statisch.GetMaxAmtID();
            }
            else
            {
                minId = 0;
                maxId = SW.Statisch.GetMaxAmtStadtID();
            }
        }

        #endregion

        /// <summary>
        /// Übernimmt für den aktiven Spieler das gewählte Amt (Stufe/Gebiet/Amt jeweils als Combobox-Index).
        /// Gehört das Amt einer KI, wird das bisherige Amt des Spielers mit ihr getauscht. Liefert die Meldung.
        /// </summary>
        [PublicAPI]
        public string UebernehmeAmt(int stufe, int gebietIndex, int amtIndex)
        {
            if (amtIndex < 0)
                return "Bitte ein Amt auswählen";

            int gebiet = gebietIndex + 1;
            int amt = AmtIdAusIndex(stufe, amtIndex);
            int aktiv = SW.Dynamisch.GetAktiverSpieler();

            int aktuellerInhaber = SW.Dynamisch.GetGebietwithID(gebiet, stufe).GetAmtX(amt);

            if (aktuellerInhaber >= SW.Statisch.GetMinKIID())
            {
                // Das Amt gehört einer KI: tauschen.
                int altGebiet = SW.Dynamisch.GetHumWithID(aktiv).GetAmtGebiet();
                int altAmt = SW.Dynamisch.GetHumWithID(aktiv).GetAmtID();
                int altStufe = SW.Dynamisch.GetStufeVonAmtmitIDx(altAmt);

                int amtStufe = SW.Dynamisch.GetStufeVonAmtmitIDx(amt);
                SW.Dynamisch.AmtAufStufeXGebietYidZanWvergeben(amtStufe, gebiet, amt, aktiv);

                SW.Dynamisch.GetSpWithID(aktuellerInhaber).SetAmt(altAmt, altGebiet);

                if (altAmt != 0)
                    SW.Dynamisch.GetGebietwithID(altGebiet, altStufe).SetAmtXtoY(altAmt, aktuellerInhaber);
                else
                    SW.Dynamisch.GetSpWithID(aktuellerInhaber).SetAmt(0, 0);

                return "Ihr seid nun " + SW.Statisch.GetAmtwithID(amt).GetAmtsname(true);
            }

            if (aktuellerInhaber == 0)
            {
                // Das Amt ist unbesetzt.
                int amtStufe = SW.Dynamisch.GetStufeVonAmtmitIDx(amt);
                SW.Dynamisch.AmtAufStufeXGebietYidZanWvergeben(amtStufe, gebiet, amt, aktiv);

                return "Ihr seid nun " + SW.Statisch.GetAmtwithID(amt).GetAmtsname(true);
            }

            return "Das gewählte Amt gehört einem menschlichen Mitspieler";
        }

        private static int AmtIdAusIndex(int stufe, int amtIndex)
        {
            // Der Combobox-Index ist relativ zur Stufe; auf die globale Amts-ID umrechnen (wie im Original).
            if (stufe == 1)
                return amtIndex + SW.Statisch.GetMaxAmtStadtID();

            if (stufe == 2)
                return amtIndex + SW.Statisch.GetMaxAmtLandID();

            return amtIndex;
        }

        /// <summary>Baut für den aktiven Spieler in der gewählten Stadt ein Haus des gewählten Typs (jeweils Combobox-Index).</summary>
        [PublicAPI]
        public void BaueHaus(int stadtIndex, int hausTypIndex)
        {
            int stadtId = stadtIndex + 1;
            int hausId = hausTypIndex + 1;

            var haus = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpielerHatHausVonStadtAnArraystelle(stadtId);
            haus.SetHausID(hausId);
            haus.SetStadtID(stadtId);
        }

        /// <summary>
        /// Lässt den aktiven Spieler im nächsten Jahr verklagen: bucht ein Delikt, wählt einen missgünstigen
        /// KI-Kläger und drei Richter und setzt eine Gerichtsverhandlung auf. Liefert die Meldung.
        /// </summary>
        [PublicAPI]
        public string LasseVerklagen()
        {
            int aktiv = SW.Dynamisch.GetAktiverSpieler();

            if (SW.Dynamisch.GetHumWithID(aktiv).GetWirdBereitsVerklagt())
                return "Ihr werdet diese Runde bereits verklagt";

            SW.Dynamisch.GetHumWithID(aktiv).SetBegingVerbrechenX(0, 20);

            int minAmtId = SW.Dynamisch.GetMinGegnerAmtID(aktiv);
            int maxAmtId = SW.Dynamisch.GetMaxGegnerAmtID(aktiv);
            int klaeger = SW.Dynamisch.GetKIthatDislikesHumX(aktiv, minAmtId, maxAmtId);

            int gebietsId = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());

            // Ist der Spieler zufällig Richter in dieser Stadt, ein anderes Gebiet wählen.
            if (SW.Dynamisch.GetHumWithID(aktiv).GetAmtID() == 5)
            {
                while (SW.Dynamisch.GetHumWithID(aktiv).GetAmtGebiet() == gebietsId)
                    gebietsId = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
            }

            int klageId = SW.Dynamisch.GetEmptyGerichtsverhandlung();

            int r1 = SW.Dynamisch.GetStadtwithID(gebietsId).GetRichter();

            if (r1 == 0)
                r1 = FindeRichter(aktiv);

            int r2 = FindeRichter(aktiv, r1);
            int r3 = FindeRichter(aktiv, r1, r2);

            SW.Dynamisch.GetGerichtsverhandlungX(klageId).SetAll(r1, r2, r3, gebietsId, 0, aktiv, klaeger);
            SW.Dynamisch.GetHumWithID(aktiv).SetWirdBereitsVerklagt(true);

            return "Ein Kläger bereitet einen Prozess gegen Euch vor.\nIm nächsten Jahr müsst Ihr Euch vor Gericht verantworten.";
        }

        /// <summary>Sucht einen Stadtrichter, der weder der Spieler noch einer der bereits gewählten Richter ist.</summary>
        private static int FindeRichter(int aktiverSpieler, int ausgeschlossen1 = 0, int ausgeschlossen2 = 0)
        {
            for (int versuch = 0; versuch < 1000; versuch++)
            {
                int stadt = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
                int richter = SW.Dynamisch.GetStadtwithID(stadt).GetRichter();

                if (richter != 0 && richter != aktiverSpieler && richter != ausgeschlossen1 && richter != ausgeschlossen2)
                    return richter;
            }

            return 0;
        }
    }
}
