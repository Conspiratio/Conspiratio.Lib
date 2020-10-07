using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivConfessio : Privileg
    {
        public PrivConfessio() : base("Confessio", 8)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " wisst Ihr über die Sünden Eurer Mitmenschen Bescheid. Demütig werden sie von einigen kriminellen Handlungen gegen Euch ablassen.");
        }
    }
}
