using System;

using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Kampf  // TODO: Muss später noch refactored werden, ist aktuell aber aus Kompatibilitätsgründen für die alten Savegames notwendig (bei Deserialisierung)
{
    /// <summary>
    /// Einheit für Zollburgen (Nahkampf)
    /// </summary>
    [Serializable]
    public class ZollOffizier : Einheit
    {
        #region Konstruktor
        // Problem: Diese Types werden wohl nicht vom SerializationBinder aufgelöst sondern intern, was dann bei der Deserialisierung zu einer TypeLoadException führt, siehe auch: https://stackoverflow.com/a/55379118/13328804
        // Exceptiondetails (System.Exception {System.TypeLoadException}):
        // Message: Der Typ "Conspiratio.Kampf.RaubSchuetze" in der Assembly "Conspiratio, Version=1.4.3.0, Culture=neutral, PublicKeyToken=null" konnte nicht geladen werden.
        // TypeName: Conspiratio.Kampf.RaubSchuetze"
        //
        // Lösung:
        // In der alten Assembly folgendes Attribut für das Forwarding des Types einfügen:
        // [assembly: TypeForwardedTo(typeof(Conspiratio.Kampf.ZollOffizier))]
        public ZollOffizier() : base("Offizier", "Offiziere", EnumStuetzpunktArt.Zollburg, 7, 5, 200, 4, 25, 25, typeof(RaubKanonier), typeof(RaubSchuetze))
        {

        }
        #endregion
    }
}
