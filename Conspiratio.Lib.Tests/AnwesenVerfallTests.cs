using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der jährliche Zustandsverfall eines Wohnsitzes (<see cref="ZugNachrichtenManager.AktualisiereAnwesen"/>),
    /// der Moneysink für Anwesen: ein fertiger Wohnsitz verliert jedes Jahr Zustand, unabhängig von der Nutzung,
    /// und der Spieler zahlt für die Renovierung einen an seinen aktuellen Wert gekoppelten Preis oder verliert
    /// sonst Ansehen, Gesundheit und Verkaufswert (<see cref="Conspiratio.Lib.Gameplay.Wohnsitz.SpHatHaus"/>).
    /// </summary>
    public class AnwesenVerfallTests
    {
        private const int StadtId = 1;
        private const int Landhaus = 5;   // 10 000 Taler Kaufpreis, Ansehensbonus 5
        private const int Schloss = 9;    // 250 000 Taler Kaufpreis

        [Fact]
        public void Ein_fertiger_Wohnsitz_verliert_jaehrlich_Zustand()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueFertigenWohnsitz(Landhaus);

            new ZugNachrichtenManager().AktualisiereAnwesen();

            Assert.Equal(92, anwesen.GetZustandInProzent(StadtId));
        }

        [Fact]
        public void Der_Verfall_summiert_sich_ueber_mehrere_Jahre()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueFertigenWohnsitz(Landhaus);
            var zugNachrichten = new ZugNachrichtenManager();

            zugNachrichten.AktualisiereAnwesen();
            zugNachrichten.AktualisiereAnwesen();
            zugNachrichten.AktualisiereAnwesen();

            Assert.Equal(76, anwesen.GetZustandInProzent(StadtId));
        }

        /// <summary>
        /// Die Reihenfolge ist die eigentliche Gefahrenstelle: eine in diesem Jahr beauftragte Renovierung
        /// muss den Zustand exakt auf 100 % zurücksetzen, nicht auf 100 % minus dem Jahresverfall. Liefe der
        /// Verfall nach der Renovierung statt davor, stünde hier 92 statt 100 – der Test ist also für die
        /// Reihenfolge im Produktionscode empfindlich, nicht nur für die Existenz der Renovierung.
        /// </summary>
        [Fact]
        public void Eine_beauftragte_Renovierung_beendet_das_Jahr_bei_vollem_Zustand()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueFertigenWohnsitz(Landhaus);
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId);
            haus.ZustandInProzent = 50;  // simuliert bereits eingetretenen Vorjahresverfall

            SW.Dynamisch.GetAktHum().ErhoeheTaler(1000000);
            anwesen.Renoviere(StadtId);
            Assert.True(anwesen.RenovierungBereitsBeauftragt(StadtId));

            new ZugNachrichtenManager().AktualisiereAnwesen();

            Assert.Equal(100, anwesen.GetZustandInProzent(StadtId));
            Assert.False(anwesen.RenovierungBereitsBeauftragt(StadtId));
        }

        [Fact]
        public void Ein_Wohnsitz_im_Bau_verfaellt_nicht()
        {
            TestSpielwelt.Starte();
            var anwesen = new AnwesenManager();
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId);

            haus.SetHausID(Schloss);
            haus.SetStadtID(StadtId);
            haus.SetRestlicheBauzeit(SW.Statisch.GetHaus(Schloss).Bauzeit);  // > 0, also noch im Bau
            haus.ZustandInProzent = 100;

            Assert.False(anwesen.IstFertig(StadtId));

            new ZugNachrichtenManager().AktualisiereAnwesen();

            Assert.Equal(100, anwesen.GetZustandInProzent(StadtId));
        }

        [Fact]
        public void Der_Zustand_faellt_nicht_unter_null()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueFertigenWohnsitz(Landhaus);
            var zugNachrichten = new ZugNachrichtenManager();

            // 8 % pro Jahr, 100 % Startzustand: nach 13 Jahren wäre man ohne Klemmung bei -4 %.
            for (int jahr = 0; jahr < 13; jahr++)
                zugNachrichten.AktualisiereAnwesen();

            Assert.Equal(0, anwesen.GetZustandInProzent(StadtId));
        }

        /// <summary>
        /// Zeigt die eigentliche Konsequenz des Verfalls: ein heruntergekommenes Anwesen ist nicht nur eine
        /// andere Zahl auf dem Bildschirm, sondern bringt spürbar weniger Ansehen, Wert und Gesundheit als
        /// dasselbe Anwesen im Neuzustand – das schließt den in der Spec beschriebenen Kreislauf.
        /// </summary>
        [Fact]
        public void Ein_verfallenes_Anwesen_bringt_weniger_Ansehen_und_Wert_als_ein_intaktes()
        {
            TestSpielwelt.Starte();
            var anwesen = BaueFertigenWohnsitz(Schloss);
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId);
            haus.HausErweiterungen = new System.Collections.Generic.List<int> { 0 };  // "herrschaftlicher Park": Gesundheitsbonus 5

            int ansehenIntakt = haus.GetAnsehensbonus();
            int wertIntakt = anwesen.GetVerkaufswert(StadtId);
            int gesundheitIntakt = haus.GetGesundheitsbonus();

            var zugNachrichten = new ZugNachrichtenManager();
            for (int jahr = 0; jahr < 5; jahr++)
                zugNachrichten.AktualisiereAnwesen();  // 5 * 8 % = 40 Prozentpunkte Verfall

            Assert.True(haus.GetAnsehensbonus() < ansehenIntakt);
            Assert.True(anwesen.GetVerkaufswert(StadtId) < wertIntakt);
            Assert.True(haus.GetGesundheitsbonus() < gesundheitIntakt);
        }

        /// <summary>Errichtet dem aktiven Spieler einen bereits bezugsfertigen Wohnsitz in <see cref="StadtId"/>.</summary>
        private static AnwesenManager BaueFertigenWohnsitz(int hausId)
        {
            var anwesen = new AnwesenManager();
            var haus = SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(StadtId);

            haus.SetHausID(hausId);
            haus.SetStadtID(StadtId);
            haus.SetRestlicheBauzeit(0);
            haus.ZustandInProzent = 100;

            return anwesen;
        }
    }
}
