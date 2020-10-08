using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public interface IProzentwertFestlegenDialog
    {
        void ShowDialog(ProzentwertArt prozentwertArt, int zielStuetzpunktID = 0);
    }
}
