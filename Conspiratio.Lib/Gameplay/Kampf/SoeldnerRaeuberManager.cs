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

        /// <summary>Liegt für einen Stützpunkt des aktiven Spielers ein Kaufangebot vor?</summary>
        public bool StehenKaufangeboteAn()
        {
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
                if (stuetzpunkt.Besitzer == aktiverSpieler && stuetzpunkt.AngebotVonSpielerID != 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Legt dem aktiven Spieler zu Zugbeginn alle eingegangenen Kaufangebote seiner Mitspieler vor
        /// (Annahme verkauft den Stützpunkt, Ablehnung erstattet dem Anbieter den reservierten Betrag).
        /// </summary>
        public async Task VerarbeiteEingehendeKaufangebote()
        {
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
                if (stuetzpunkt.Besitzer == aktiverSpieler && stuetzpunkt.AngebotVonSpielerID != 0)
                    await stuetzpunkt.AngebotVorlegen();
        }

        /// <summary>Ob der Stützpunkt vom Besitzer zum Verkauf angeboten wird.</summary>
        public bool IstZumVerkaufAngeboten(int stuetzpunktId) =>
            SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1].ZumVerkaufAngeboten;

        /// <summary>Bietet den Stützpunkt zum Verkauf an bzw. nimmt das Angebot zurück.</summary>
        public void SetzeZumVerkauf(int stuetzpunktId, bool angeboten) =>
            SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1].ZumVerkaufAngeboten = angeboten;

        /// <summary>
        /// Erzeugt zu Zugbeginn für zum Verkauf angebotene Stützpunkte des aktiven Spielers gelegentlich ein
        /// zufälliges KI-Kaufangebot (Preis um den aktuellen Wert), das anschließend vorgelegt wird.
        /// </summary>
        public void GeneriereKiKaufangebote()
        {
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                if (stuetzpunkt.Besitzer != aktiverSpieler || !stuetzpunkt.ZumVerkaufAngeboten || stuetzpunkt.AngebotVonSpielerID != 0)
                    continue;

                // Nicht jede Runde meldet sich ein Käufer.
                if (SW.Statisch.Rnd.Next(0, 100) >= 50)
                    continue;

                int kiId = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());
                int preis = stuetzpunkt.BerechneWert() * SW.Statisch.Rnd.Next(70, 116) / 100;

                if (preis < 1)
                    preis = 1;

                stuetzpunkt.AngebotVonSpielerID = kiId;
                stuetzpunkt.AngebotPreis = preis;
            }
        }

        /// <summary>
        /// Zeigt dem aktiven Spieler zu Zugbeginn die Ergebnisse seiner eigenen Kaufangebote (Annahme oder
        /// Ablehnung durch den jeweiligen Besitzer) und leert die Nachrichtenliste anschließend.
        /// </summary>
        public async Task ZeigeHandelsnachrichten()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.HandelsNachrichten.Count == 0)
                return;

            foreach (string nachricht in spieler.HandelsNachrichten)
                await SW.UI.ShowText.ShowDialog(nachricht);

            spieler.HandelsNachrichten.Clear();
        }
    }
}
