using System;

namespace Conspiratio.Lib.Gameplay.Gebiete
{
    [Serializable]
    public abstract class Gebiet
    {
        private string _name;

        public Gebiet(string name)
        {
            _name = name;
        }

        public string GetGebietsName()
        {
            return _name;
        }

        public string GetBeinameFuerAmt()
        {
            return " in " + _name;
        }

        public abstract void SetAmtXtoY(int x, int y);  // Wird in den erbenden Klassen überschrieben
        public abstract int GetAmtX(int x);  // Wird in den erbenden Klassen überschrieben
    }
}
