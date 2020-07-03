using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Räuberlager (Fernkampf)
    /// </summary>
    [Serializable]
    public class RaubSchuetze : Einheit
    {
        #region Konstruktor
        public RaubSchuetze() : base("Schütze", "Schützen", EnumStuetzpunktArt.Raeuberlager, 7, 4, 200, 4, 13, 13, typeof(ZollOffizier), typeof(ZollSoeldner))
        {

        }
        #endregion
    }
}
