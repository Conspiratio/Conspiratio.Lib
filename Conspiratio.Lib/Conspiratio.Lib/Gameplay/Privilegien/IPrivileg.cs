namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public interface IPrivileg
    {
        int ID { get; }
        string Name { get; }

        void PrivExecute();
    }
}