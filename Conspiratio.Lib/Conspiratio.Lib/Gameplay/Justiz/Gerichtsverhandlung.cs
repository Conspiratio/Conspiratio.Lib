using System;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    [Serializable]
    public class Gerichtsverhandlung
    {
        private int[] _richterID;
        private int _gebietsID;
        private int _gebietsStufe;
        private int _angeklagterID;
        private int _klaegerID;

        public Gerichtsverhandlung()
        {
            _richterID = new int[3];
        }

        public void SetToZero()
        {
            _richterID[0] = 0;
            _richterID[1] = 0;
            _richterID[2] = 0;
            _gebietsID = 0;
            _gebietsStufe = 0;
            _angeklagterID = 0;
            _klaegerID = 0;
        }

        public void SetKlaegerID(int id)
        {
            _klaegerID = id;
        }

        public void SetAngeklagterID(int id)
        {
            _angeklagterID = id;
        }

        public void SetGebietsStufe(int id)
        {
            _gebietsStufe = id;
        }

        public void SetGebietsID(int id)
        {
            _gebietsID = id;
        }

        public void SetRichterXID(int x, int id)
        {
            _richterID[x] = id;
        }

        public int GetKlaegerID()
        {
            return _klaegerID;
        }

        public int GetGebietsID()
        {
            return _gebietsID;
        }

        public int GetGebietsStufe()
        {
            return _gebietsStufe;
        }

        public int GetAngeklagterID()
        {
            return _angeklagterID;
        }

        public int GetRichterXID(int x)
        {
            return _richterID[x];
        }

        public bool IsEmpty()
        {
            return _richterID[0] == 0 && _richterID[1] == 0 && _richterID[2] == 0 && _gebietsID == 0 && _gebietsStufe == 0 && _angeklagterID == 0 && _klaegerID == 0;
        }

        public void SetAll(int richterID1, int richterID2, int richterID3, int gebietsID, int gebietsStufe, int angeklagterID, int klaegerID)
        {
            _richterID[0] = richterID1;
            _richterID[1] = richterID2;
            _richterID[2] = richterID3;
            _gebietsID = gebietsID;
            _gebietsStufe = gebietsStufe;
            _angeklagterID = angeklagterID;
            _klaegerID = klaegerID;
        }
    }
}
