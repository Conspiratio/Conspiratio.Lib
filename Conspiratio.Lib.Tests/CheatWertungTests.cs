using System.IO;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Einstellungen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Ein Spiel mit aktivem Cheatmodus wird nicht gewertet (Spielrunden-Feedback): weder in der
    /// spielübergreifenden Profilstatistik noch in der Auftrags-Bestenliste. In beiden werden Spiele
    /// miteinander verglichen, und in beiden ist der Eintrag nicht rückgängig zu machen – ein einziges
    /// erschummeltes Spiel verdürbe die Zahlen dauerhaft.
    /// </summary>
    public class CheatWertungTests
    {
        /// <summary>Ein eigenes Verzeichnis je Testlauf, damit die Tests nicht übereinander schreiben.</summary>
        private static string FrischesVerzeichnis()
        {
            string pfad = Path.Combine(Path.GetTempPath(), "conspiratio-cheatwertung-" + Path.GetRandomFileName());
            Directory.CreateDirectory(pfad);

            return pfad;
        }

        [Fact]
        public void Ohne_Cheatmodus_wird_das_Spiel_ins_Profil_gewertet()
        {
            Assert.Equal(1, WerteSpielUndLiesSpielezahl(cheatmodus: false));
        }

        [Fact]
        public void Mit_Cheatmodus_bleibt_das_Profil_unberuehrt()
        {
            Assert.Equal(0, WerteSpielUndLiesSpielezahl(cheatmodus: true));
        }

        /// <summary>
        /// Legt ein Spiel samt Profil an, wertet es und liefert die Zahl der im Profil gezählten Spiele.
        /// Beide Fälle laufen über denselben Weg, damit der Unterschied wirklich nur am Cheatmodus hängt.
        /// </summary>
        private static int WerteSpielUndLiesSpielezahl(bool cheatmodus)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.Cheatmodus = cheatmodus;

            string verzeichnis = FrischesVerzeichnis();
            var manager = new ProfilManager(verzeichnis);
            var profil = manager.ErstelleProfil("Prüfer");

            var mensch = SW.Dynamisch.GetAktHum();
            mensch.ProfilId = profil.Id;
            mensch.GetSpielerStatistik().HiBestechungen = 7;

            manager.WerteLaufendesSpiel();

            // Neu einlesen statt das Objekt im Speicher zu befragen: Geprüft wird, was auf der Platte steht.
            return new ProfilManager(verzeichnis).FindeProfil(profil.Id).Meta.SpieleGesamt;
        }

        [Fact]
        public void Ohne_Cheatmodus_landet_der_Auftrag_in_der_Bestenliste()
        {
            Assert.Equal(1, TrageAuftragEinUndZaehle(cheatmodus: false));
        }

        [Fact]
        public void Mit_Cheatmodus_bleibt_die_Bestenliste_leer()
        {
            Assert.Equal(0, TrageAuftragEinUndZaehle(cheatmodus: true));
        }

        private static int TrageAuftragEinUndZaehle(bool cheatmodus)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.Cheatmodus = cheatmodus;

            string verzeichnis = FrischesVerzeichnis();

            new HighscoreManager(verzeichnis).FuegeEintragHinzu(
                EnumAuftrag.KleinerWohlstand, "Prüfer", spieljahr: 1610, jahreGespielt: 10, mitspielerAnzahl: 1);

            return new HighscoreManager(verzeichnis).GetEintraege(EnumAuftrag.KleinerWohlstand).Count;
        }
    }
}
