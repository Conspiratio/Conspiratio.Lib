using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivFinGesetzeAendern : Privileg
    {
        public PrivFinGesetzeAendern() : base("Finanzgesetze festlegen", 25)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " bestimmt Ihr die Finanzgesetze!");
        }
    }
}