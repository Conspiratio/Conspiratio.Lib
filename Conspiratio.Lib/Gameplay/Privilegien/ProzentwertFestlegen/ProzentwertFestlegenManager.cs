using System;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.ProzentwertFestlegen
{
    /// <summary>
    /// Kapselt die Logik von ProzentwertFestlegenForm für alle fünf Prozentwert-Arten
    /// (Umsatzsteuer, Zollsatz, Sicherheit/Tarnung, Zustand, Kapazität). Es gibt zwei Betriebsarten:
    /// Live-Werte (Umsatzsteuer/Zollsatz) werden bei jeder Wertänderung sofort übernommen; Kostenmodi
    /// (die drei Stützpunkt-Verbesserungen) zeigen Kosten an und werden erst per Auftrag ausgeführt.
    /// </summary>
    public class ProzentwertFestlegenManager
    {
        private readonly ProzentwertArt _art;
        private readonly int _zielIndex; // zielStuetzpunktID - 1

        public ProzentwertFestlegenManager(ProzentwertArt art, int zielStuetzpunktID = 0)
        {
            _art = art;
            _zielIndex = zielStuetzpunktID - 1;
        }

        public ProzentwertArt Art => _art;

        /// <summary>
        /// Kostenmodus (Stützpunkt-Verbesserungen) mit Kostenanzeige und Auftrag-Button. Andernfalls
        /// wird der Wert live übernommen (Umsatzsteuer/Zollsatz).
        /// </summary>
        public bool IstKostenModus =>
            _art == ProzentwertArt.SicherheitTarnungStuetzpunkt ||
            _art == ProzentwertArt.ZustandStuetzpunkt ||
            _art == ProzentwertArt.KapazitaetStuetzpunkt;

        private Stuetzpunkt Ziel => SW.Dynamisch.GetStuetzpunkte()[_zielIndex];

        private Stadt AmtStadt =>
            SW.Dynamisch.GetStadtwithID(SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetAmtGebiet());

        #region Anzeige

        public string GetLabelText()
        {
            switch (_art)
            {
                case ProzentwertArt.UmsatzsteuerStadt:
                    return "Als Bürgermeister von " + AmtStadt.GetGebietsName() +
                           " könnt Ihr die Umsatzsteuer festlegen. Die Umsatzsteuer soll";
                case ProzentwertArt.ZollsatzZollburg:
                    return "Als Besitzer von " + SW.Dynamisch.GetZollburgWithIDx(_zielIndex).Name +
                           " könnt Ihr den Zollsatz festlegen. Der Zollsatz soll";
                case ProzentwertArt.SicherheitTarnungStuetzpunkt:
                    return "Verbessern: Auf welchen Wert wollt Ihr die " + Ziel.SicherheitTarnungAlsString() +
                           " von " + Ziel.Name + " erhöhen? Sie soll";
                case ProzentwertArt.ZustandStuetzpunkt:
                    return "Reparieren: Um welchen Wert wollt Ihr den Zustand von " + Ziel.Name +
                           " verbessern? Der Zustand soll";
                case ProzentwertArt.KapazitaetStuetzpunkt:
                    return $"Ausbauen: Um welche Anzahl wollt Ihr die {GetKapazitaetBezeichnung()} von " +
                           $"{Ziel.Name} erweitern? Es soll insgesamt";
                default:
                    return string.Empty;
            }
        }

        public string GetSuffixText() =>
            _art == ProzentwertArt.KapazitaetStuetzpunkt ? "geben" : "% betragen";

        private string GetKapazitaetBezeichnung()
        {
            if (Ziel.Art == EnumStuetzpunktArt.Zollburg)
                return ((Zollburg)Ziel).KapazitaetBezeichnung;
            if (Ziel.Art == EnumStuetzpunktArt.Raeuberlager)
                return ((Raeuberlager)Ziel).KapazitaetBezeichnung;
            return "Unterkünfte";
        }

        #endregion

        #region NumericButton-Konfiguration

        public int GetMinWert()
        {
            switch (_art)
            {
                case ProzentwertArt.UmsatzsteuerStadt: return Convert.ToInt32(SW.Statisch.GetMinUmsatzsteuer() * 100);
                case ProzentwertArt.ZollsatzZollburg: return Convert.ToInt32(SW.Statisch.GetMinZollsatz() * 100);
                case ProzentwertArt.SicherheitTarnungStuetzpunkt: return Ziel.SicherheitTarnungInProzent;
                case ProzentwertArt.ZustandStuetzpunkt: return Ziel.ZustandInProzent;
                case ProzentwertArt.KapazitaetStuetzpunkt: return Ziel.Kapazitaet;
                default: return 0;
            }
        }

        /// <summary>
        /// Der aktuell eingestellte Wert (Startwert des Eingabefeldes). Bei den Stützpunkt-Modi ist
        /// dies zugleich das Minimum; bei Umsatzsteuer/Zollsatz der aktuelle Steuer-/Zollsatz.
        /// </summary>
        public int GetStartWert()
        {
            switch (_art)
            {
                case ProzentwertArt.UmsatzsteuerStadt: return Convert.ToInt32(AmtStadt.GetUmsatzsteuer() * 100);
                case ProzentwertArt.ZollsatzZollburg: return Convert.ToInt32(SW.Dynamisch.GetZollburgWithIDx(_zielIndex).Zoll * 100);
                default: return GetMinWert(); // Stützpunkt-Modi: Start = aktueller Wert = Minimum
            }
        }

        public int GetMaxWert()
        {
            switch (_art)
            {
                case ProzentwertArt.UmsatzsteuerStadt: return Convert.ToInt32(SW.Statisch.GetMaxUmsatzsteuer() * 100);
                case ProzentwertArt.ZollsatzZollburg: return Convert.ToInt32(SW.Statisch.GetMaxZollsatz() * 100);
                case ProzentwertArt.SicherheitTarnungStuetzpunkt: return 100;
                case ProzentwertArt.ZustandStuetzpunkt: return 100;
                case ProzentwertArt.KapazitaetStuetzpunkt: return Ziel.MaximaleKapazitaet;
                default: return 0;
            }
        }

        public int GetMaximaleStellen() =>
            (_art == ProzentwertArt.UmsatzsteuerStadt || _art == ProzentwertArt.ZollsatzZollburg) ? 2 : 3;

        #endregion

        #region Live-Werte (Umsatzsteuer/Zollsatz)

        /// <summary>Übernimmt bei Live-Werten (Umsatzsteuer/Zollsatz) den Wert sofort.</summary>
        public void SetzeWertLive(int wert)
        {
            double dProzentwert = wert / 100.0;

            if (_art == ProzentwertArt.UmsatzsteuerStadt)
                AmtStadt.SetUmsatzsteuerAufX(dProzentwert);
            else if (_art == ProzentwertArt.ZollsatzZollburg)
                SW.Dynamisch.GetZollburgWithIDx(_zielIndex).Zoll = dProzentwert;
        }

        #endregion

        #region Kostenmodi (Stützpunkt-Verbesserungen)

        /// <summary>Berechnet die Kosten, um den Zielwert zu erreichen (0, wenn nicht erhöht wird).</summary>
        public int BerechneKosten(int wert)
        {
            switch (_art)
            {
                case ProzentwertArt.SicherheitTarnungStuetzpunkt:
                    return wert > Ziel.SicherheitTarnungInProzent
                        ? Ziel.BerechneKostenSicherheitTarnung(wert - Ziel.SicherheitTarnungInProzent) : 0;
                case ProzentwertArt.ZustandStuetzpunkt:
                    return wert > Ziel.ZustandInProzent
                        ? Ziel.BerechneKostenZustand(wert - Ziel.ZustandInProzent) : 0;
                case ProzentwertArt.KapazitaetStuetzpunkt:
                    return wert > Ziel.Kapazitaet
                        ? Ziel.BerechneKostenKapazitaet(wert - Ziel.Kapazitaet) : 0;
                default:
                    return 0;
            }
        }

        /// <summary>Prüft (ohne Nebenwirkung), ob der aktive Spieler die Kosten tragen kann.</summary>
        public bool KannBezahlen(int kosten) =>
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetTaler() >= kosten;

        /// <summary>Meldungstext, wenn das Geld für die Kosten nicht reicht (wie im Original).</summary>
        public string GetNichtGenugGoldMeldung(int kosten) =>
            "Ihr besitzt die " + kosten.ToStringGeld(false) + "\n Taler für dieses\nVorhaben nicht.";

        /// <summary>
        /// Führt die Stützpunkt-Verbesserung aus: übernimmt den Zielwert, zieht die Kosten ab und
        /// liefert die Erfolgsmeldung. Vorher müssen Kosten &gt; 0 und Bezahlbarkeit sichergestellt sein.
        /// </summary>
        public string FuehreAuftragAus(int wert, int kosten)
        {
            var spieler = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler());

            switch (_art)
            {
                case ProzentwertArt.SicherheitTarnungStuetzpunkt:
                    Ziel.SicherheitTarnungInProzent = wert;
                    spieler.ErhoeheTaler(-kosten);
                    return "Eure Investition hat sich bezahlt gemacht und die " + Ziel.SicherheitTarnungAlsString() +
                           " von " + Ziel.Name + " hat sich auf " + wert + " % erhöht.";
                case ProzentwertArt.ZustandStuetzpunkt:
                    Ziel.ZustandInProzent = wert;
                    spieler.ErhoeheTaler(-kosten);
                    return "Lasst die Handwerker kommen! Der Zustand von " + Ziel.Name +
                           " hat sich aufgrund Eurer Renovierungsarbeiten auf " + wert + " % erhöht.";
                case ProzentwertArt.KapazitaetStuetzpunkt:
                    Ziel.Kapazitaet = wert;
                    spieler.ErhoeheTaler(-kosten);
                    return "Lasst die Arbeiter kommen! Die Kapazität von " + Ziel.Name +
                           " hat sich aufgrund Eurer Bauarbeiten auf " + wert + " erhöht.";
                default:
                    return null;
            }
        }

        #endregion
    }
}
