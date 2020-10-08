using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivSteuerhinterziehungC : Privileg
    {
        public PrivSteuerhinterziehungC() : base("Steuerhinterziehung-C", 29)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " seid Ihr in der Lage, einen großen Teil Eurer Umsätze zu verschleiern und reduziert somit Eure Steuerabgaben.");
        }
    }
}
