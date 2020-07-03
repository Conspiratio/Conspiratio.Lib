using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivKaufmann : Privileg
    {
        public PrivKaufmann() : base("Kaufmann", 12)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als erfahrener Kaufmann beeinflusst Ihr viele Händler aus dem ganzen Königreich. Durch das Weitergeben von falschen Informationen könnt Ihr die gesamte Wirtschaft ins Schwanken bringen.");
            SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(10);
        }
    }
}
