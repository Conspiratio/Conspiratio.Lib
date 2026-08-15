using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Das interaktive Wortgefecht eines Duells (Issue #17). Neben den Regeln je Runde sichern zwei
    /// Quotentests die Balance ab: Können soll überwiegen, Fechtunterricht spürbar helfen, aber Training
    /// allein darf keine Siegstrategie sein.
    /// </summary>
    public class WortgefechtTests
    {
        private const int AmtBuergermeister = 7;

        /// <summary>Spielt ein komplettes Wortgefecht; der Spieler kontert stets richtig oder stets falsch.</summary>
        private static WortgefechtManager SpieleDurch(int zielId, bool immerRichtig)
        {
            var gefecht = new WortgefechtManager(zielId);

            while (!gefecht.IstBeendet)
            {
                var angriff = gefecht.NaechsterAngriff();
                int angriffsIndex = angriff.MenschWaehlt ? 0 : gefecht.WaehleKiAngriff();

                var konter = gefecht.WaehleAngriff(angriffsIndex);

                int konterIndex;
                if (!konter.MenschWaehlt)
                    konterIndex = gefecht.WaehleKiKonter();
                else if (immerRichtig)
                    konterIndex = konter.RichtigerIndex;
                else
                    konterIndex = (konter.RichtigerIndex + 1) % konter.Optionen.Count;

                gefecht.WerteKonterAus(konterIndex);
            }

            return gefecht;
        }

        private static int Siegquote(int zielId, bool immerRichtig, int laeufe = 2000)
        {
            int siege = 0;

            for (int lauf = 0; lauf < laeufe; lauf++)
            {
                if (SpieleDurch(zielId, immerRichtig).SpielerHatGewonnen)
                    siege++;
            }

            return siege * 100 / laeufe;
        }

        [Fact]
        public void Jede_Runde_bietet_mindestens_drei_verschiedene_Sprueche()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            var gefecht = new WortgefechtManager(zielId);

            while (!gefecht.IstBeendet)
            {
                var angriff = gefecht.NaechsterAngriff();

                Assert.True(angriff.Optionen.Count >= 3);
                Assert.Equal(angriff.Optionen.Count, angriff.Optionen.Distinct().Count());

                int gewaehlt = angriff.MenschWaehlt ? 0 : gefecht.WaehleKiAngriff();
                var konter = gefecht.WaehleAngriff(gewaehlt);

                Assert.True(konter.Optionen.Count >= 3);
                Assert.Equal(konter.Optionen.Count, konter.Optionen.Distinct().Count());
                Assert.InRange(konter.RichtigerIndex, 0, konter.Optionen.Count - 1);

                // Die Konterrunde gehört zur gewählten Beleidigung, und es kontert die Gegenseite.
                Assert.Equal(angriff.Optionen[gewaehlt], konter.Beleidigung);
                Assert.NotEqual(angriff.WaehlerIstAktiverSpieler, konter.WaehlerIstAktiverSpieler);

                gefecht.WerteKonterAus(konter.MenschWaehlt ? konter.RichtigerIndex : gefecht.WaehleKiKonter());
            }
        }

        [Fact]
        public void Kein_Spruchpaar_wiederholt_sich_im_selben_Duell()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            var gefecht = new WortgefechtManager(zielId);
            var verwendet = new List<string>();

            while (!gefecht.IstBeendet)
            {
                var angriff = gefecht.NaechsterAngriff();
                int gewaehlt = angriff.MenschWaehlt ? 0 : gefecht.WaehleKiAngriff();
                var konter = gefecht.WaehleAngriff(gewaehlt);

                Assert.DoesNotContain(konter.Beleidigung, verwendet);
                verwendet.Add(konter.Beleidigung);

                gefecht.WerteKonterAus(konter.MenschWaehlt ? konter.RichtigerIndex : gefecht.WaehleKiKonter());
            }
        }

        /// <summary>Im Hot-Seat wählen beide Menschen selbst, und die Angreiferrolle wechselt.</summary>
        [Fact]
        public void Gegen_einen_Menschen_waehlen_beide_Seiten_selbst()
        {
            TestSpielwelt.Starte(menschen: 2);
            SW.Dynamisch.GetHumWithID(2).SetAmt(AmtBuergermeister, 1);

            var gefecht = new WortgefechtManager(2);
            Assert.True(gefecht.GegnerIstMensch);

            bool? letzterAngreifer = null;

            while (!gefecht.IstBeendet)
            {
                var angriff = gefecht.NaechsterAngriff();
                Assert.True(angriff.MenschWaehlt);

                if (letzterAngreifer.HasValue)
                    Assert.NotEqual(letzterAngreifer.Value, angriff.WaehlerIstAktiverSpieler);

                letzterAngreifer = angriff.WaehlerIstAktiverSpieler;

                var konter = gefecht.WaehleAngriff(0);
                Assert.True(konter.MenschWaehlt);

                gefecht.WerteKonterAus(konter.RichtigerIndex);
            }
        }

        [Fact]
        public void Der_Treffervorsprung_aus_der_Fechtfaehigkeit_ist_gedeckelt()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 1000;
            var starkerSpieler = new WortgefechtManager(zielId);

            Assert.Equal(1, starkerSpieler.SpielerTreffer);
            Assert.Equal(0, starkerSpieler.GegnerTreffer);

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 0;
            int starkesZiel = TestSpielwelt.SetzeKiGegner(1, AmtBuergermeister, bosheit: 100);
            var schwacherSpieler = new WortgefechtManager(starkesZiel);

            Assert.Equal(0, schwacherSpieler.SpielerTreffer);
            Assert.Equal(1, schwacherSpieler.GegnerTreffer);

            // Ein Duell darf durch den Vorsprung nie schon vor dem ersten Wortwechsel entschieden sein.
            Assert.False(schwacherSpieler.IstBeendet);
        }

        [Fact]
        public void Wer_immer_passend_kontert_gewinnt_auch_ohne_Fechtunterricht()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);
            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 0;

            // Gemessen rund 70 %; die Schwelle lässt Luft für Zufall und leichtes Nachjustieren.
            Assert.True(Siegquote(zielId, immerRichtig: true) >= 60);
        }

        [Fact]
        public void Fechtunterricht_allein_ist_keine_Siegstrategie()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);
            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 200;

            // Gemessen rund 41 %: Training hilft spürbar, ersetzt den Witz aber nicht.
            Assert.True(Siegquote(zielId, immerRichtig: false) < 50);
        }

        [Fact]
        public void Fechtunterricht_hebt_die_Siegquote()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 0;
            int ohne = Siegquote(zielId, immerRichtig: true);

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 150;
            int mit = Siegquote(zielId, immerRichtig: true);

            Assert.True(mit > ohne, $"Mit Unterricht {mit} %, ohne {ohne} %");
        }
    }
}
