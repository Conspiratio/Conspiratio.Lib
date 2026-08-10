using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Katastrophen (WinForms-Issue #37). Welche Stadt welche Art treffen kann, steuern die
    /// Anfälligkeiten aus den Spieldaten – das ist der Kern und entsprechend abgesichert.
    /// </summary>
    public class KatastrophenTests
    {
        /// <summary>Führt so lange Jahre durch, bis eine Katastrophe eintritt (oder gibt auf).</summary>
        private static KatastrophenErgebnis ErzwingeKatastrophe(int maxVersuche = 2000)
        {
            var manager = new KatastrophenManager();

            for (int versuch = 0; versuch < maxVersuche; versuch++)
            {
                var ergebnis = manager.FuehreKatastrophenDurch();

                if (ergebnis.Eingetreten)
                    return ergebnis;
            }

            return null;
        }

        /// <summary>Setzt für alle Städte die Anfälligkeiten – so lässt sich die Auswahl gezielt lenken.</summary>
        private static void SetzeAnfaelligkeitUeberall(int wert)
        {
            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                int[] anfaelligkeit = SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen();

                for (int art = 0; art < SW.Statisch.GetMaxKatastrohpen(); art++)
                    anfaelligkeit[art] = wert;
            }
        }

        [Fact]
        public void Nur_anfaellige_Staedte_werden_heimgesucht()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(0);

            // Genau eine Stadt ist anfällig, und zwar nur für die Pest.
            const int verwundbareStadt = 3;
            SW.Dynamisch.GetStadtwithID(verwundbareStadt).GetKatastrophen()[(int)EnumKatastrophe.Pest] = 100;

            for (int lauf = 0; lauf < 50; lauf++)
            {
                var ergebnis = ErzwingeKatastrophe();
                Assert.NotNull(ergebnis);

                Assert.Equal(EnumKatastrophe.Pest, ergebnis.Art);
                Assert.Equal(new[] { verwundbareStadt }, ergebnis.BetroffeneStaedte);
            }
        }

        [Fact]
        public void Ohne_jede_Anfaelligkeit_passiert_nichts()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(0);

            var manager = new KatastrophenManager();

            for (int jahr = 0; jahr < 500; jahr++)
                Assert.False(manager.FuehreKatastrophenDurch().Eingetreten);
        }

        [Fact]
        public void Eine_Grafschaft_trifft_genau_die_Staedte_ihres_Landes()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(100);

            for (int lauf = 0; lauf < 200; lauf++)
            {
                var ergebnis = ErzwingeKatastrophe();
                Assert.NotNull(ergebnis);

                if (ergebnis.Umfang != EnumKatastrophenumfang.Grafschaft)
                    continue;

                // Alle betroffenen Städte liegen im selben Land …
                var laender = ergebnis.BetroffeneStaedte
                    .Select(id => SW.Dynamisch.GetStadtwithID(id).GetLandID())
                    .Distinct()
                    .ToList();

                Assert.Single(laender);

                // … und es fehlt keine Stadt dieses Landes.
                var erwartet = new List<int>();
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    if (SW.Dynamisch.GetStadtwithID(stadtId).GetLandID() == laender[0])
                        erwartet.Add(stadtId);
                }

                Assert.Equal(erwartet.OrderBy(x => x), ergebnis.BetroffeneStaedte.OrderBy(x => x));
                return;
            }

            Assert.Fail("In 200 Katastrophen kam keine auf Grafschaftsebene vor.");
        }

        [Fact]
        public void Eine_Reichskatastrophe_trifft_alle_anfaelligen_Staedte()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(100);

            int alleStaedte = SW.Statisch.GetMaxStadtID() - 1;

            for (int lauf = 0; lauf < 200; lauf++)
            {
                var ergebnis = ErzwingeKatastrophe();
                Assert.NotNull(ergebnis);

                if (ergebnis.Umfang != EnumKatastrophenumfang.Reich)
                    continue;

                Assert.Equal(alleStaedte, ergebnis.BetroffeneStaedte.Count);
                return;
            }

            Assert.Fail("In 200 Katastrophen kam keine auf Reichsebene vor.");
        }

        [Fact]
        public void Eine_Katastrophe_kostet_Einwohner_Reichtum_und_Vorraete_und_hebt_die_Preise()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(0);

            const int stadtId = 5;
            SW.Dynamisch.GetStadtwithID(stadtId).GetKatastrophen()[(int)EnumKatastrophe.Brand] = 100;

            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);
            stadt.SetEinwohnerAufX(10000);
            stadt.SetReichtumToX(8);
            stadt.SetRohstoffVorratWithIDXToY(1, 1000);

            // Preise sind je Ware auf Min/Max gedeckelt – vom Minimum aus ist Luft nach oben.
            stadt.SetRohstoffPreisVonIDXToY(1, SW.Dynamisch.GetRohstoffwithID(1).GetPreisMin());
            int preisVorher = stadt.GetRohstoffPreisVonIDX(1);

            var ergebnis = ErzwingeKatastrophe();
            Assert.NotNull(ergebnis);

            Assert.True(stadt.GetEinwohner() < 10000, "Einwohner müssten sinken");
            Assert.True(stadt.GetReichtum() < 8, "Reichtum müsste sinken");
            Assert.True(stadt.GetRohstoffIDXVorrat(1) < 1000, "Vorräte müssten vernichtet werden");
            Assert.True(stadt.GetRohstoffPreisVonIDX(1) > preisVorher, "Die Knappheit müsste die Preise heben");
            Assert.NotEmpty(ergebnis.Meldung);
        }

        /// <summary>
        /// Der Wohnsitz des Spielers in der betroffenen Stadt nimmt Schaden – der in einer verschonten
        /// nicht. Das Hausarray ist nach Stadt-ID indiziert (je Stadt höchstens ein Wohnsitz).
        /// </summary>
        [Fact]
        public void Der_Wohnsitz_leidet_nur_in_der_betroffenen_Stadt()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(0);

            const int getroffen = 5;
            const int verschont = 6;
            SW.Dynamisch.GetStadtwithID(getroffen).GetKatastrophen()[(int)EnumKatastrophe.Brand] = 100;

            var spieler = SW.Dynamisch.GetAktHum();

            var hausGetroffen = spieler.GetSpielerHatHausVonStadtAnArraystelle(getroffen);
            hausGetroffen.SetHausID(1);
            hausGetroffen.ZustandInProzent = 100;

            var hausVerschont = spieler.GetSpielerHatHausVonStadtAnArraystelle(verschont);
            hausVerschont.SetHausID(1);
            hausVerschont.ZustandInProzent = 100;

            var ergebnis = ErzwingeKatastrophe();
            Assert.NotNull(ergebnis);
            Assert.Equal(new[] { getroffen }, ergebnis.BetroffeneStaedte);

            Assert.True(hausGetroffen.ZustandInProzent < 100, "Der Wohnsitz müsste beschädigt sein");
            Assert.Equal(100, hausVerschont.ZustandInProzent);
            Assert.NotEmpty(ergebnis.SpielerMeldungen);
        }

        /// <summary>Verschonte Städte dürfen sich durch eine Katastrophe nicht verändern.</summary>
        [Fact]
        public void Verschonte_Staedte_bleiben_unberuehrt()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(0);

            const int getroffen = 5;
            const int verschont = 6;
            SW.Dynamisch.GetStadtwithID(getroffen).GetKatastrophen()[(int)EnumKatastrophe.Brand] = 100;

            var stadt = SW.Dynamisch.GetStadtwithID(verschont);
            stadt.SetEinwohnerAufX(9000);
            stadt.SetReichtumToX(7);
            stadt.SetRohstoffVorratWithIDXToY(1, 400);

            Assert.NotNull(ErzwingeKatastrophe());

            Assert.Equal(9000, stadt.GetEinwohner());
            Assert.Equal(7, stadt.GetReichtum());
            Assert.Equal(400, stadt.GetRohstoffIDXVorrat(1));
        }

        [Fact]
        public void Katastrophen_bleiben_selten()
        {
            TestSpielwelt.Starte();
            SetzeAnfaelligkeitUeberall(100);

            var manager = new KatastrophenManager();
            int jahreMitKatastrophe = 0;
            const int jahre = 4000;

            for (int jahr = 0; jahr < jahre; jahr++)
            {
                if (manager.FuehreKatastrophenDurch().Eingetreten)
                    jahreMitKatastrophe++;
            }

            int quote = jahreMitKatastrophe * 100 / jahre;

            // Ausgelegt auf etwa alle vier bis fünf Jahre; die Schranken lassen Spielraum für Zufall.
            Assert.InRange(quote, 15, 30);
        }
    }
}
