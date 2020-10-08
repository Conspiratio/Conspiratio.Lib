using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivUntergebene : Privileg
    {
        public PrivUntergebene() : base("Untergebene", 6)
        {
        }

        public override void PrivExecute()
        {
            SW.UI.UntergebeneDialog.ShowDialog();
        }
    }
}
