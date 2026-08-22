using System.Collections.Generic;

// Die Kampfeinheiten liegen aus Savegame-Kompatibilitaetsgruenden im Namespace Conspiratio.Kampf.
using Conspiratio.Kampf;

using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

// "Kampf" ist zugleich der Namespace und der Klassenname; unqualifiziert wäre der Typ mehrdeutig.
using KampfObjekt = Conspiratio.Lib.Gameplay.Kampf.Kampf;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Wirkung der Karawanen-Sicherheit auf Überfälle: geringere Angriffswahrscheinlichkeit und, falls es doch
    /// zu einem Kampf kommt, Moralbonus und Zusatztruppen für eine bereits vorhandene Verteidigung.
    /// </summary>
    public class KarawanenSicherheitTests
    {
        private const int LandWattern = 1;
        private const int StadtInWattern = 1;  // Laender[1].GetStadtX(0)

        [Theory]
        [InlineData(20, 1.0)]   // Billigste Stufe: unveränderte Wahrscheinlichkeit (Referenzwert)
        [InlineData(30, 0.75)]  // Mittlere Stufe: 25 % geringeres Risiko
        [InlineData(40, 0.5)]   // Teuerste Stufe: halbes Risiko
        public void ErmittleAngriffswahrscheinlichkeitsfaktor_sinkt_mit_steigender_Sicherheit(int sicherheit, double erwarteterFaktor)
        {
            Assert.Equal(erwarteterFaktor, Kampfberechnung.ErmittleAngriffswahrscheinlichkeitsfaktor(sicherheit), 3);
        }

        [Fact]
        public void GetMinSicherheitVerkaufenderKarawanenInLand_liefert_Referenzwert_ohne_verkaufende_Karawane()
        {
            TestSpielwelt.Starte();

            int minSicherheit = SW.Dynamisch.GetMinSicherheitVerkaufenderKarawanenInLand(LandWattern);

            Assert.Equal(20, minSicherheit);
        }

        [Fact]
        public void GetMinSicherheitVerkaufenderKarawanenInLand_findet_die_Sicherheit_der_verkaufenden_Karawane()
        {
            TestSpielwelt.Starte();
            RichteVerkaufendeKarawaneEin(karawaneID: 2);  // Sicherheit 40

            int minSicherheit = SW.Dynamisch.GetMinSicherheitVerkaufenderKarawanenInLand(LandWattern);

            Assert.Equal(40, minSicherheit);
        }

        [Fact]
        public void ErhoeheVerteidigungDurchKarawanenSicherheit_wirkt_nicht_bei_billigster_Stufe()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfMitVerteidigung(karawaneID: 0, moralVerteidiger: 50);  // Sicherheit 20
            int truppenVorher = kampf.TruppenVerteidiger.Count;

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            Assert.Equal(50, kampf.MoralVerteidiger);
            Assert.Equal(truppenVorher, kampf.TruppenVerteidiger.Count);
        }

        [Fact]
        public void ErhoeheVerteidigungDurchKarawanenSicherheit_gibt_Moralbonus_und_Zusatztruppen_bei_teuerster_Stufe()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfMitVerteidigung(karawaneID: 2, moralVerteidiger: 50);  // Sicherheit 40
            int truppenVorher = kampf.TruppenVerteidiger.Count;

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            Assert.Equal(60, kampf.MoralVerteidiger);  // +10
            Assert.Equal(truppenVorher + 2, kampf.TruppenVerteidiger.Count);
            Assert.Equal(2, kampf.TruppenVerteidiger.FindAll(e => e is KarawanenWache).Count);
        }

        [Fact]
        public void ErhoeheVerteidigungDurchKarawanenSicherheit_deckelt_Moral_bei_100()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfMitVerteidigung(karawaneID: 2, moralVerteidiger: 95);  // Sicherheit 40, +10 würde 105 ergeben

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            Assert.Equal(100, kampf.MoralVerteidiger);
        }

        [Fact]
        public void Ohne_Patrouille_stellt_die_billigste_Karawane_keine_eigenen_Wachen()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfOhneVerteidigung(karawaneID: 0);  // Sicherheit 20

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            Assert.True(kampf.TruppenVerteidiger == null || kampf.TruppenVerteidiger.Count == 0);
            Assert.Equal(0, kampf.SpielerIDVerteidiger);
        }

        [Theory]
        [InlineData(1, 2, 50)]  // Sicherheit 30: 2 Wachen, Moral 50
        [InlineData(2, 4, 60)]  // Sicherheit 40: 4 Wachen, Moral 60
        public void Ohne_Patrouille_stellt_eine_gesicherte_Karawane_eigene_Wachen(int karawaneID, int erwarteteWachen, int erwarteteMoral)
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfOhneVerteidigung(karawaneID);

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            Assert.Equal(erwarteteWachen, kampf.TruppenVerteidiger.Count);
            Assert.Equal(erwarteteWachen, kampf.TruppenVerteidiger.FindAll(e => e is KarawanenWache).Count);
            Assert.Equal(erwarteteMoral, kampf.MoralVerteidiger);
        }

        [Fact]
        public void Die_eigenen_Wachen_verteidigen_fuer_den_Besitzer_der_Karawane()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfOhneVerteidigung(karawaneID: 2);

            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);

            // Damit der Sieg der Wachen die Beute beim Besitzer laesst und nicht beim Angreifer.
            Assert.Equal(kampf.Karawane.SpielerID, kampf.SpielerIDVerteidiger);
            Assert.Equal(0, kampf.StuetzpunktIDVerteidiger);  // Die Wachen gehoeren zu keinem Stuetzpunkt
        }

        [Fact]
        public void Eine_von_eigenen_Wachen_geschuetzte_Karawane_ergibt_sich_nicht_kampflos()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfOhneVerteidigung(karawaneID: 2);
            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);
            kampf.TruppenAngreifer.Add(new RaubRaeuber());

            var ergebnis = new Kampfberechnung().BerechneKampfErgebnis(kampf);

            Assert.DoesNotContain("keine Beschuetzer in Sicht", ergebnis.Zusammenfassung.Replace("ü", "ue"));
            Assert.Contains("Wachen", ergebnis.Zusammenfassung);
        }

        [Fact]
        public void Eine_ungesicherte_Karawane_ergibt_sich_weiterhin_kampflos()
        {
            TestSpielwelt.Starte();

            var kampf = ErzeugeKampfOhneVerteidigung(karawaneID: 0);
            Kampfberechnung.ErhoeheVerteidigungDurchKarawanenSicherheit(kampf);
            kampf.TruppenAngreifer.Add(new RaubRaeuber());

            var ergebnis = new Kampfberechnung().BerechneKampfErgebnis(kampf);

            Assert.Contains("keine Beschuetzer in Sicht", ergebnis.Zusammenfassung.Replace("ü", "ue"));
        }

        /// <summary>Ein Ueberfall ohne jede Patrouille in der Naehe: kein verteidigender Stuetzpunkt, keine Truppen.</summary>
        private static KampfObjekt ErzeugeKampfOhneVerteidigung(int karawaneID)
        {
            return new KampfObjekt
            {
                SpielerIDAngreifer = SW.Statisch.GetMinKIID(),
                SpielerIDVerteidiger = 0,
                MoralAngreifer = 50,
                MoralVerteidiger = 0,
                TruppenAngreifer = new List<Einheit>(),
                TruppenVerteidiger = null,
                StuetzpunktIDAngreifer = 1,
                StuetzpunktIDVerteidiger = 0,
                AktionIndexAngreifer = 0,
                AktionIndexVerteidiger = 0,
                LandID = LandWattern,
                KampfArt = EnumKampfArt.KarawanenPluenderung,
                Karawane = new KampfKarawane(1, StadtInWattern, karawaneID, 0, 0, 0, 0)
            };
        }

        private static KampfObjekt ErzeugeKampfMitVerteidigung(int karawaneID, int moralVerteidiger)
        {
            return new KampfObjekt
            {
                SpielerIDAngreifer = 100,
                SpielerIDVerteidiger = 1,
                MoralAngreifer = 50,
                MoralVerteidiger = moralVerteidiger,
                TruppenAngreifer = new List<Einheit>(),
                TruppenVerteidiger = new List<Einheit>(),
                StuetzpunktIDAngreifer = 1,
                StuetzpunktIDVerteidiger = 1,
                AktionIndexAngreifer = 0,
                AktionIndexVerteidiger = 0,
                LandID = LandWattern,
                KampfArt = EnumKampfArt.KarawanenPluenderung,
                Karawane = new KampfKarawane(1, StadtInWattern, karawaneID, 0, 0, 0, 0)
            };
        }

        /// <summary>Setzt beim aktiven Spieler eine Werkstatt in <see cref="StadtInWattern"/> auf permanenten Verkauf mit der gewünschten Karawane.</summary>
        private static void RichteVerkaufendeKarawaneEin(int karawaneID)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.GetSpielerHatInStadtXWerkstaettenY(1, StadtInWattern).SetEnabled(true);
            spieler.SetKarawaneInStadtXzuY(StadtInWattern, karawaneID);

            var slot = spieler.GetProduktionsslot(StadtInWattern, 0);
            slot.SetTaetigkeit((int)EnumProduktionsslotAktionsart.PermanentVerkaufen);
            slot.SetVerkaufAnzahl(50);
            slot.SetVerkaufRohstoff(1);
        }
    }
}
