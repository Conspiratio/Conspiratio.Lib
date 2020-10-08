using System;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Räuberlager (Fernkampf)
    /// </summary>
    [Serializable]
    public class RaubKanonier : Einheit
    {
        #region Konstruktor
        public RaubKanonier() : base("Kanonier", "Kanoniere", EnumStuetzpunktArt.Raeuberlager, 11, 1, 150, 1, 6, 6, typeof(ZollSoeldner), typeof(ZollOffizier))
        {

        }
        #endregion
    }
}
