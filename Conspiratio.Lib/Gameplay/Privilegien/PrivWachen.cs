using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivWachen : Privileg
    {
        public PrivWachen() : base("Wachen", 18)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Euer hohes Amt erlaubt Euch, Wachen vor Euren Besitztümern zu postieren. Spionage- und Sabotageversuche Eurer Konkurrenten sind weniger erfolgreich.");
        }
    }
}
