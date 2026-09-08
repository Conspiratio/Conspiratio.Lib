using System.Threading.Tasks;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Wege zur Produktionsfertigkeit: Bücher, Lehrstunden, der Hausgelehrte im Schloss – und das
    /// Verlernen, wenn eine Ware jahrelang liegen bleibt.
    ///
    /// Die beiden Zufallswege (Vortrag, Meisterschriftfund) hängen an Waren, die der Spieler gerade
    /// herstellt. Die Tests hier setzen deshalb bewusst **keine** Produktion, wo sie es nicht müssen:
    /// Dann würfelt <c>FuehreJahresereignisseDurch</c> zwar, findet aber keinen Kandidaten, und der
    /// gemessene Weg bleibt für sich.
    /// </summary>
    public class FertigkeitErwerbTests
    {
        private const int Stadt = 1;
        private const int Slot = 0;
        private const int Schloss = 9;
        private const int Villa = 6;

        private static ProduktionsfertigkeitManager Manager => new ProduktionsfertigkeitManager();

        private static HumSpieler Spieler => SW.Dynamisch.GetAktHum();

        /// <summary>Eine Ware, die in dieser Stadt zur Hauptproduktion gehört.</summary>
        private static int WareAusHauptproduktion(int stadtId = Stadt)
            => SW.Dynamisch.GetStadtwithID(stadtId).GetHauptproduktion()[0];

        /// <summary>Eine Ware, die in dieser Stadt <b>nicht</b> zur Hauptproduktion gehört.</summary>
        private static int WareOhneHauptproduktion(int stadtId = Stadt)
        {
            var haupt = SW.Dynamisch.GetStadtwithID(stadtId).GetHauptproduktion();

            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
            {
                if (System.Array.IndexOf(haupt, rohstoffId) < 0)
                    return rohstoffId;
            }

            return 0;
        }

        /// <summary>Setzt dem Spieler ein fertiges Haus der gewünschten Stufe.</summary>
        private static void GibHaus(int hausId)
        {
            var haus = Spieler.GetSpielerHatHausVonStadtAnArraystelle(Stadt);
            haus.SetHausID(hausId);
            haus.SetRestlicheBauzeit(0);
        }

        /// <summary>Lässt den Spieler eine Ware tatsächlich herstellen – nur dann gilt sie als betrieben.</summary>
        private static void ProduziereWare(int rohstoffId)
        {
            var slot = Spieler.GetProduktionsslot(Stadt, Slot);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.Produzieren);
            slot.SetProduktionRohstoff(rohstoffId);
            slot.SetProduktionStaetten(5);
        }

        private static void SetzeTaler(int taler) => Spieler.ErhoeheTaler(taler - Spieler.GetTaler());

        #region Bücher

        /// <summary>
        /// Der Ortsbezug ist der ganze Witz des Buches: Es steht dort, wo die Ware hergestellt wird.
        /// Anderswo gibt es nichts zu holen – das macht das Reisen zum Teil des Lernens.
        /// </summary>
        [Fact]
        public void Ein_Buch_gibt_es_nur_wo_die_Ware_hergestellt_wird()
        {
            TestSpielwelt.Starte();

            Assert.True(Manager.GibtEsBuch(Stadt, WareAusHauptproduktion()));
            Assert.False(Manager.GibtEsBuch(Stadt, WareOhneHauptproduktion()));
        }

        [Fact]
        public void Ein_Buch_kostet_Taler_und_hebt_die_Fertigkeit()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            int preis = Manager.GetBuchpreis(ware);

            SetzeTaler(100000);

            Assert.True(Manager.KaufeBuch(Stadt, ware, out _));
            Assert.Equal(100000 - preis, Spieler.GetTaler());
            Assert.Equal(ProduktionsfertigkeitManager.BuchGewinn, Spieler.GetProduktionsfertigkeit(ware));
        }

        /// <summary>
        /// Ein zweites Exemplar desselben Buches lehrt nichts. Sonst wäre die Fertigkeit für den
        /// wohlhabenden Spieler eine Ware wie jede andere – man kaufte sie einfach zwölfmal.
        /// </summary>
        [Fact]
        public void Dasselbe_Buch_lehrt_nur_einmal()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            SetzeTaler(100000);

            Assert.True(Manager.KaufeBuch(Stadt, ware, out _));

            Assert.False(Manager.GibtEsBuch(Stadt, ware));
            Assert.False(Manager.KaufeBuch(Stadt, ware, out _));
            Assert.Equal(ProduktionsfertigkeitManager.BuchGewinn, Spieler.GetProduktionsfertigkeit(ware));
        }

        [Fact]
        public void Ohne_Taler_kein_Buch()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            SetzeTaler(10);

            Assert.False(Manager.KaufeBuch(Stadt, ware, out _));
            Assert.Equal(10, Spieler.GetTaler());
            Assert.Equal(0, Spieler.GetProduktionsfertigkeit(ware));
        }

        #endregion

        #region Lehrstunden

        /// <summary>
        /// Die Preisstaffel ist die eigentliche Bremse: Wer eine Ware zur Vollendung bringen will, zahlt
        /// weit mehr als das Zwanzigfache der ersten Stunde – und kann das nicht für jede Ware tun.
        /// </summary>
        [Fact]
        public void Jede_Lehrstunde_macht_die_naechste_teurer()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            SetzeTaler(1000000);

            Assert.Equal(ProduktionsfertigkeitManager.GrundpreisLehrstunde, Manager.GetLehrstundenPreis(ware));

            Assert.True(Manager.NimmLehrstunde(ware, out _));

            Assert.Equal(ProduktionsfertigkeitManager.GrundpreisLehrstunde +
                         ProduktionsfertigkeitManager.PreisSteigerungProLehrstunde,
                         Manager.GetLehrstundenPreis(ware));

            Assert.True(Manager.NimmLehrstunde(ware, out _));

            Assert.Equal(ProduktionsfertigkeitManager.GrundpreisLehrstunde +
                         2 * ProduktionsfertigkeitManager.PreisSteigerungProLehrstunde,
                         Manager.GetLehrstundenPreis(ware));

            Assert.Equal(2 * ProduktionsfertigkeitManager.LehrstundeGewinn,
                         Spieler.GetProduktionsfertigkeit(ware));
        }

        /// <summary>Die Staffel gilt je Ware: Eine neue Ware beginnt wieder beim Grundpreis.</summary>
        [Fact]
        public void Die_Preisstaffel_gilt_je_Ware()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            int andere = WareOhneHauptproduktion();

            SetzeTaler(1000000);
            Assert.True(Manager.NimmLehrstunde(ware, out _));

            Assert.Equal(ProduktionsfertigkeitManager.GrundpreisLehrstunde, Manager.GetLehrstundenPreis(andere));
        }

        [Fact]
        public void Bei_voller_Fertigkeit_gibt_es_nichts_mehr_zu_lernen()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            Spieler.SetProduktionsfertigkeit(ware, HumSpieler.MaxProduktionsfertigkeit);
            SetzeTaler(1000000);

            Assert.False(Manager.NimmLehrstunde(ware, out _));
            Assert.Equal(1000000, Spieler.GetTaler());
        }

        #endregion

        #region Verlernen

        /// <summary>
        /// Das Verlernen greift erst nach <see cref="ProduktionsfertigkeitManager.VerlernenNachJahren"/>
        /// ungenutzten Jahren. Es ist der einzige Weg nach unten und bewusst ein abwendbarer: Wer die
        /// Ware weiter herstellt, verliert nichts.
        /// </summary>
        [Fact]
        public async Task Verlernen_setzt_erst_nach_drei_ungenutzten_Jahren_ein()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            Spieler.SetProduktionsfertigkeit(ware, 50);

            // Erstes Jahr: hergestellt, also vermerkt.
            ProduziereWare(ware);
            await Manager.FuehreJahresereignisseDurch();

            int startJahr = SW.Dynamisch.GetAktuellesJahr();

            // Ab jetzt nichts mehr herstellen.
            Spieler.GetProduktionsslot(Stadt, Slot).SetProduktionStaetten(0);

            SW.Dynamisch.SetAktuellesJahr(startJahr + 2);
            await Manager.FuehreJahresereignisseDurch();
            Assert.Equal(50, Spieler.GetProduktionsfertigkeit(ware));

            SW.Dynamisch.SetAktuellesJahr(startJahr + 3);
            await Manager.FuehreJahresereignisseDurch();
            Assert.Equal(50 - ProduktionsfertigkeitManager.VerlernenProJahr,
                         Spieler.GetProduktionsfertigkeit(ware));
        }

        /// <summary>Wer die Ware weiter herstellt, behält sein Können – auch nach vielen Jahren.</summary>
        [Fact]
        public async Task Wer_die_Ware_herstellt_verlernt_sie_nicht()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            Spieler.SetProduktionsfertigkeit(ware, 50);
            ProduziereWare(ware);

            for (int i = 0; i < 10; i++)
            {
                SW.Dynamisch.ErhoehAktuellesJahrUmEins();
                await Manager.FuehreJahresereignisseDurch();
            }

            Assert.True(Spieler.GetProduktionsfertigkeit(ware) >= 50,
                        "Wer produziert, darf nicht verlernen, war: " + Spieler.GetProduktionsfertigkeit(ware));
        }

        /// <summary>
        /// Frisch gelesenes Buchwissen darf nicht verfallen, bevor es einmal angewandt wurde – sonst
        /// wäre ein Buch, das man auf Vorrat kauft, nach drei Jahren wertlos, ohne dass der Spieler
        /// etwas falsch gemacht hätte.
        /// </summary>
        [Fact]
        public async Task Buchwissen_verfaellt_nicht_bevor_es_angewandt_wurde()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            SetzeTaler(100000);
            Assert.True(Manager.KaufeBuch(Stadt, ware, out _));

            SW.Dynamisch.SetAktuellesJahr(SW.Dynamisch.GetAktuellesJahr() + 20);
            await Manager.FuehreJahresereignisseDurch();

            Assert.Equal(ProduktionsfertigkeitManager.BuchGewinn, Spieler.GetProduktionsfertigkeit(ware));
        }

        #endregion

        #region Hausgelehrter

        /// <summary>Ohne Schloss kein Gelehrter – und damit auch kein Lohn.</summary>
        [Fact]
        public async Task Der_Hausgelehrte_verlangt_ein_Schloss()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            Spieler.SetProduktionsfertigkeit(ware, 40);
            GibHaus(Villa);
            SetzeTaler(100000);

            await Manager.FuehreJahresereignisseDurch();

            Assert.Equal(40, Spieler.GetProduktionsfertigkeit(ware));
            Assert.Equal(100000, Spieler.GetTaler());
        }

        /// <summary>
        /// Im Schloss lehrt er von selbst – und zwar die Ware, in der der Spieler ohnehin am weitesten
        /// ist. Meisterschaft vertieft sich, sie verteilt sich nicht.
        /// </summary>
        [Fact]
        public async Task Der_Hausgelehrte_vertieft_die_beste_Ware_und_kostet_Lohn()
        {
            TestSpielwelt.Starte();

            var haupt = SW.Dynamisch.GetStadtwithID(Stadt).GetHauptproduktion();
            int beste = haupt[0];
            int schwaechere = haupt[1];

            Spieler.SetProduktionsfertigkeit(beste, 40);
            Spieler.SetProduktionsfertigkeit(schwaechere, 20);
            GibHaus(Schloss);
            SetzeTaler(100000);

            await Manager.FuehreJahresereignisseDurch();

            Assert.Equal(40 + ProduktionsfertigkeitManager.HausgelehrterGewinn,
                         Spieler.GetProduktionsfertigkeit(beste));
            Assert.Equal(20, Spieler.GetProduktionsfertigkeit(schwaechere));
            Assert.Equal(100000 - ProduktionsfertigkeitManager.HausgelehrterJahreslohn, Spieler.GetTaler());
        }

        /// <summary>
        /// Über die Obergrenze hinaus lehrt er nicht, und ohne jede Vorkenntnis findet er keinen
        /// Anknüpfungspunkt: Der erste Schritt und die letzte Strecke bleiben Sache des Spielers.
        /// </summary>
        [Fact]
        public async Task Der_Hausgelehrte_lehrt_weder_von_vorn_noch_bis_ans_Ende()
        {
            TestSpielwelt.Starte();

            int ware = WareAusHauptproduktion();
            GibHaus(Schloss);
            SetzeTaler(100000);

            // Ohne jede Vorkenntnis: nichts passiert, und der Lohn wird nicht faellig.
            await Manager.FuehreJahresereignisseDurch();
            Assert.Equal(100000, Spieler.GetTaler());

            // Ueber der Obergrenze: ebenso.
            Spieler.SetProduktionsfertigkeit(ware, ProduktionsfertigkeitManager.HausgelehrterObergrenze);
            await Manager.FuehreJahresereignisseDurch();

            Assert.Equal(ProduktionsfertigkeitManager.HausgelehrterObergrenze,
                         Spieler.GetProduktionsfertigkeit(ware));
            Assert.Equal(100000, Spieler.GetTaler());
        }

        #endregion

        #region Zufallswege

        /// <summary>
        /// Vortrag und Schriftfund sind die beiden Wege, die von selbst kommen. Einzelne Jahre sagen
        /// darüber nichts – geprüft wird deshalb die Rate über viele Jahre, wie bei jeder anderen
        /// Wahrscheinlichkeit im Spiel auch. Ein 30-Jahre-Durchlauf des E2E-Treibers ließ sich dazu
        /// nicht auswerten: Seine Meldungen laufen alle durch denselben Nachrichtendialog.
        ///
        /// Je Jahr wird der Zustand zurückgesetzt (Fertigkeit, gelesene Schriften, Taler), denn beide
        /// Wege enden sonst nach kurzer Zeit von selbst: Die Meisterschrift gibt es je Ware nur
        /// einmal, und der Vortrag verlangt Eintritt.
        /// </summary>
        [Fact]
        public async Task Vortrag_und_Schriftfund_treffen_ihre_Haeufigkeit()
        {
            const int Jahre = 3000;

            TestSpielwelt.Starte(seed: 20260908);

            int ware = WareAusHauptproduktion();
            ProduziereWare(ware);

            int vortraege = 0;
            int funde = 0;

            for (int jahr = 0; jahr < Jahre; jahr++)
            {
                Spieler.SetProduktionsfertigkeit(ware, 0);
                Spieler.GetFertigkeitsschriftenSicher()[ware] = 0;
                SetzeTaler(100000);

                foreach (string meldung in await Manager.FuehreJahresereignisseDurch())
                {
                    if (meldung.Contains("Vortrag") || meldung.Contains("Schwätzer"))
                        vortraege++;
                    else if (meldung.Contains("Meisterschrift"))
                        funde++;
                }
            }

            // Grosszuegige Bänder um die erwarteten 12 % bzw. 4 %: Geprüft wird, dass beide Wege
            // überhaupt und in der richtigen Größenordnung feuern, nicht der genaue Wurf.
            double vortragsRate = 100.0 * vortraege / Jahre;
            double fundRate = 100.0 * funde / Jahre;

            Assert.InRange(vortragsRate, ProduktionsfertigkeitManager.VortragChanceProzent - 3,
                           ProduktionsfertigkeitManager.VortragChanceProzent + 3);
            Assert.InRange(fundRate, ProduktionsfertigkeitManager.MeisterschriftFundChanceProzent - 2,
                           ProduktionsfertigkeitManager.MeisterschriftFundChanceProzent + 2);
        }

        /// <summary>
        /// Ohne eigene Produktion gibt es weder Einladung noch Fund: Beide Wege hängen an Waren, die
        /// der Spieler tatsächlich betreibt. Das ist zugleich die Zusicherung, dass ein Spieler ohne
        /// Werkstätte zu Zugbeginn nicht mit Meldungen behelligt wird.
        /// </summary>
        [Fact]
        public async Task Ohne_Produktion_kommt_weder_Einladung_noch_Fund()
        {
            TestSpielwelt.Starte(seed: 20260908);
            SetzeTaler(100000);

            for (int jahr = 0; jahr < 500; jahr++)
                Assert.Empty(await Manager.FuehreJahresereignisseDurch());
        }

        #endregion

        #region Anzeige

        /// <summary>
        /// Die Wortleiter deckt die ganze Skala ab und ist monoton: Kein höherer Wert darf ein
        /// schlechteres Wort ergeben, und die Enden müssen erreichbar sein.
        /// </summary>
        [Fact]
        public void Die_Wortleiter_deckt_die_ganze_Skala_ab()
        {
            Assert.Equal("unkundig", ProduktionsfertigkeitManager.FertigkeitAlsText(0));
            Assert.Equal("vollendet",
                         ProduktionsfertigkeitManager.FertigkeitAlsText(HumSpieler.MaxProduktionsfertigkeit));

            string vorher = ProduktionsfertigkeitManager.FertigkeitAlsText(0);
            int stufen = 1;

            for (int wert = 1; wert <= HumSpieler.MaxProduktionsfertigkeit; wert++)
            {
                string jetzt = ProduktionsfertigkeitManager.FertigkeitAlsText(wert);

                if (jetzt != vorher)
                {
                    stufen++;
                    vorher = jetzt;
                }
            }

            Assert.Equal(7, stufen);
        }

        #endregion
    }
}
