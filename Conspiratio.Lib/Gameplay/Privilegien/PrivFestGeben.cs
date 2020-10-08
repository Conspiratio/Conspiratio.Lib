using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivFestGeben : Privileg
    {
        public PrivFestGeben(): base("Ein Fest geben", 33)
        {
        }

        public override void PrivExecute()
        {
            SW.UI.FestGebenDialog.ShowDialog();
        }
    }
}
