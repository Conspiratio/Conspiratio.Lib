using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivZollkartell : Privileg
    {
        public PrivZollkartell() : base("Zollkartell", 23)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " gehört Ihr zu den " + (SW.Statisch.GetMaxLandID() - 1) + " Personen, die die totale Kontrolle über die internen Grenzen von " + SW.Dynamisch.GetReichWithID(1).GetGebietsName() + " besitzen. Als Mitglied dieses Kartells braucht Ihr keine Zölle entrichten.");
        }
    }
}
