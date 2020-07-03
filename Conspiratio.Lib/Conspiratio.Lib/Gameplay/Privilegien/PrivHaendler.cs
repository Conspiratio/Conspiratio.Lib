using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivHaendler : Privileg
    {
        public PrivHaendler(): base("Händler", 11)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als Händler seid Ihr stets über die Wirtschaftslage Eures Königreiches informiert. Dies gewährt Euch vollen Einblick in die Verkaufspreise aller Städte.");
            SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(9);
        }
    }
}
