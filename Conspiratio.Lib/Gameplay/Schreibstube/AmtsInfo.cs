using System;

namespace Conspiratio.Lib.Gameplay.Schreibstube
{
    [Serializable]
    public class AmtsInfo
    {
        private int _amtsID;  // Gibt an um welches Amt es sich handelt
        private int _gebietsID;  // Gibt an um welche(s) Stadt, Land oder Reich es sich handelt

        public AmtsInfo(int amtsID, int GebID)
        {
            _amtsID = amtsID;
            _gebietsID = GebID;
        }

        public int GetAmtsID()
        {
            return _amtsID;
        }

        public int GetGebietsID()
        {
            return _gebietsID;
        }

        public void SetAmtsID(int amtsID)
        {
            _amtsID = amtsID;
        }

        public void SetGebietsID(int gebietsID)
        {
            _gebietsID = gebietsID;
        }

        public void SetAll(int amtsID, int gebietsID)
        {
            _amtsID = amtsID;
            _gebietsID = gebietsID;
        }
    }
}
