using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Zollburgen (Fernkampf)
    /// </summary>
    [Serializable]
    public class ZollKanonier : Einheit
    {
        #region Konstruktor
        public ZollKanonier() : base("Kanonier", "Kanoniere", EnumStuetzpunktArt.Zollburg, 12, 1, 150, 1, 8, 8, typeof(RaubRaeuber), typeof(RaubBombenleger))
        {

        }
        #endregion
    }
}
