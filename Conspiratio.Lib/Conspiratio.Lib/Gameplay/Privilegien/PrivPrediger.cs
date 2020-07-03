using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivPrediger : Privileg
    {

        public PrivPrediger() : base("Prediger", 32)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als Prediger in der Kirche schwingt Ihr stets große Reden. Diese Kunst nutzt Ihr auch in Euren Produktionsstätten, was Eure Arbeiter härter schuften lässt.");
        }
    }
}
