using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Bauzeit eines Wohnsitzes: das Fertigstellungsjahr, das die Ansicht anzeigt, und die Möglichkeit,
    /// den Baumeister für eine sofortige Fertigstellung im Folgejahr zu bestechen.
    /// </summary>
    public class WohnsitzBauzeitTests
    {
        private const int StadtId = 1;
        private const int Villa = 6;      // Bauzeit 6 Jahre, Preis 20 000 Taler
        private const int Kate = 1;       // Bauzeit 1 Jahr, Preis 1 000 Taler

        [Fact]
        public void Das_Fertigstellungsjahr_liegt_um_die_Restbauzeit_in_der_Zukunft()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Villa);

            int erwartet = SW.Dynamisch.GetAktuellesJahr() + anwesen.GetRestlicheBauzeit(StadtId);

            Assert.Equal(erwartet, anwesen.GetFertigstellungsjahr(StadtId));
        }

        [Fact]
        public void Ein_fertiger_Wohnsitz_hat_das_aktuelle_Jahr_als_Fertigstellungsjahr()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Villa);
            SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId).SetRestlicheBauzeit(0);

            Assert.Equal(SW.Dynamisch.GetAktuellesJahr(), anwesen.GetFertigstellungsjahr(StadtId));
        }

        [Fact]
        public void Eine_Beschleunigung_wird_nur_angeboten_wenn_mehr_als_ein_Jahr_zu_sparen_ist()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Villa);

            Assert.True(anwesen.KannBauBeschleunigen(StadtId));

            // Ein Wohnsitz, der ohnehin nächstes Jahr fertig wird, lässt sich nicht mehr beschleunigen.
            SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId).SetRestlicheBauzeit(1);
            Assert.False(anwesen.KannBauBeschleunigen(StadtId));
        }

        [Fact]
        public void Eine_Kate_laesst_sich_nicht_beschleunigen()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Kate);  // Bauzeit 1 Jahr

            Assert.False(anwesen.KannBauBeschleunigen(StadtId));
        }

        [Fact]
        public void Der_Bestechungspreis_richtet_sich_nach_den_gesparten_Jahren()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Villa);

            // Villa: 20 000 Taler, 6 Jahre Bauzeit, davon 5 Jahre abkaufbar; 20 % je Jahr = 4 000 Taler.
            Assert.Equal(20000, anwesen.GetBaubeschleunigungsPreis(StadtId));

            SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId).SetRestlicheBauzeit(3);
            Assert.Equal(8000, anwesen.GetBaubeschleunigungsPreis(StadtId));  // 2 Jahre
        }

        [Fact]
        public void Die_Bestechung_zieht_den_Preis_ab_und_kuerzt_die_Bauzeit_auf_ein_Jahr()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueWohnsitz(Villa);

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.ErhoeheTaler(100000);

            int talerVorher = spieler.GetTaler();
            int preis = anwesen.GetBaubeschleunigungsPreis(StadtId);

            anwesen.BeschleunigeBau(StadtId);

            Assert.Equal(1, anwesen.GetRestlicheBauzeit(StadtId));
            Assert.Equal(talerVorher - preis, spieler.GetTaler());
            Assert.False(anwesen.IstFertig(StadtId));  // Fertig wird er erst beim nächsten Rundenwechsel
        }

        /// <summary>Errichtet dem aktiven Spieler den gewünschten Wohnsitz in <see cref="StadtId"/>.</summary>
        private static AnwesenManager BaueWohnsitz(int hausId)
        {
            var anwesen = new AnwesenManager();
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId);

            haus.SetHausID(hausId);
            haus.SetStadtID(StadtId);
            haus.SetRestlicheBauzeit(SW.Statisch.GetHaus(hausId).Bauzeit);

            return anwesen;
        }
    }
}
