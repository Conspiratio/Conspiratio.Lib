using System;

using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einfache Einheit für Zollburgen (Nahkampf)
    /// </summary>
    [Serializable]
    public class ZollSoeldner: Einheit
    {
        #region Konstruktor
        public ZollSoeldner() : base("Söldner", "Söldner", EnumStuetzpunktArt.Zollburg, 5, 4, 100, 3, 20, 20, typeof(RaubSchuetze), typeof(RaubKanonier))
        {

        }
        #endregion
    }
}
