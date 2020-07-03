using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einfache Einheit für Räuberlager (Nahkampf)
    /// </summary>
    [Serializable]
    public class RaubRaeuber : Einheit
    {
        #region Konstruktor
        public RaubRaeuber() : base("Räuber", "Räuber", EnumStuetzpunktArt.Raeuberlager, 4, 4, 100, 3, 18, 18, typeof(ZollMusketier), typeof(ZollKanonier))
        {

        }
        #endregion
    }
}
