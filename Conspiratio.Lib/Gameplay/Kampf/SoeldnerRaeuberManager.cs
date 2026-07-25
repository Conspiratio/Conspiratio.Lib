using System.Threading.Tasks;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>Anzeigedaten eines Stützpunkts für den Kauf-Dialog.</summary>
    public class StuetzpunktKaufInfo
    {
        public string Name { get; }
        public string Beschreibung { get; }
        public int Wert { get; }
        public int ZustandProzent { get; }
        public string SicherheitLabel { get; }
        public int SicherheitProzent { get; }

        public StuetzpunktKaufInfo(string name, string beschreibung, int wert, int zustandProzent,
            string sicherheitLabel, int sicherheitProzent)
        {
            Name = name;
            Beschreibung = beschreibung;
            Wert = wert;
            ZustandProzent = zustandProzent;
            SicherheitLabel = sicherheitLabel;
            SicherheitProzent = sicherheitProzent;
        }
    }

    /// <summary>
    /// Kapselt die Anzeigedaten und Aktionen des Söldner-&amp;-Räuber-Bereichs (Migration von
    /// frmSoeldnerRaeuberKarte/frmStuetzpunktKaufen): die Stützpunkte auf der Karte (Räuberlager und
    /// Zollburgen), ihre Besitzverhältnisse für die Flaggen sowie das Kaufangebot an fremde Besitzer.
    /// </summary>
    public class SoeldnerRaeuberManager
    {
        /// <summary>Anzahl der Stützpunkte (Räuberlager + Zollburgen).</summary>
        public int Anzahl => SW.Dynamisch.GetStuetzpunkte().Length;

        /// <summary>Koordinate des Stützpunkt-Rechtecks (x = 0 links, 1 rechts, 2 oben, 3 unten), 1-basiert.</summary>
        public int GetRechteck(int stuetzpunktId, int x) => SW.Statisch.GetStuetzpunktRechteck(stuetzpunktId, x);

        public int GetBesitzer(int stuetzpunktId) => SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1].Besitzer;

        /// <summary>Gehört der Stützpunkt dem aktiven Spieler (dann wird er verwaltet, nicht gekauft)?</summary>
        public bool GehoertAktivemSpieler(int stuetzpunktId) => GetBesitzer(stuetzpunktId) == SW.Dynamisch.GetAktiverSpieler();

        /// <summary>Gehört der Stützpunkt einem menschlichen Spieler (dann weht dessen Flagge)?</summary>
        public bool GehoertMenschlichemSpieler(int stuetzpunktId) => GetBesitzer(stuetzpunktId) < SW.Statisch.GetMinKIID();

        /// <summary>Banner-Nummer des (menschlichen) Besitzers für die Flagge.</summary>
        public int GetBesitzerBanner(int stuetzpunktId) => SW.Dynamisch.GetHumWithID(GetBesitzer(stuetzpunktId)).GetBanner();

        /// <summary>Aktueller Taler-Stand des Spielers (Obergrenze für das Kaufangebot).</summary>
        public int GetSpielerTaler() => SW.Dynamisch.GetAktHum().GetTaler();

        /// <summary>Anzeigedaten für den Kauf-Dialog eines Stützpunkts.</summary>
        public StuetzpunktKaufInfo GetKaufInfo(int stuetzpunktId)
        {
            var sp = SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1];

            string besitzerName = sp.Besitzer >= SW.Statisch.GetMinKIID()
                ? SW.Dynamisch.GetKIwithID(sp.Besitzer).GetKompletterName()
                : SW.Dynamisch.GetHumWithID(sp.Besitzer).GetKompletterName();

            return new StuetzpunktKaufInfo(
                sp.Name,
                sp.StuetzpunktArtAlsString() + " im Besitz von " + besitzerName + ".",
                sp.BerechneWert(),
                sp.ZustandInProzent,
                sp.SicherheitTarnungAlsString(),
                sp.SicherheitTarnungInProzent);
        }

        /// <summary>Unterbreitet dem Besitzer des Stützpunkts ein Kaufangebot (nur einmal pro Jahr möglich).</summary>
        public Task<bool> KaufangebotAbgeben(int stuetzpunktId, int preis) =>
            SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1].KaufangebotAbgeben(preis);
    }
}
