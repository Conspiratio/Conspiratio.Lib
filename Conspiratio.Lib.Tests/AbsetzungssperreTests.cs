using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Privilegien.Untergebene;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Antragssperre für Amtsenthebungen (Spielrunden-Feedback): Ohne sie stellte dieselbe KI den
    /// Antrag Jahr für Jahr, sobald ihre Beziehung zum Amtsinhaber unter die Schwelle gerutscht war –
    /// und die rutscht nicht von allein zurück. Die Sperre gilt je Antragsteller und Opfer, ihre Dauer
    /// hängt an der eingestellten KI-Aggressivität.
    /// </summary>
    public class AbsetzungssperreTests
    {
        private const int Buergermeister = 1;
        private const int Gebiet = 1;

        /// <summary>Der Antragsteller in diesen Tests – irgendeine KI, die nicht das Opfer ist.</summary>
        private static int Antragsteller => SW.Statisch.GetMinKIID() + 5;

        [Fact]
        public void Der_erste_Antrag_geht_durch()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);

            Assert.Single(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Fact]
        public void Derselbe_Antragsteller_kommt_im_Folgejahr_nicht_erneut_zum_Zug()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);
            LeereAlleVerfahren();

            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);

            Assert.Empty(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Fact]
        public void Nach_Ablauf_der_Sperrfrist_darf_er_wieder()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);
            LeereAlleVerfahren();

            for (int i = 0; i < SW.Dynamisch.GetAbsetzungssperreJahre(); i++)
                SW.Dynamisch.ErhoehAktuellesJahrUmEins();

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);

            Assert.Single(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Fact]
        public void Ein_anderer_Antragsteller_ist_nicht_mitgesperrt()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);
            LeereAlleVerfahren();

            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller + 1);

            Assert.Single(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Fact]
        public void Ein_anderes_Opfer_ist_nicht_mitgesperrt()
        {
            TestSpielwelt.Starte();
            int opferEins = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            int opferZwei = TestSpielwelt.SetzeKiGegner(2, Buergermeister, gebietId: Gebiet + 1);

            SW.Dynamisch.SetAmtsenthebungVonID(opferEins, Antragsteller);
            LeereAlleVerfahren();

            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            SW.Dynamisch.SetAmtsenthebungVonID(opferZwei, Antragsteller);

            Assert.Single(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        /// <summary>
        /// Das Gericht (Antragsteller 0) kennt keine Sperre – ein Urteil ist keine Laune eines Rivalen.
        /// Es begründet auch keine: Sonst machte eine Verurteilung das Opfer für Jahre unantastbar.
        /// </summary>
        [Fact]
        public void Das_Gericht_ist_von_der_Sperre_ausgenommen()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId);
            LeereAlleVerfahren();

            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            SW.Dynamisch.SetAmtsenthebungVonID(opferId);

            Assert.Single(new AmtsenthebungsManager().ErmittleVerfahren());
        }

        [Theory]
        [InlineData(1, 20)]
        [InlineData(50, 11)]
        [InlineData(100, 2)]
        public void Die_Sperrdauer_haengt_an_der_KI_Aggressivitaet(int prozent, int erwarteteJahre)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = prozent;

            Assert.Equal(erwarteteJahre, SW.Dynamisch.GetAbsetzungssperreJahre());
        }

        /// <summary>
        /// Die Sperrdauer wird bei jeder Prüfung neu aus der Einstellung gelesen, nicht beim Anlegen
        /// eingefroren – wer die Aggressivität im laufenden Spiel senkt, verlängert damit sofort alle
        /// offenen Sperren.
        /// </summary>
        [Fact]
        public void Eine_Aenderung_der_Aggressivitaet_wirkt_auf_bestehende_Sperren()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 100;  // 2 Jahre
            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);

            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            SW.Dynamisch.ErhoehAktuellesJahrUmEins();
            Assert.False(SW.Dynamisch.IstAbsetzungGesperrt(Antragsteller, opferId));

            SW.Dynamisch.Spielstand.Einstellungen.KiAggressivitaetProzent = 1;  // 20 Jahre
            Assert.True(SW.Dynamisch.IstAbsetzungGesperrt(Antragsteller, opferId));
        }

        /// <summary>
        /// Der Mensch unterliegt derselben Sperre – sonst wäre ihm als Dauerfeuer erlaubt, was der KI
        /// verwehrt ist. Die Meldung nennt das Jahr, damit die Sperre nicht als kaputter Knopf erscheint.
        /// </summary>
        [Fact]
        public void Auch_der_Mensch_ist_gesperrt_und_erfaehrt_es()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);
            var manager = new UntergebeneManager();

            manager.LeiteAmtsenthebungEin(opferId);
            LeereAlleVerfahren();
            SW.Dynamisch.ErhoehAktuellesJahrUmEins();

            string meldung = manager.LeiteAmtsenthebungEin(opferId);

            Assert.Empty(new AmtsenthebungsManager().ErmittleVerfahren());
            Assert.Contains("erst im Jahre", meldung);
        }

        /// <summary>Das Verfahren nennt, wer es angestrengt hat.</summary>
        [Fact]
        public void Das_Verfahren_nennt_den_Antragsteller()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId, Antragsteller);
            var verfahren = new AmtsenthebungsManager().ErmittleVerfahren()[0];

            Assert.Equal(Antragsteller, verfahren.AntragstellerId);
            Assert.NotEmpty(verfahren.AntragstellerName);
            Assert.Contains(verfahren.AntragstellerName, verfahren.GetAntragText());
        }

        /// <summary>
        /// Ein Antrag aus einem Spielstand vor der Einführung des Feldes hat Antragsteller 0. Der Text
        /// muss dann ohne Namen auskommen, statt mit einem führenden Leerzeichen zu beginnen.
        /// </summary>
        [Fact]
        public void Ohne_bekannten_Antragsteller_bleibt_der_Text_unpersoenlich()
        {
            TestSpielwelt.Starte();
            int opferId = TestSpielwelt.SetzeKiGegner(1, Buergermeister, gebietId: Gebiet);

            SW.Dynamisch.SetAmtsenthebungVonID(opferId);
            var verfahren = new AmtsenthebungsManager().ErmittleVerfahren()[0];

            Assert.Equal(0, verfahren.AntragstellerId);
            Assert.StartsWith("Eine Absetzung von", verfahren.GetAntragText());
        }

        /// <summary>
        /// Leert alle offenen Verfahren, ohne sie auszuwerten: Ein noch offener Antrag würde einen neuen
        /// schon durch die vorhandene Prüfung „läuft bereits“ abweisen, und dann prüfte der Test nicht
        /// die Sperre.
        /// </summary>
        private static void LeereAlleVerfahren()
        {
            for (int slot = 1; slot < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); slot++)
                SW.Dynamisch.SetAmtsenthebungDaten(slot, 0, 0, 0, 0);
        }
    }
}
