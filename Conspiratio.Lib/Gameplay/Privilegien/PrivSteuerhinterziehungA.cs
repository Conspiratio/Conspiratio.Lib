using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivSteuerhinterziehungA : Privileg
    {
        public PrivSteuerhinterziehungA() : base("Steuerhinterziehung-A", 27)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " seid Ihr in der Lage, einen kleinen Teil Eurer Umsätze zu verschleiern und reduziert somit Eure Steuerabgaben");
        }
    }
}
