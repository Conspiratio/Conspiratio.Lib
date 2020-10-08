using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivKerkerklatsch : Privileg
    {

        public PrivKerkerklatsch() : base("Kerkerklatsch", 7)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " kommt Ihr über Eure Gefangenen an einige Informationen über Eure städtische Konkurrenz.");
        }
    }
}
