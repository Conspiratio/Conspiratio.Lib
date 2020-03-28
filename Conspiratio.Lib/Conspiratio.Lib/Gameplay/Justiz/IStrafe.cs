namespace Conspiratio.Lib.Gameplay.Justiz
{
    public interface IStrafe
    {
        string Name { get; }

        string StrafeExecute(int opferID);
    }
}