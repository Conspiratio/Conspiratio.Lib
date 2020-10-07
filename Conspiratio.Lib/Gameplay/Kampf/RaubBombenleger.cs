using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Räuberlager (Nahkampf)
    /// </summary>
    [Serializable]
    public class RaubBombenleger : Einheit
    {
        #region Konstruktor
        public RaubBombenleger() : base("Bombenleger", "Bombenleger", EnumStuetzpunktArt.Raeuberlager, 8, 2, 100, 2, 10, 10, typeof(ZollKanonier), typeof(ZollMusketier))
        {

        }
        #endregion
    }
}
