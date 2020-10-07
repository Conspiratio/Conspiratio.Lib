using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivTestament : Privileg
    {
        public PrivTestament() : base("Testament machen", 3)
        {
        }

        public override void PrivExecute()
        {
            SW.UI.TestamentAnzeigenDialog.ShowDialog(false);
        }
    }
}
