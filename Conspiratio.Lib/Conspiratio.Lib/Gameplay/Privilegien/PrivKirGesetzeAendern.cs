using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivKirGesetzeAendern : Privileg
    {
        public PrivKirGesetzeAendern() : base("Kirchengesetze festlegen", 24)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " bestimmt Ihr die Kirchengesetze!");
        }
    }
}
