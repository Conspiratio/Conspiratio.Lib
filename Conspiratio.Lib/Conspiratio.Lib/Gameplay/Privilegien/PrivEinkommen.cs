using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivEinkommen : Privileg
    {
        public PrivEinkommen() : base("Einkommen", 5)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " verdient Ihr " + SW.Statisch.GetAmtwithID(SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetAmtID()).GetEinkommen().ToStringGeld() + " im Jahr.");
        }
    }
}
