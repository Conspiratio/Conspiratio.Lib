using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Anschwärzen (DynamischeSpieldaten.AnschwaerzenAusfuehren): entkoppelt vom UI-Zustand, damit
    /// sowohl ein menschlicher als auch ein KI-Täter es nutzen können. Beweise senken die
    /// Glaubwürdigkeits-Schwelle (Issue: aggressive KI).
    /// </summary>
    public class AnschwaerzenTests
    {
        [Fact]
        public void Ohne_Beweise_glaubt_der_Adressat_erst_ab_Beziehung_80()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int opfer = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);

            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 79);
            Assert.Contains("kein Wort", SW.Dynamisch.AnschwaerzenAusfuehren(menschId, opfer, adressat));

            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 80);
            Assert.Contains("Glauben", SW.Dynamisch.AnschwaerzenAusfuehren(menschId, opfer, adressat));
        }

        [Fact]
        public void Beweise_senken_die_Schwelle_beim_menschlichen_Taeter()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int opfer = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);

            TestSpielwelt.GibBeweise(opfer, 10); // min(10*3,30)=30 -> Schwelle 80-30=50
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 55);

            Assert.Contains("Glauben", SW.Dynamisch.AnschwaerzenAusfuehren(menschId, opfer, adressat));
        }

        [Fact]
        public void KI_Taeter_nutzt_die_Deliktpunkte_des_Opfers_als_Beweis()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);

            SW.Dynamisch.GetHumWithID(menschId).SetDeliktpunkte(10); // Opfer ist der Mensch
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(anklaeger, 55);

            Assert.Contains("Glauben", SW.Dynamisch.AnschwaerzenAusfuehren(anklaeger, menschId, adressat));
        }

        [Fact]
        public void Glaubt_der_Adressat_nicht_und_das_Opfer_ist_ein_Mensch_bleibt_es_folgenlos()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int anklaeger = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);

            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(anklaeger, 0);

            Assert.Null(SW.Dynamisch.AnschwaerzenAusfuehren(anklaeger, menschId, adressat));
        }

        [Fact]
        public void Glaubt_der_Adressat_nicht_und_das_Opfer_ist_eine_KI_wird_es_dem_Opfer_gemeldet()
        {
            TestSpielwelt.Starte();
            int menschId = SW.Dynamisch.GetAktiverSpieler();
            int opfer = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);

            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(menschId, 0);

            // Opfers Beziehung zum Täter startet zufällig (CreateRndBeziehungen); vorher/nachher
            // vergleichen statt auf einen absoluten Wert < 0 zu prüfen, den ErhoeheBeziehungZuX
            // ohnehin bei 0 kappt.
            int beziehungVorher = SW.Dynamisch.GetKIwithID(opfer).GetBeziehungZuKIX(menschId);

            string meldung = SW.Dynamisch.AnschwaerzenAusfuehren(menschId, opfer, adressat);

            Assert.Contains("kein Wort", meldung);
            Assert.True(SW.Dynamisch.GetKIwithID(opfer).GetBeziehungZuKIX(menschId) < beziehungVorher);
        }

        [Fact]
        public void Man_kann_niemanden_bei_sich_selbst_anschwaerzen()
        {
            TestSpielwelt.Starte();
            int opfer = TestSpielwelt.SetzeKiGegner(0, 0);

            string meldung = SW.Dynamisch.AnschwaerzenAusfuehren(SW.Dynamisch.GetAktiverSpieler(), opfer, opfer);
            Assert.Contains("sich selbst", meldung);
        }

        [Fact]
        public void Der_UI_Wrapper_funktioniert_weiterhin_zweistufig()
        {
            TestSpielwelt.Starte();
            int opfer = TestSpielwelt.SetzeKiGegner(0, 0);
            int adressat = TestSpielwelt.SetzeKiGegner(1, 0);
            SW.Dynamisch.GetKIwithID(adressat).SetBeziehungZuX(SW.Dynamisch.GetAktiverSpieler(), 100);

            // Adressats Beziehung zum Opfer startet zufällig (CreateRndBeziehungen); vorher/nachher
            // vergleichen statt auf einen absoluten Wert < 0 zu prüfen, den ErhoeheBeziehungZuX
            // ohnehin bei 0 kappt.
            int beziehungVorher = SW.Dynamisch.GetKIwithID(adressat).GetBeziehungZuKIX(opfer);

            Assert.Equal(0, SW.Dynamisch.GetAnschwaerzID());
            SW.Dynamisch.Anschwaerzen(opfer);
            Assert.Equal(opfer, SW.Dynamisch.GetAnschwaerzID());

            SW.Dynamisch.Anschwaerzen(adressat);
            Assert.Equal(0, SW.Dynamisch.GetAnschwaerzID()); // zweiter Schritt setzt zurueck
            Assert.True(SW.Dynamisch.GetKIwithID(adressat).GetBeziehungZuKIX(opfer) < beziehungVorher); // Wirkung kam an
        }
    }
}
