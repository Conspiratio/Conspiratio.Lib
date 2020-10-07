using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Zollburgen (Fernkampf)
    /// </summary>
    [Serializable]
    public class ZollMusketier : Einheit
    {
        #region Konstruktor
        public ZollMusketier() : base("Musketier", "Musketiere", EnumStuetzpunktArt.Zollburg, 7, 3, 100, 2, 15, 15, typeof(RaubBombenleger), typeof(RaubRaeuber))
        {

        }
        #endregion
    }
}
