using System;
using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Gesetz #3 "Maximale Taler" ist die einzige vermoegensproportionale Bremse des Spiels. Geprueft
    /// wird, dass sie am Gesamtvermoegen haengt (frueher nur an der Barschaft - wer sein Vermoegen in
    /// Betrieben und Waren hielt, war schlicht ausgenommen), dass sie vorher warnt und dass sie nie
    /// mehr pfaendet, als an Bargeld da ist.
    ///
    /// Die Grenze setzen die Tests selbst (<c>SetGesetzXtoY</c>) und leiten sie aus dem tatsaechlichen
    /// Vermoegen des Startspielers ab - so haengen sie nicht am jeweiligen Balancing-Standardwert.
    /// </summary>
    public class VermoegensgesetzTests
    {
        private const int SpielerId = 1;

        private static int Grenze => SW.Dynamisch.GetGesetzX(3) * 100000;

        private static int Gesamtvermoegen => SW.Dynamisch.GetAktHum().GetGesamtVermoegen(SpielerId);

        /// <summary>
        /// Schaltet alle Werkstattplaetze des Spielers frei. Sie sind das einzige Sachvermoegen, das
        /// sich im Test frei aufbauen laesst - Warenlager deckelt <c>SetStadtRohstoffAnzahl</c> am
        /// vorhandenen Lagerplatz.
        /// </summary>
        private static void SetzeAlleWerkstaetten()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
                    spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).SetEnabled(true);
        }

        /// <summary>Das Vermoegen ohne Bargeld - Wohnsitz und (sofern gesetzt) Werkstaetten.</summary>
        private static int Sachwerte()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int barschaft = spieler.GetTaler();

            spieler.SetTaler(0);
            int sachwerte = spieler.GetGesamtVermoegen(SpielerId);
            spieler.SetTaler(barschaft);

            return sachwerte;
        }

        /// <summary>Setzt die gesetzliche Grenze auf den naechsten Hunderttausender ueber <paramref name="taler"/>.</summary>
        private static void SetzeGrenzeAuf(int taler)
        {
            SW.Dynamisch.SetGesetzXtoY(3, Math.Max(1, (taler + 99999) / 100000));
        }

        private static List<string> PruefeUndSammle()
        {
            return new ZugNachrichtenManager().PruefeVerbrechen().ToList();
        }

        /// <summary>
        /// Der Kern der Aenderung: Barschaft unter der Grenze, Gesamtvermoegen darueber. Frueher blieb
        /// das folgenlos, obwohl die Strafe schon immer am Gesamtvermoegen bemessen wurde.
        /// </summary>
        [Fact]
        public void Vermoegen_in_Sachwerten_schuetzt_nicht_mehr_vor_der_Grenze()
        {
            TestSpielwelt.Starte();
            SetzeAlleWerkstaetten();

            int sachwerte = Sachwerte();
            SetzeGrenzeAuf(sachwerte * 2);
            SW.Dynamisch.GetAktHum().SetTaler(Grenze - sachwerte / 2);

            Assert.True(SW.Dynamisch.GetAktHum().GetTaler() < Grenze, "Voraussetzung: Barschaft unter der Grenze");
            Assert.True(Gesamtvermoegen > Grenze, "Voraussetzung: Gesamtvermoegen darueber");

            Assert.Contains(PruefeUndSammle(), m => m.Contains("Strafe"));
        }

        /// <summary>
        /// Strafe und Schuldenprozess laufen im Client im selben Zugbeginn ab - der Spieler kann
        /// zwischen beiden nichts verkaufen. Die Strafe darf ihn deshalb nicht allein ins Minus und
        /// damit vor die Glaeubiger treiben.
        /// </summary>
        [Fact]
        public void Die_Strafe_uebersteigt_nie_die_Barschaft()
        {
            TestSpielwelt.Starte();
            SetzeAlleWerkstaetten();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000);
            SetzeGrenzeAuf(Sachwerte() / 2);

            Assert.True(Gesamtvermoegen * 15 / 100 > spieler.GetTaler(), "Voraussetzung: volle Strafe uebersteigt die Barschaft");

            PruefeUndSammle();

            Assert.Equal(0, spieler.GetTaler());
        }

        /// <summary>Reicht das Bargeld, werden die vollen 15 Prozent des Gesamtvermoegens faellig.</summary>
        [Fact]
        public void Bei_genug_Bargeld_werden_fuenfzehn_Prozent_faellig()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetTaler(1000000);
            SetzeGrenzeAuf(100000);

            int vermoegenVorher = Gesamtvermoegen;
            int barschaftVorher = spieler.GetTaler();

            PruefeUndSammle();

            Assert.Equal(barschaftVorher - vermoegenVorher * 15 / 100, spieler.GetTaler());
        }

        /// <summary>
        /// Die Warnung ist der Teil, der die Regel fair macht: Sie kommt, bevor die Strafe faellig wird,
        /// sodass der Spieler noch eine volle Runde Zeit hat, zu verkaufen oder anzulegen.
        /// </summary>
        [Fact]
        public void Knapp_unter_der_Grenze_kommt_eine_Warnung_ohne_Strafe()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            SetzeGrenzeAuf(1000000);
            spieler.SetTaler(Grenze * 9 / 10 - Sachwerte());

            int barschaftVorher = spieler.GetTaler();
            var meldungen = PruefeUndSammle();

            Assert.NotEmpty(meldungen);
            Assert.DoesNotContain(meldungen, m => m.Contains("Strafe"));
            Assert.Equal(barschaftVorher, spieler.GetTaler());
        }

        /// <summary>Ein Einsteiger weit unterhalb der Grenze soll von alldem nichts mitbekommen.</summary>
        [Fact]
        public void Weit_unter_der_Grenze_bleibt_alles_still()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            SetzeGrenzeAuf(Gesamtvermoegen * 4);

            int barschaftVorher = spieler.GetTaler();

            Assert.Empty(PruefeUndSammle());
            Assert.Equal(barschaftVorher, spieler.GetTaler());
        }
    }
}
