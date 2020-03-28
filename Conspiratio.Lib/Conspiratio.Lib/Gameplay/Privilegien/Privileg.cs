namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public abstract class Privileg : IPrivileg
    {
        public string Name { get; }
        public int ID { get; }

        protected Privileg(string name, int id)
        {
            Name = name;
            ID = id;
        }

        public abstract void PrivExecute();
    }
}
