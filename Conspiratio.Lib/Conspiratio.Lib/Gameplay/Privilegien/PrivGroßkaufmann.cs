using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivGroßkaufmann : Privileg
    {
        public PrivGroßkaufmann() : base("Großkaufmann", 13)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als Großkaufmann habt Ihr selbst auf einige Großhändler Einfluss. Wissentlich könnt Ihr diese Beziehung nutzen, um Rohstoffpreise im gesamten Reich zu kontrollieren.");
            SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(11);
        }
    }
}
