using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Balancing des Warenhandels: Der Preis soll auf Übersättigung eines Marktes reagieren, damit
    /// mehrere Kontore nicht beliebig skalieren. Gemessen wird der Abschlag in Jahren lokalen Bedarfs
    /// (<c>Einwohner / 10</c>), nicht in festen Talern – so skaliert er mit der Stadtgröße.
    /// </summary>
    public class HandelsbalancingTests
    {
        private const int Korn = 1;

        /// <summary>Stadt 1 („Frozen Castle") hat 5 000 Einwohner, also 500 Stück Jahresbedarf.</summary>
        private const int GrosseStadt = 1;

        /// <summary>Stadt 2 („Icepike") hat 2 500 Einwohner, also 250 Stück Jahresbedarf.</summary>
        private const int KleineStadt = 2;

        /// <summary>Basispreis, den die Tests setzen – innerhalb von Korns Korridor [7, 20].</summary>
        private const int Basispreis = 8;

        private static Stadt BereiteStadtVor(int stadtId, int vorrat)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);
            stadt.SetRohstoffPreisVonIDXToY(Korn, Basispreis);
            stadt.SetRohstoffVorratWithIDXToY(Korn, vorrat);
            return stadt;
        }

        [Fact]
        public void Ohne_Vorrat_bleibt_der_Basispreis_unveraendert()
        {
            TestSpielwelt.Starte();

            Assert.Equal(Basispreis, BereiteStadtVor(GrosseStadt, 0).GetRohstoffPreisVonIDX(Korn));
        }

        [Fact]
        public void Ein_Jahresbedarf_Vorrat_kostet_genau_einen_Abschlagsschritt()
        {
            TestSpielwelt.Starte();

            // 500 Stück = ein Jahresbedarf von Stadt 1 => 10 % Abschlag => 8 * 90 / 100 = 7.
            Assert.Equal(7, BereiteStadtVor(GrosseStadt, 500).GetRohstoffPreisVonIDX(Korn));
        }

        [Fact]
        public void Der_Abschlag_ist_gedeckelt()
        {
            TestSpielwelt.Starte();

            // Selbst bei absurdem Vorrat greift nur MaxAbschlagProzent => 8 * 50 / 100 = 4.
            Assert.Equal(4, BereiteStadtVor(GrosseStadt, 1_000_000).GetRohstoffPreisVonIDX(Korn));
        }

        /// <summary>
        /// Die bewusste Verhaltensänderung: Früher klemmte der Getter bei <c>preisMin</c> und machte den
        /// Mengenabschlag damit wirkungslos. Jetzt begrenzt nur noch <c>MaxAbschlagProzent</c>.
        /// </summary>
        [Fact]
        public void Bei_Uebersaettigung_faellt_der_Preis_unter_den_Mindestpreis()
        {
            TestSpielwelt.Starte();

            int preis = BereiteStadtVor(GrosseStadt, 1_000_000).GetRohstoffPreisVonIDX(Korn);

            Assert.True(preis < SW.Dynamisch.GetRohstoffwithID(Korn).GetPreisMin(),
                        "Der Mengenabschlag muss unter preisMin wirken dürfen, sonst bleibt er folgenlos.");
        }

        [Fact]
        public void Eine_groessere_Stadt_verkraftet_denselben_Vorrat_besser()
        {
            TestSpielwelt.Starte();

            int gross = BereiteStadtVor(GrosseStadt, 500).GetRohstoffPreisVonIDX(Korn);
            int klein = BereiteStadtVor(KleineStadt, 500).GetRohstoffPreisVonIDX(Korn);

            // Gleicher Vorrat, halber Bedarf => doppelter Abschlag: 10 % gegen 20 %.
            Assert.Equal(7, gross);
            Assert.Equal(6, klein);
        }
    }
}
