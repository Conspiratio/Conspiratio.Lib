namespace Conspiratio.Lib.Gameplay.Justiz
{
    public abstract class Strafe : IStrafe
    {
        public string Name { get; }

        protected Strafe(string name)
        {
            Name = name;
        }

        public abstract string StrafeExecute(int opferID, int deliktpunkte);
    }
}