using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Niederlassung
{
    [Serializable]
    public class SpHatWerkstaetten
    {
        private bool _enabled;
        private int _rohstoffID;
        private int[] _skill;

        public SpHatWerkstaetten()
        {
            _skill = new int[SW.Statisch.GetMaxAnzahlSkills()];

            for (int i = 1; i < SW.Statisch.GetMaxAnzahlSkills(); i++)
                _skill[i] = 0;
        }

        public int GetRohstoffID()
        {
            return _rohstoffID;
        }

        public int GetSKillX(int x)
        {
            return _skill[x];
        }

        public bool GetEnabled()
        {
            return _enabled;
        }

        public void SetRohstoffID(int rohstoffID)
        {
            _rohstoffID = rohstoffID;
        }

        public void SetSkillX(int x, int wert)
        {
            _skill[x] = wert;
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;

            if (!_enabled)
                _skill[1] = 0;
        }
    }
}
