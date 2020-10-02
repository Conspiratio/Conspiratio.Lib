using System;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public class Kind
    {
        private bool _maennlich;
        private int _alter;
        private string _name;  // Ob ein Kind an einem Slot auch existiert wird überprüft ob der name != "" ist
        public int Geburtsjahr { get; set; }

        public Kind(bool maennlich, string name, int geburtsjahr)
        {
            _alter = 0;
            _maennlich = maennlich;
            _name = name;
            Geburtsjahr = geburtsjahr;
        }

        public string GetKindName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public void SetMaennlich(bool maennlich)
        {
            _maennlich = maennlich;
        }

        public void SetAlter(int alter)
        {
            _alter = alter;
        }

        public void AlterPlusEins()
        {
            _alter++;
        }

        public bool GetMaennlich()
        {
            return _maennlich;
        }

        public int GetAlter()
        {
            return _alter;
        }
    }
}
