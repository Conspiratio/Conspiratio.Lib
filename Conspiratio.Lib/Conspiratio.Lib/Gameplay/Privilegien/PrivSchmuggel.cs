using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivSchmuggel : Privileg
    {
        public PrivSchmuggel() : base("Schmuggel", 22)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " könnt Ihr mit geschickten Schmuggelgeschäften Euren Sold aufbessern.");
        }
    }
}
