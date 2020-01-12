namespace Conspiratio.Lib.Gameplay.Justiz
{
    public abstract class Strafen
    {
        protected string Strafname;

        public Strafen(string name)
        {
            Strafname = name;
        }

        public abstract string StrafeExecute(int opferID);

        public string GetName()
        {
            return Strafname;
        }
    }
}