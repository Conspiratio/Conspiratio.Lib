using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivZollfrei : Privileg
    {
        public PrivZollfrei() : base("Zollfrei", 31)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Euer Amt erlaubt es Euch, Eure Waren manchmal unentgeltlich über die Grenzen zu befördern.");
        }
    }
}
