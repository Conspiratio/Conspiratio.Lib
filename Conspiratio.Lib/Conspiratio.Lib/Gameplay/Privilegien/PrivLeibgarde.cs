using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivLeibgarde : Privileg
    {
        public PrivLeibgarde() : base("Leibgarde", 19)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " besitzt Ihr Eure eigene Leibgarde. Sabotagen und Spionagen sind kaum möglich.");
        }
    }
}
