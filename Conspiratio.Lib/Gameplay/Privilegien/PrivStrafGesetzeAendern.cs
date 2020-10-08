using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivStrafGesetzeAendern : Privileg
    {
        public PrivStrafGesetzeAendern() : base("Strafgesetze festlegen", 26)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " bestimmt Ihr die Justizgesetze!");
        }
    }
}
