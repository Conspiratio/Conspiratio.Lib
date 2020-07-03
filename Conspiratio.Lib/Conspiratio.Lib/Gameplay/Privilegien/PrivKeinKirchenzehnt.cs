using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivKeinKirchenzehnt : Privileg
    {
        public PrivKeinKirchenzehnt() : base("Kein Kirchenzehnt", 16)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Da Ihr ein hohes geistliches Amt bekleidet, müsst Ihr keinen Kirchenzehnt bezahlen");
        }
    }
}
