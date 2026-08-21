using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Nebenbefund aus einem Review: <c>HumSpieler.GetGesamtVermoegen</c> zählt Werkstätten über eine
    /// Schleife, die bei Werkstattindex 1 statt 0 beginnt. Das interne Feld
    /// <c>_spielerHatInStadtXWerkstaettenY</c> ist aber 0-basiert (Platz 1 einer Stadt liegt auf
    /// Index 0) – Platz 1 jeder Stadt fehlt dadurch komplett im Gesamtvermögen. Das korrekte Muster
    /// steht in <c>HumSpieler.ErmittleLagerplatzInStadt</c>.
    /// </summary>
    public class GesamtvermoegenTests
    {
        private const int Stadt1 = 1;

        [Fact]
        public void Eine_Werkstatt_auf_Platz_1_zaehlt_ins_Gesamtvermoegen()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            int spielerId = SW.Dynamisch.GetAktiverSpieler();

            int vermoegenOhneWerkstatt = spieler.GetGesamtVermoegen(spielerId);

            // Platz 1 (1-basiert) von Stadt 1 liegt intern auf Werkstattindex 0 - genau die Stelle,
            // die eine Schleife mit Startwert 1 überspringt.
            spieler.GetSpielerHatInStadtXWerkstaettenY(1, Stadt1).SetEnabled(true);

            int vermoegenMitWerkstatt = spieler.GetGesamtVermoegen(spielerId);

            // Das Gesamtvermögen muss eine besessene Werkstatt genau mit dem Betrag zählen, den ihr
            // Verkauf tatsächlich einbringt (HandelsManager.GetWerkstattVerkaufspreis) - nicht gegen
            // eine fest eingetippte Zahl, sonst prüft der Test nur sich selbst statt die Übereinstimmung
            // von Bewertung und echtem Verkaufserlös.
            int erwarteteSteigerung = new HandelsManager().GetWerkstattVerkaufspreis(Stadt1, 1);

            Assert.Equal(vermoegenOhneWerkstatt + erwarteteSteigerung, vermoegenMitWerkstatt);
        }

        /// <summary>
        /// Sucht einen Werkstattplatz, dessen Rohstoff-ID über der Zahl der Werkstattplätze je Stadt
        /// liegt (0..5, s. <c>SW.Statisch.GetMaxWerkstaettenProStadt()</c> = 6). Der fehlerhafte Code in
        /// <c>GetGesamtVermoegen</c> indiziert <c>_hatInStadtXMengeYRohstoffe</c> versehentlich mit dem
        /// 0-basierten Werkstattplatz statt der Rohstoff-ID – bei einer solchen ID kann er den echten
        /// Lagerbestand unmöglich zufällig treffen, was den Test beweiskräftig macht.
        /// </summary>
        private static bool FindePlatzMitHochnumerierterRohID(out int stadtId, out int werkstattNr, out int rohId)
        {
            for (stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (werkstattNr = 1; werkstattNr <= SW.Statisch.GetMaxWerkstaettenProStadt(); werkstattNr++)
                {
                    rohId = SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(werkstattNr);

                    if (rohId >= SW.Statisch.GetMaxWerkstaettenProStadt())
                        return true;
                }
            }

            stadtId = 0;
            werkstattNr = 0;
            rohId = 0;
            return false;
        }

        /// <summary>
        /// Nebenbefund aus demselben Review: Zeile 495 liest den Lagerbestand über
        /// <c>_hatInStadtXMengeYRohstoffe[i, j]</c> mit dem 0-basierten Werkstattindex <c>j</c> (0..5)
        /// statt über die tatsächliche Rohstoff-ID <c>rohid</c>, die eine Zeile darüber schon berechnet
        /// wird. Bei einer Rohstoff-ID oberhalb der Werkstattplatzzahl wird der Bestand dadurch am
        /// falschen Index abgelesen (dort steht 0) und taucht gar nicht im Gesamtvermögen auf.
        /// </summary>
        [Fact]
        public void Rohstoffbestand_auf_einem_hochnumerierten_Platz_zaehlt_ins_Gesamtvermoegen()
        {
            TestSpielwelt.Starte();

            Assert.True(FindePlatzMitHochnumerierterRohID(out int stadtId, out int werkstattNr, out int rohId),
                        "Es muss einen Werkstattplatz geben, dessen Rohstoff-ID die Zahl der " +
                        "Werkstattplätze je Stadt übersteigt.");

            var spieler = SW.Dynamisch.GetAktHum();
            int spielerId = SW.Dynamisch.GetAktiverSpieler();

            var werkstatt = spieler.GetSpielerHatInStadtXWerkstaettenY(werkstattNr, stadtId);
            werkstatt.SetRohstoffID(rohId);
            werkstatt.SetSkillX(1, 100_000);  // reichlich Lagerraum, damit SetStadtRohstoffAnzahl nicht kappt
            werkstatt.SetEnabled(true);

            int vermoegenOhneRohstoff = spieler.GetGesamtVermoegen(spielerId);

            int anzahl = spieler.SetStadtRohstoffAnzahl(stadtId, rohId, 50);
            Assert.True(anzahl > 0, "Der Testaufbau muss tatsächlich Lagerbestand anlegen können.");

            int vermoegenMitRohstoff = spieler.GetGesamtVermoegen(spielerId);

            int erwarteteSteigerung = SW.Dynamisch.GetStadtwithID(stadtId).GetRohstoffPreisVonIDX(rohId) * anzahl;

            Assert.Equal(vermoegenOhneRohstoff + erwarteteSteigerung, vermoegenMitRohstoff);
        }
    }
}
