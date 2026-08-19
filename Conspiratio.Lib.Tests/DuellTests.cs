using System.Runtime.Serialization;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>Auswertung eines Duells (Issue #17) und die Folgen für den Verlierer.</summary>
    public class DuellTests
    {
        private const int AmtBuergermeister = 7;

        [Fact]
        public void Der_Verlierer_verliert_Gesundheit_und_unter_der_Schwelle_sein_Amt()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            // Schaden ist 30–50: Aus 35 Gesundheit wird sicher weniger als die Schwelle von 30.
            SW.Dynamisch.GetSpWithID(zielId).SetGesundheit(35);

            var ergebnis = new FechtDuellManager().WendeDuellAusgangAn(zielId, spielerGewinnt: true);

            Assert.True(ergebnis.SpielerHatGewonnen);
            Assert.True(ergebnis.AmtVerloren);
            Assert.NotEmpty(ergebnis.AmtName);
            Assert.NotEmpty(ergebnis.GegnerName);
            Assert.True(SW.Dynamisch.GetSpWithID(zielId).GetGesundheit() < FechtDuellManager.AmtsverlustSchwelle);
            Assert.Equal(0, SW.Dynamisch.GetSpWithID(zielId).GetAmtID());
        }

        [Fact]
        public void Bei_voller_Gesundheit_bleibt_das_Amt_erhalten()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            SW.Dynamisch.GetSpWithID(zielId).SetGesundheit(SW.Statisch.GetMaxGesundheit());

            var ergebnis = new FechtDuellManager().WendeDuellAusgangAn(zielId, spielerGewinnt: true);

            Assert.False(ergebnis.AmtVerloren);
            Assert.Equal(string.Empty, ergebnis.AmtName);
            Assert.Equal(AmtBuergermeister, SW.Dynamisch.GetSpWithID(zielId).GetAmtID());
        }

        [Fact]
        public void Verliert_der_Spieler_trifft_es_ihn_selbst()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetGesundheit(SW.Statisch.GetMaxGesundheit());

            var ergebnis = new FechtDuellManager().WendeDuellAusgangAn(zielId, spielerGewinnt: false);

            Assert.False(ergebnis.SpielerHatGewonnen);
            Assert.True(spieler.GetGesundheit() < SW.Statisch.GetMaxGesundheit());
            Assert.Equal(SW.Dynamisch.GetSpWithID(zielId).GetKompletterName(), ergebnis.GegnerName);
        }

        [Fact]
        public void Die_Siegchance_bleibt_zwischen_fuenf_und_fuenfundneunzig_Prozent()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            var manager = new FechtDuellManager();

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = 10000;
            Assert.Equal(95, manager.BerechneSiegchance(zielId));

            SW.Dynamisch.GetAktHum().Fechtfaehigkeit = -10000;
            Assert.Equal(5, manager.BerechneSiegchance(zielId));
        }
    }

    /// <summary>
    /// Spielstand-Verträglichkeit: Beim Laden werden Objekte ohne Konstruktoraufruf erzeugt
    /// (<see cref="FormatterServices.GetUninitializedObject"/>), nachträglich ergänzte Felder kommen
    /// also als null an. Die Zugriffe müssen das abfangen, sonst zerreißt es alte Spielstände.
    /// </summary>
    public class SpielstandKompatibilitaetTests
    {
        private static HumSpieler WieAusAltemSpielstand()
        {
            // Bewusst dieselbe (veraltete) API, die auch der SpielstandContractResolver benutzt – nur so
            // entsteht ein Objekt im selben Zustand wie beim Laden eines alten Spielstands.
#pragma warning disable SYSLIB0050
            return (HumSpieler)FormatterServices.GetUninitializedObject(typeof(HumSpieler));
#pragma warning restore SYSLIB0050
        }

        [Fact]
        public void Delikt_Speicher_wird_bei_Bedarf_angelegt()
        {
            TestSpielwelt.Starte();
            var spieler = WieAusAltemSpielstand();

            Assert.Equal(0, spieler.GetBegingVerbrechenX(0));
            Assert.Equal(SW.Statisch.GetMaxGesetze(), spieler.GetBegingVerbrechenX().Length);
        }

        [Fact]
        public void Erpressungsliste_wird_bei_Bedarf_angelegt()
        {
            TestSpielwelt.Starte();
            var spieler = WieAusAltemSpielstand();

            Assert.Empty(spieler.GetErpressungen());
            Assert.False(spieler.ErpresstBereits(5));

            spieler.ErpressungAnlegen(5, 1610);
            Assert.True(spieler.ErpresstBereits(5));
        }

        [Fact]
        public void Ahnentafel_wird_bei_Bedarf_angelegt()
        {
            TestSpielwelt.Starte();

            Assert.Empty(WieAusAltemSpielstand().GetAhnentafelListe());
        }

        [Fact]
        public void Gegnerische_Sabotage_wird_bei_Bedarf_angelegt()
        {
            TestSpielwelt.Starte();
            var spieler = WieAusAltemSpielstand();
            int kiId = SW.Statisch.GetMinKIID();

            Assert.Equal(0, spieler.GetGegnerischeSabotage(kiId).GetDauer());

            spieler.GetGegnerischeSabotage(kiId).SetDauer(5);
            Assert.Equal(5, spieler.GetGegnerischeSabotage(kiId).GetDauer());

            spieler.GegnerischeSabotageEntfernen(kiId);
            Assert.Equal(0, spieler.GetGegnerischeSabotage(kiId).GetDauer());
        }
    }

    /// <summary>
    /// Die KI-Feindseligkeits-Formel (Issue: aggressive KI) wird aus PruefeKiBeleidigtSpieler
    /// extrahiert, damit AggressionManager sie für Sabotage/Anschwärzen wiederverwenden kann.
    /// </summary>
    public class KiFeindseligkeitTests
    {
        [Theory]
        [InlineData(50, 0, 0)]    // neutrale Beziehung, keine Bosheit -> keine Chance
        [InlineData(0, 0, 6)]     // maximale Feindseligkeit (50), keine Bosheit: 50*13/100 = 6
        [InlineData(0, 100, 7)]   // wie oben plus volle Bosheit: (650+100)/100 = 7
        [InlineData(80, 100, 1)]  // gute Beziehung, aber hohe Bosheit: (0*13+100)/100 = 1
        public void Folgt_der_Formel(int beziehung, int bosheit, int erwarteteChance)
        {
            Assert.Equal(erwarteteChance, FechtDuellManager.BerechneKiFeindseligkeitChance(beziehung, bosheit));
        }
    }
}
