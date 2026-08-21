using System;

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

            // Stadt 1, Platz 1 produziert Holz (Stufe 1) => Grundpreis 2000 (siehe
            // HandelsbalancingTests.Der_erste_Betrieb_kostet_unveraendert_den_Grundpreis).
            // GetGesamtVermoegen bewertet eine besessene Werkstatt mit 70 % dieses Grundpreises.
            int erwarteteSteigerung = Convert.ToInt32(2000 * 0.7);

            Assert.Equal(vermoegenOhneWerkstatt + erwarteteSteigerung, vermoegenMitWerkstatt);
        }
    }
}
