using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Ruhm verblaßt: Die Jahresabrechnung zieht `AnsehensverfallProzent` des Ansehens wieder ab.
    ///
    /// Ohne das war `PermaAnsehen` ein reines Konto. Die Auslastungsbelohnung zahlte jedes Jahr bis zu
    /// `AnsehenMaxProJahr` Punkte dafür, dass die Werke besetzt sind, und das Standesansehen eines
    /// schlichten Ritters stieg linear: 30 nach fünf Jahren, 80 nach fünfzehn, 205 nach vierzig. Die
    /// Gerichtsschwellen (80 und 30) waren danach bedeutungslos — sie sitzen aber richtig, denn der
    /// Median der 390 KI-Spieler liegt unverändert bei 29. Inflationiert ist nur der Mensch.
    ///
    /// Gemessen mit dem Verfall: Ein Händler ohne Amt pendelt sich bei einem Standesansehen von 55 ein,
    /// ein Herzog mit Amt bei knapp 300 — beide Schwellen unterscheiden also wieder.
    /// </summary>
    public class AnsehensverfallTests
    {
        /// <summary>
        /// Legt eine frische Welt an, setzt das Ansehen und rechnet ein Jahr ab.
        ///
        /// Verglichen werden immer <b>zwei</b> Läufe: Der Zufluss dieses Jahres (Hofhaltung, Auslastung)
        /// ist in beiden derselbe und fällt in der Differenz heraus. So braucht der Test keine Annahme
        /// darüber, was ein frischer Spieler nebenbei an Geltung gewinnt.
        /// </summary>
        private static int NachEinemJahr(int ansehenVorher)
        {
            TestSpielwelt.Starte();
            SW.Dynamisch.GetAktHum().SetPermaAnsehen(ansehenVorher);
            new AbrechnungsManager().ErstelleAbrechnungFuerAktivenSpieler();

            return SW.Dynamisch.GetAktHum().GetPermaAnsehen();
        }

        [Fact]
        public void Hohes_Ansehen_verfaellt_um_den_festgelegten_Anteil()
        {
            int mitAnsehen = NachEinemJahr(200);
            int ohneAnsehen = NachEinemJahr(0);

            int erwartet = 200 - 200 * AbrechnungsManager.AnsehensverfallProzent / 100;

            // Ein Taler Spielraum fuer die Ganzzahldivision des gemeinsamen Zuflusses.
            Assert.InRange(mitAnsehen - ohneAnsehen, erwartet - 1, erwartet + 1);
        }

        /// <summary>
        /// Kleine Namen verblassen nicht: Unterhalb von <c>100 / AnsehensverfallProzent</c> ergibt die
        /// Ganzzahldivision null. Das schützt den Anfänger, dessen Geltung sonst schneller schwände,
        /// als er sie aufbauen kann.
        /// </summary>
        [Fact]
        public void Kleines_Ansehen_verfaellt_nicht()
        {
            int schwelle = 100 / AbrechnungsManager.AnsehensverfallProzent;
            int klein = schwelle - 1;

            int mitAnsehen = NachEinemJahr(klein);
            int ohneAnsehen = NachEinemJahr(0);

            Assert.Equal(klein, mitAnsehen - ohneAnsehen);
        }

        /// <summary>Auch Schande verblaßt: Bei negativem Ansehen bewegt der Verfall den Wert zur Null.</summary>
        [Fact]
        public void Schande_verblasst_ebenfalls()
        {
            int mitSchande = NachEinemJahr(-200);
            int ohneAnsehen = NachEinemJahr(0);

            int erwartet = -200 + 200 * AbrechnungsManager.AnsehensverfallProzent / 100;

            Assert.InRange(mitSchande - ohneAnsehen, erwartet - 1, erwartet + 1);
        }

        /// <summary>
        /// Der eigentliche Zweck: Das Ansehen wächst nicht mehr ohne Ende, sondern läuft auf ein
        /// Gleichgewicht zu. Aus einem hohen Startwert sinkt es deutlich, bleibt aber positiv — es
        /// wandert also auf einen Punkt zu und nicht auf null.
        /// </summary>
        [Fact]
        public void Das_Ansehen_laeuft_auf_ein_Gleichgewicht_zu()
        {
            TestSpielwelt.Starte();

            var spieler = SW.Dynamisch.GetAktHum();
            spieler.SetPermaAnsehen(500);

            var abrechnung = new AbrechnungsManager();

            for (int jahr = 0; jahr < 50; jahr++)
                abrechnung.ErstelleAbrechnungFuerAktivenSpieler();

            int ansehen = spieler.GetPermaAnsehen();

            Assert.True(ansehen < 200, "Aus 500 muss das Ansehen deutlich gesunken sein, war: " + ansehen);
            Assert.True(ansehen >= 0, "Es darf nicht ins Negative kippen, war: " + ansehen);
        }
    }
}
