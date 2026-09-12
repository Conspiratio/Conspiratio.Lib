using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Der Geldleiher (Spielrunden-Feedback). Zwei Dinge waren schief: Die Angebotshöhe richtet sich nach
    /// dem Vermögen eines <em>zufälligen</em> Gläubigers und hatte mit dem Bedarf des Kreditnehmers nichts
    /// zu tun – annehmen ließ sie sich aber nur ganz, sodass man Zinsen auf Taler zahlte, die man gar
    /// nicht einsetzen konnte. Und Ablehnen kostete nichts und würfelte ein neues Angebot, Höhe und
    /// Zinssatz waren also beliebig oft nachziehbar.
    /// </summary>
    public class KreditTests
    {
        /// <summary>Ein Angebot mit festen Konditionen, damit die Tests nicht am Zufall hängen.</summary>
        private static KreditAngebot Angebot(int summe = 10_000, int zins = 10, int jahre = 5)
        {
            int kiId = SW.Statisch.GetMinKIID() + 1;
            SW.Dynamisch.GetKIwithID(kiId).SetTaler(1_000_000);

            return new KreditAngebot(kiId, SW.Dynamisch.GetKIwithID(kiId).GetName(), summe, zins, jahre);
        }

        [Fact]
        public void Ein_Teilbetrag_wird_auch_nur_als_Teilbetrag_gebucht()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int vorher = spieler.GetTaler();
            var angebot = Angebot();

            new SchreibstubeManager().NimmKredit(angebot, 2_600);

            Assert.Equal(vorher + 2_600, spieler.GetTaler());
            Assert.Equal(2_600, spieler.GetKreditMitID(spieler.GetEmptyKreditID() - 1).GetTaler());
        }

        /// <summary>Der Gläubiger gibt genau das her, was der Spieler nimmt – nicht die volle Summe.</summary>
        [Fact]
        public void Der_Glaeubiger_verliert_nur_den_genommenen_Betrag()
        {
            TestSpielwelt.Starte();
            var angebot = Angebot();
            int vorher = SW.Dynamisch.GetKIwithID(angebot.KiId).GetTaler();

            new SchreibstubeManager().NimmKredit(angebot, 2_600);

            Assert.Equal(vorher - 2_600, SW.Dynamisch.GetKIwithID(angebot.KiId).GetTaler());
        }

        [Fact]
        public void Mehr_als_angeboten_gibt_es_nicht()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int vorher = spieler.GetTaler();

            new SchreibstubeManager().NimmKredit(Angebot(summe: 3_000), 99_999);

            Assert.Equal(vorher + 3_000, spieler.GetTaler());
        }

        [Fact]
        public void Unter_dem_Mindestbetrag_wird_aufgerundet()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int vorher = spieler.GetTaler();

            new SchreibstubeManager().NimmKredit(Angebot(), 1);

            Assert.Equal(vorher + SchreibstubeManager.MindestKreditbetrag, spieler.GetTaler());
        }

        /// <summary>Die alte Überladung nimmt weiterhin die volle Summe.</summary>
        [Fact]
        public void Ohne_Betragsangabe_gilt_die_volle_Summe()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();
            int vorher = spieler.GetTaler();

            new SchreibstubeManager().NimmKredit(Angebot(summe: 4_200));

            Assert.Equal(vorher + 4_200, spieler.GetTaler());
        }

        [Fact]
        public void Nach_drei_Absagen_ist_der_Geldleiher_nicht_mehr_zu_sprechen()
        {
            TestSpielwelt.Starte();
            var manager = new SchreibstubeManager();

            Assert.Equal(SchreibstubeManager.MaxKreditAbsagenProZug, manager.GetVerbleibendeAbsagen());
            Assert.True(manager.KannKreditNehmen());

            for (int i = 0; i < SchreibstubeManager.MaxKreditAbsagenProZug; i++)
                manager.LehneKreditAb();

            Assert.Equal(0, manager.GetVerbleibendeAbsagen());
            Assert.False(manager.KannKreditNehmen());

            // Der Kreditplatz ist davon unberührt – gesperrt ist das Verhandeln, nicht das Leihen.
            Assert.True(manager.HatFreienKreditplatz());
        }

        [Fact]
        public void Der_naechste_Zug_gibt_die_Absagen_wieder_frei()
        {
            TestSpielwelt.Starte();
            var manager = new SchreibstubeManager();

            for (int i = 0; i < SchreibstubeManager.MaxKreditAbsagenProZug; i++)
                manager.LehneKreditAb();

            new RundenManager().BeginneZug();

            Assert.Equal(SchreibstubeManager.MaxKreditAbsagenProZug, manager.GetVerbleibendeAbsagen());
            Assert.True(manager.KannKreditNehmen());
        }

        /// <summary>
        /// Jede Absage hebt die Untergrenze des Zinswurfs, nicht die Obergrenze: Der günstige Satz wird
        /// unwahrscheinlicher, der schlechteste Fall bleibt derselbe. Über viele Würfe geprüft, weil eine
        /// einzelne Ziehung nichts über eine Verteilung aussagt.
        /// </summary>
        [Fact]
        public void Jede_Absage_macht_einen_guenstigen_Zins_unwahrscheinlicher()
        {
            TestSpielwelt.Starte(seed: 4711);
            var manager = new SchreibstubeManager();

            int guenstigOhneAbsage = ZaehleGuenstigeAngebote(manager, 400);

            for (int i = 0; i < SchreibstubeManager.MaxKreditAbsagenProZug; i++)
                manager.LehneKreditAb();

            int guenstigNachAbsagen = ZaehleGuenstigeAngebote(manager, 400);

            Assert.True(guenstigNachAbsagen < guenstigOhneAbsage,
                        "Nach drei Absagen gab es " + guenstigNachAbsagen + " günstige Angebote, vorher " +
                        guenstigOhneAbsage + " – die Untergrenze wirkt nicht.");
        }

        /// <summary>Der schlechteste Fall bleibt unverändert: Die Obergrenze rührt die Absage nicht an.</summary>
        [Fact]
        public void Die_Obergrenze_des_Zinses_bleibt_unveraendert()
        {
            TestSpielwelt.Starte(seed: 4711);
            var manager = new SchreibstubeManager();

            for (int i = 0; i < SchreibstubeManager.MaxKreditAbsagenProZug; i++)
                manager.LehneKreditAb();

            for (int i = 0; i < 400; i++)
                Assert.True(manager.ErstelleKreditAngebot().Zins <= SW.Statisch.GetKreditZinsMax());
        }

        private static int ZaehleGuenstigeAngebote(SchreibstubeManager manager, int wuerfe)
        {
            int guenstig = 0;

            for (int i = 0; i < wuerfe; i++)
                if (manager.ErstelleKreditAngebot().Zins <= SW.Statisch.GetKreditZinsMin())
                    guenstig++;

            return guenstig;
        }
    }
}
