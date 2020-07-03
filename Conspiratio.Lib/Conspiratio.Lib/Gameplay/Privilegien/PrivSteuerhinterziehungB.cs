using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivSteuerhinterziehungB : Privileg
    {
        public PrivSteuerhinterziehungB() : base("Steuerhinterziehung-B", 28)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " seid Ihr in der Lage, einen Teil Eurer Umsätze zu verschleiern und reduziert somit Eure Steuerabgaben.");
        }
    }
}
