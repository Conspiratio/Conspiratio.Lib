using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivUmsatzsteuerFestlegen : Privileg
    {
        public PrivUmsatzsteuerFestlegen() : base("Umsatzsteuer festlegen", 14)
        {
        }

        public override void PrivExecute()
        {
            SW.UI.ProzentwertFestlegenDialog.ShowDialog(ProzentwertArt.UmsatzsteuerStadt);
        }
    }
}
