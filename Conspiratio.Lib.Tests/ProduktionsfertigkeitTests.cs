using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Produktionsfertigkeit je Ware hebt in `Produktionsslot.GetProduktion` den **unteren** Rand
    /// der Jahresschwankung: Der Meister hat weniger Fehlchargen, keine besseren Glücksjahre.
    ///
    /// Weil die Qualitätsangabe des Jahresbuchs aus derselben Schwankung abgeleitet wird, wandert sie
    /// automatisch mit. Jene Zahl zeigte bis dahin reinen Zufall an — sie war eine Anzeige ohne Ursache
    /// und ohne Wirkung.
    ///
    /// Fertigkeit 0 ist exakt das bisherige Verhalten; alte Spielstände rechnen unverändert weiter.
    /// </summary>
    public class ProduktionsfertigkeitTests
    {
        private const int Korn = 1;
        private const int Stadt = 1;
        private const int Slot = 0;
        private const int Staetten = 20;

        /// <summary>Ohne Ausschlag steht die Qualität bei 50 — darüber liegt ein gutes Jahr.</summary>
        private const int QualitaetOhneAusschlag = 50;

        /// <summary>Stellt einen voll besetzten Produktionsplatz und liefert ihn zurück.</summary>
        private static Produktionsslot BereiteSlotVor()
        {
            TestSpielwelt.Starte();

            var ware = SW.Dynamisch.GetRohstoffwithID(Korn);
            int proStaette = ware.GetWerkstaetten() > 0 ? ware.GetArbeiter() / ware.GetWerkstaetten() : 1;

            var slot = SW.Dynamisch.GetAktHum().GetProduktionsslot(Stadt, Slot);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(Korn);
            slot.SetProduktionStaetten(Staetten);
            slot.SetProduktionArbeiter(Staetten * proStaette);

            return slot;
        }

        /// <summary>Produziert <paramref name="durchgaenge"/> Jahre und meldet Summe und kleinste Qualität.</summary>
        private static (long Summe, int KleinsteQualitaet) Produziere(Produktionsslot slot, int fertigkeit, int durchgaenge)
        {
            long summe = 0;
            int kleinste = int.MaxValue;

            for (int i = 0; i < durchgaenge; i++)
            {
                int qualitaet = 0;
                summe += slot.GetProduktion(Stadt, Korn, fertigkeit, ref qualitaet);

                if (qualitaet < kleinste)
                    kleinste = qualitaet;
            }

            return (summe, kleinste);
        }

        /// <summary>
        /// Der Kern der Regel: Bei voller Fertigkeit gibt es kein schlechtes Jahr mehr. Die Schwankung
        /// liegt dann in [0, PlusMinus], die Qualität also nie unter 50.
        /// </summary>
        [Fact]
        public void Volle_Fertigkeit_kennt_kein_schlechtes_Jahr()
        {
            var slot = BereiteSlotVor();

            var ergebnis = Produziere(slot, HumSpieler.MaxProduktionsfertigkeit, 500);

            Assert.True(ergebnis.KleinsteQualitaet >= QualitaetOhneAusschlag,
                        "Bei voller Fertigkeit darf die Qualitaet nie unter 50 fallen, war: " + ergebnis.KleinsteQualitaet);
        }

        /// <summary>Ohne Fertigkeit bleibt der Zufall, wie er war — es gibt weiterhin schlechte Jahre.</summary>
        [Fact]
        public void Ohne_Fertigkeit_gibt_es_weiterhin_schlechte_Jahre()
        {
            var slot = BereiteSlotVor();

            var ergebnis = Produziere(slot, 0, 500);

            Assert.True(ergebnis.KleinsteQualitaet < QualitaetOhneAusschlag,
                        "Ohne Fertigkeit muss ueber 500 Jahre ein schlechtes dabei sein.");
        }

        /// <summary>
        /// Und sie lohnt sich: Über viele Jahre bringt die volle Fertigkeit mehr als gar keine. Der
        /// Zugewinn ist bewusst klein — rund 5 % der Menge, nicht ein Vielfaches.
        /// </summary>
        [Fact]
        public void Fertigkeit_hebt_den_Ertrag()
        {
            var slot = BereiteSlotVor();

            long ohne = Produziere(slot, 0, 2000).Summe;
            long voll = Produziere(slot, HumSpieler.MaxProduktionsfertigkeit, 2000).Summe;

            Assert.True(voll > ohne, "Volle Fertigkeit muss mehr einbringen: " + voll + " gegen " + ohne);

            // Der Zugewinn liegt in der Groessenordnung eines Zwanzigstels - nicht eines Vielfachen.
            Assert.InRange(voll, ohne, (long)(ohne * 1.2));
        }

        /// <summary>Halbe Fertigkeit liegt zwischen beiden Enden — die Skala wirkt stufenlos.</summary>
        [Fact]
        public void Halbe_Fertigkeit_liegt_dazwischen()
        {
            var slot = BereiteSlotVor();

            long ohne = Produziere(slot, 0, 2000).Summe;
            long halb = Produziere(slot, HumSpieler.MaxProduktionsfertigkeit / 2, 2000).Summe;
            long voll = Produziere(slot, HumSpieler.MaxProduktionsfertigkeit, 2000).Summe;

            Assert.True(halb > ohne && halb < voll,
                        "Halbe Fertigkeit muss zwischen 0 und voll liegen: " + ohne + " / " + halb + " / " + voll);
        }

        /// <summary>
        /// Ein Produktionsplatz, der auf „Produzieren“ steht, aber keine Arbeiter hat, liefert nichts –
        /// und darf daran nicht scheitern. Die Qualitätsrechnung teilte durch die Schwankungsbreite,
        /// die bei einer Menge von 0 selbst 0 ist: 0/0 ergibt NaN, und <c>Convert.ToInt32</c> quittiert
        /// das mit einer OverflowException.
        ///
        /// Der Zustand ist im Client mit zwei Klicks hergestellt, denn Arbeiter und Stätten sind zwei
        /// getrennte Zahlenknöpfe – wer die Stätten zuerst setzt und den Zug beendet, hatte den
        /// Absturz. Gefunden hat ihn ein Test zur Anzeige, nicht das Spiel.
        /// </summary>
        [Fact]
        public void Ein_Platz_ohne_Arbeiter_liefert_nichts_statt_abzustuerzen()
        {
            TestSpielwelt.Starte();

            var slot = SW.Dynamisch.GetAktHum().GetProduktionsslot(Stadt, Slot);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(Korn);
            slot.SetProduktionStaetten(Staetten);
            slot.SetProduktionArbeiter(0);

            int qualitaet = -1;

            Assert.Equal(0, slot.GetProduktion(Stadt, Korn, 0, ref qualitaet));
            Assert.Equal(QualitaetOhneAusschlag, qualitaet);
        }

        /// <summary>Derselbe Fall auf dem Weg, auf dem er den Spieler traf: dem Jahresbuch.</summary>
        [Fact]
        public void Das_Jahresbuch_uebersteht_einen_Platz_ohne_Arbeiter()
        {
            TestSpielwelt.Starte();

            var slot = SW.Dynamisch.GetAktHum().GetProduktionsslot(Stadt, Slot);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(Korn);
            slot.SetProduktionStaetten(Staetten);
            slot.SetProduktionArbeiter(0);

            var buch = new BuchManager().ErstelleJahresbuchFuerAktivenSpieler();

            Assert.False(buch.EtwasProduziert);
            Assert.Equal(0, buch.ProduzierteWaren[Korn]);
        }

        [Fact]
        public void Die_Fertigkeit_wird_auf_null_bis_hundert_geklemmt()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.SetProduktionsfertigkeit(Korn, 500);
            Assert.Equal(HumSpieler.MaxProduktionsfertigkeit, spieler.GetProduktionsfertigkeit(Korn));

            spieler.SetProduktionsfertigkeit(Korn, -20);
            Assert.Equal(0, spieler.GetProduktionsfertigkeit(Korn));

            spieler.ErhoeheProduktionsfertigkeit(Korn, 30);
            Assert.Equal(30, spieler.GetProduktionsfertigkeit(Korn));
        }

        /// <summary>
        /// Spielstand-Vertraeglichkeit: Das Feld kommt aus einem alten Spielstand als null, der Accessor
        /// legt es an, und jede Ware startet bei 0 — also beim bisherigen Verhalten.
        /// </summary>
        [Fact]
        public void Ein_alter_Spielstand_startet_bei_null()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();

            var fertigkeiten = spieler.GetProduktionsfertigkeitenSicher();

            Assert.Equal(SW.Statisch.GetMaxRohID(), fertigkeiten.Length);

            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
                Assert.Equal(0, spieler.GetProduktionsfertigkeit(rohstoffId));
        }
    }
}
