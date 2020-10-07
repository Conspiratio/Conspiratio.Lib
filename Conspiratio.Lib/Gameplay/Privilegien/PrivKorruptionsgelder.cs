using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivKorruptionsgelder : Privileg
    {
        public PrivKorruptionsgelder() : base("Korruptionsgelder", 21)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " erhaltet Ihr gelegentlich kleine Spenden dafür, dass Ihr ein Auge zudrückt");
        }
    }
}
