using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivBauwerkStiften : Privileg
    {
        public PrivBauwerkStiften() : base("Bauwerk stiften", 10)
        {
        }

        public override void PrivExecute()
        {
            SW.UI.BauwerkStiftenDialog.ShowDialog();
        }
    }
}
