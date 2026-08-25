using Conspiratio.Lib.Gameplay.Spielwelt;

using Xunit;

namespace Conspiratio.Lib.Tests
{
    /// <summary>
    /// Die Gesetzestafel und ihre Grenzen muessen zueinander passen. Der Startwert eines Gesetzes
    /// liegt sonst ausserhalb dessen, was ueberhaupt einstellbar ist, und die erste
    /// <see cref="DynamischeSpieldaten.GesetzXschaltenUmY"/> zieht ihn stillschweigend in den
    /// erlaubten Bereich - auch dann, wenn gar nichts geaendert werden sollte.
    /// </summary>
    public class GesetzesgrenzenTests
    {
        /// <summary>
        /// Die Tafel hat feste Kapazitaet 100 und ist duenn besetzt; ungenutzte Plaetze haben
        /// Startwert 0 und Grenzen 0..0 und erfuellen die Zusicherung damit von selbst.
        /// </summary>
        [Fact]
        public void Jeder_Startwert_liegt_innerhalb_seiner_eigenen_Grenzen()
        {
            TestSpielwelt.Starte();

            for (int gesetzId = 0; gesetzId < SW.Statisch.GetMaxGesetze(); gesetzId++)
            {
                int startwert = SW.Dynamisch.GetGesetzX(gesetzId);
                int unten = SW.Statisch.GetGesetzXDefUntergrenze(gesetzId);
                int oben = SW.Statisch.GetGesetzXDefObergrenze(gesetzId);

                Assert.True(startwert >= unten && startwert <= oben,
                            "Gesetz " + gesetzId + ": Startwert " + startwert +
                            " liegt nicht in [" + unten + ", " + oben + "].");
            }
        }

        /// <summary>
        /// So aeussert sich der Fehler im Spiel: Ein KI-Minister wuerfelt je Gesetz -1, 0 oder +1
        /// (<c>KiGesetzgebungManager</c>). Faellt die 0, soll sich nichts aendern - bei einem
        /// Startwert ausserhalb der Grenzen sprang das Gesetz trotzdem dauerhaft um.
        /// </summary>
        [Fact]
        public void Eine_Aenderung_um_null_verschiebt_kein_Gesetz()
        {
            TestSpielwelt.Starte();

            for (int gesetzId = 0; gesetzId < SW.Statisch.GetMaxGesetze(); gesetzId++)
            {
                int vorher = SW.Dynamisch.GetGesetzX(gesetzId);

                SW.Dynamisch.GesetzXschaltenUmY(gesetzId, 0);

                Assert.Equal(vorher, SW.Dynamisch.GetGesetzX(gesetzId));
            }
        }
    }
}
