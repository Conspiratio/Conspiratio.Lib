using System;

using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Zusätzlicher Wachposten, den eine gut gesicherte Karawane einer bereits verteidigenden Zollburg-Patrouille
    /// zur Seite stellt. Wird nie regulär rekrutiert, sondern ausschließlich von <see cref="Kampfberechnung"/>
    /// bei einem Karawanenüberfall abhängig von der Sicherheit der überfallenen Karawane vergeben. Absichtlich
    /// kein bestehender Zollburg-Typ (z. B. ZollSoeldner): deren Verluste werden nach dem Kampf typbasiert von der
    /// echten Zollburg-Garnison abgezogen, was hier fälschlich Truppen abziehen würde, die nie rekrutiert wurden.
    /// </summary>
    [Serializable]
    public class KarawanenWache : Einheit
    {
        #region Konstruktor
        public KarawanenWache() : base("Wache", "Wachen", EnumStuetzpunktArt.Zollburg, 4, 4, 0, 3, 18, 18)
        {

        }
        #endregion
    }
}
