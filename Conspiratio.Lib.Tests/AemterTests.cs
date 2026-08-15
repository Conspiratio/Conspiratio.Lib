using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    public class AemterTests
    {
        /// <summary>
        /// Eine angelegte Wahl ohne einen einzigen Bewerber darf den Jahreswechsel nicht abbrechen.
        /// `IstDieWahlVoll` prüft nur, ob die Wahl überhaupt angelegt ist – zuvor wurde trotzdem blind
        /// einer der KI-Plätze ausgewürfelt und der leere Platz (Kandidat 0) führte zur
        /// NullReferenceException. Gefunden vom automatischen Spieldurchlauf des Godot-Clients.
        /// </summary>
        [Fact]
        public void WahlOhneBewerberBrichtDasAuffuellenNichtAb()
        {
            TestSpielwelt.Starte(seed: 1);

            var wahl = SW.Dynamisch.GetWahlX(1);
            wahl.AmtID = 1;
            wahl.GebietID = 1;
            wahl.Stufe = 0;

            // Kein einziger Kandidat – genau der Zustand, der den Absturz auslöste.
            foreach (int platz in new[] { 0, 1 })
                wahl.SetKandidatenXAufY(platz, 0);

            var aemter = new AemterManager();

            aemter.FuelleRestlicheAemter();

            // Das Amt bleibt unbesetzt; die Wahl steht damit im nächsten Jahr erneut an.
            Assert.Equal(1, SW.Dynamisch.GetWahlX(1).AmtID);
        }

        /// <summary>Ist genau ein Bewerber da, gewinnt er – auch wenn die übrigen Plätze leer sind.</summary>
        [Fact]
        public void EinzelnerBewerberGewinntDasAmt()
        {
            TestSpielwelt.Starte(seed: 2);

            int kiId = TestSpielwelt.SetzeKiGegner(1, 0);

            var wahl = SW.Dynamisch.GetWahlX(1);
            wahl.AmtID = 1;
            wahl.GebietID = 1;
            wahl.Stufe = 0;
            wahl.SetKandidatenXAufY(0, 0);
            wahl.SetKandidatenXAufY(1, kiId);

            new AemterManager().FuelleRestlicheAemter();

            Assert.Equal(1, SW.Dynamisch.GetSpWithID(kiId).GetAmtID());
        }
    }
}
