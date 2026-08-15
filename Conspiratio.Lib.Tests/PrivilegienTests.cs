using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Amtsprivilegien und ihre Übernahme durch eine Erpressung (Issue #13).
    /// </summary>
    public class PrivilegienTests
    {
        private const int AmtBuergermeister = 7;
        private const int PrivEinkommen = 5;          // passiv
        private const int PrivUntergebene = 6;        // passiv, zusätzlich an vorhandene Untergebene gebunden
        private const int PrivUmsatzsteuer = 14;      // aktiv (Bürgermeister)

        /// <summary>
        /// Der wichtigste Test dieser Datei: <c>GetAmtsPrivilegien</c> spiegelt die Amtsbedingungen aus
        /// <c>PrivilegienAktualisieren</c>. Beide Stellen müssen dieselbe Antwort geben – dieser
        /// Vergleich über alle Ämter hat die Gleichwertigkeit belegt und schützt vor späterem Auseinanderlaufen.
        /// </summary>
        [Fact]
        public void GetAmtsPrivilegien_deckt_sich_mit_PrivilegienAktualisieren()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();

            for (int amtId = 1; amtId < SW.Statisch.GetMaxAmtID(); amtId++)
            {
                spieler.SetAmt(amtId, 1);
                spieler.GetErpressungen().Clear();      // kein Privilegienentzug in diesem Vergleich
                SW.Dynamisch.PrivilegienAktualisieren();

                foreach (int privilegId in SW.Dynamisch.GetAmtsPrivilegien(1))
                {
                    Assert.True(spieler.CheckPrivilegX(privilegId),
                        $"Amt {amtId}: Privileg {privilegId} steht in GetAmtsPrivilegien, wurde aber nicht gesetzt.");
                }
            }
        }

        /// <summary>
        /// „Untergebene" hängt nicht nur am Amt, sondern daran, ob es überhaupt Untergebene gibt. Beim
        /// Wechsel in ein Amt ohne Untergebene muss das Privileg wieder verschwinden – zuvor blieb ein
        /// einmal gesetztes true stehen, weil dieser Fall keine Zuweisung hatte.
        /// </summary>
        [Fact]
        public void Untergebenen_Privileg_verschwindet_im_Amt_ohne_Untergebene()
        {
            TestSpielwelt.Starte();
            var spieler = SW.Dynamisch.GetAktHum();

            int mitUntergebenen = 0;
            int ohneUntergebene = 0;

            for (int amtId = 1; amtId < SW.Statisch.GetMaxAmtID(); amtId++)
            {
                spieler.SetAmt(amtId, 1);
                bool hatUntergebene = SW.Dynamisch.GetUntergebene(1)[0] != 0;

                if (hatUntergebene && mitUntergebenen == 0)
                    mitUntergebenen = amtId;
                else if (!hatUntergebene && ohneUntergebene == 0)
                    ohneUntergebene = amtId;
            }

            Assert.True(mitUntergebenen != 0 && ohneUntergebene != 0,
                "Für diesen Test braucht es je ein Amt mit und ohne Untergebene.");

            spieler.SetAmt(mitUntergebenen, 1);
            SW.Dynamisch.PrivilegienAktualisieren();
            Assert.True(spieler.CheckPrivilegX(PrivUntergebene));

            spieler.SetAmt(ohneUntergebene, 1);
            SW.Dynamisch.PrivilegienAktualisieren();
            Assert.False(spieler.CheckPrivilegX(PrivUntergebene));
        }

        [Fact]
        public void Ohne_Amt_gibt_es_keine_Amtsprivilegien()
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.GetAktHum().SetAmt(0, 0);

            Assert.Empty(SW.Dynamisch.GetAmtsPrivilegien(1));
        }

        /// <summary>
        /// Im Issue vereinbart: Der Erpresste verliert die aktiv nutzbaren Amtsprivilegien, seine
        /// passiven Vorteile bleiben ihm.
        /// </summary>
        [Fact]
        public void Erpresster_verliert_aktive_Amtsprivilegien_und_behaelt_passive()
        {
            TestSpielwelt.Starte(menschen: 2);

            var opfer = SW.Dynamisch.GetAktHum();
            opfer.SetAmt(AmtBuergermeister, 1);
            opfer.GetErpressungen().Clear();
            SW.Dynamisch.PrivilegienAktualisieren();

            Assert.True(opfer.CheckPrivilegX(PrivUmsatzsteuer));
            Assert.True(opfer.CheckPrivilegX(PrivEinkommen));

            // Der zweite Mensch erpresst den aktiven Spieler.
            SW.Dynamisch.GetHumWithID(2).ErpressungAnlegen(1, SW.Dynamisch.GetAktuellesJahr() + 3);
            SW.Dynamisch.PrivilegienAktualisieren();

            Assert.False(opfer.CheckPrivilegX(PrivUmsatzsteuer));
            Assert.True(opfer.CheckPrivilegX(PrivEinkommen));
        }

        [Fact]
        public void Aktive_und_passive_Amtsprivilegien_sind_unterscheidbar()
        {
            TestSpielwelt.Starte();

            Assert.True(SW.Dynamisch.IstAktivesAmtsPrivileg(PrivUmsatzsteuer));
            Assert.False(SW.Dynamisch.IstAktivesAmtsPrivileg(PrivEinkommen));
        }

        [Fact]
        public void Erpresser_sieht_die_Amtsprivilegien_seines_Opfers_und_den_Rueckschalter()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);

            var eintraege = new PrivilegienManager().GetErpresstePrivilegien(zielId).Select(p => p.Id).ToList();

            Assert.Contains(PrivilegienManager.EigenePrivilegienId, eintraege);
            Assert.Contains(PrivUmsatzsteuer, eintraege);

            // „Amt niederlegen" (2) gehört bewusst nicht dazu – ein fremdes Amt legt man nicht nieder.
            Assert.DoesNotContain(2, eintraege);
        }

        [Fact]
        public void Erpressungs_Eintraege_lassen_sich_ihrem_Opfer_zuordnen()
        {
            const int opferId = 42;
            int eintragsId = PrivilegienManager.ErpressungPrivilegBasisId + opferId;

            Assert.Equal(opferId, PrivilegienManager.GetErpressungsOpferId(eintragsId));

            // Echte Privilegien und der Rückschalter sind keine Erpressungs-Einträge.
            Assert.Equal(0, PrivilegienManager.GetErpressungsOpferId(PrivUmsatzsteuer));
            Assert.Equal(0, PrivilegienManager.GetErpressungsOpferId(PrivilegienManager.EigenePrivilegienId));
        }

        [Fact]
        public void Laufende_Erpressungen_erscheinen_in_der_eigenen_Privilegienliste()
        {
            TestSpielwelt.Starte();
            int zielId = TestSpielwelt.SetzeKiGegner(0, AmtBuergermeister);
            SW.Dynamisch.GetAktHum().ErpressungAnlegen(zielId, SW.Dynamisch.GetAktuellesJahr() + 2);

            var eintraege = new PrivilegienManager().GetPrivilegien().Select(p => p.Id);

            Assert.Contains(PrivilegienManager.ErpressungPrivilegBasisId + zielId, eintraege);
        }
    }
}
