using System;

namespace Conspiratio.Lib.Gameplay.Gebiete
{
    [Serializable]
    public class Land : Gebiet
    {
        #region Variablen
        private int _zollburgID;
        private int _raeuberlagerID;

        private int _geheimrat1, _geheimrat2, _geheimrat3;
        private int _justizberater;
        private int _finanzberater;
        private int _vogt;

        private int _kellermeister;
        private int _sakristan;
        private int _abt;
        private int _diakon;
        private int _bischof;

        private int _stellvertretenderBefehlshaber, _zoellner1, _zoellner2;
        private int _befehlshaber, _zollmeister;
        private int _hauptmann;

        private int[] _staedte;
        #endregion

        #region Konstruktor
        public Land(string name, int s1, int s2, int s3, int s4, int zollburgID, int raeuberlagerID): base (name)
        {
            _staedte = new int[4];
            _staedte[0] = s1;
            _staedte[1] = s2;
            _staedte[2] = s3;
            _staedte[3] = s4;
            _zollburgID = zollburgID;
            _raeuberlagerID = raeuberlagerID;
        }
        #endregion

        #region Getter und Setter

        public int GetZollburgIndex()
        {
            return _zollburgID - 1;
        }

        public int GetRaeuberlagerIndex()
        {
            return _raeuberlagerID - 1;
        }

        public int GetAnzahlStaedte()
        {
            if (_staedte[3] != 0)
                return 4;
            
            return 3;
        }

        public int GetStadtX(int x)
        {
            return _staedte[x];
        }

        public int GetGeheimrat1()
        {
            return _geheimrat1;
        }

        public int GetGeheimrat2()
        {
            return _geheimrat2;
        }

        public int GetGeheimrat3()
        {
            return _geheimrat3;
        }

        public int GetJustizberater()
        {
            return _justizberater;
        }

        public int GetFinanzberater()
        {
            return _finanzberater;
        }

        public int GetVogt()
        {
            return _vogt;
        }

        public int GetKellermeister()
        {
            return _kellermeister;
        }

        public int GetSakristan()
        {
            return _sakristan;
        }

        public int GetAbt()
        {
            return _abt;
        }

        public int GetDiakon()
        {
            return _diakon;
        }

        public int GetBischof()
        {
            return _bischof;
        }

        public int GetStellvertretenderBefehlshaber()
        {
            return _stellvertretenderBefehlshaber;
        }

        public int GetZoellner1()
        {
            return _zoellner1;
        }
        public int GetZoellner2()
        {
            return _zoellner2;
        }

        public int GetBefehlshaber()
        {
            return _befehlshaber;
        }

        public int GetZollmeister()
        {
            return _zollmeister;
        }

        public int GetHauptmann()
        {
            return _hauptmann;
        }
        #endregion

        #region SetAmtXtoY
        public override void SetAmtXtoY(int x, int y)
        {
            switch (x)
            {
                case 17:
                    _geheimrat1 = y;
                    break;
                case 18:
                    _geheimrat2 = y;
                    break;
                case 19:
                    _geheimrat3= y;
                    break;
                case 20:
                    _justizberater = y;
                    break;
                case 21:
                    _finanzberater = y;
                    break;
                case 22:
                    _vogt = y;
                    break;
                case 23:
                    _kellermeister = y;
                    break;
                case 24:
                    _sakristan = y;
                    break;
                case 25:
                    _diakon = y;
                    break;
                case 26:
                    _abt = y;
                    break;
                case 27:
                    _bischof = y;
                    break;
                case 28:
                    _stellvertretenderBefehlshaber = y;
                    break;
                case 29:
                    _zoellner1 = y;
                    break;
                case 30:
                    _zoellner2 = y;
                    break;
                case 31:
                    _befehlshaber = y;
                    break;
                case 32:
                    _zollmeister = y;
                    break;
                case 33:
                    _hauptmann = y;
                    break;
            }
        }
        #endregion

        #region GetAmtX
        public override int GetAmtX(int x)
        {
            switch (x)
            {
                case 17:
                    return _geheimrat1;
                case 18:
                    return _geheimrat2;                    
                case 19:
                    return _geheimrat3;                    
                case 20:
                    return _justizberater;                    
                case 21:
                    return _finanzberater;                    
                case 22:
                    return _vogt;                    
                case 23:
                    return _kellermeister;                    
                case 24:
                    return _sakristan;                    
                case 25:
                    return _diakon;                    
                case 26:
                    return _abt;                    
                case 27:
                    return _bischof;                    
                case 28:
                    return _stellvertretenderBefehlshaber;                    
                case 29:
                    return _zoellner1;                   
                case 30:
                    return _zoellner2;                   
                case 31:
                    return _befehlshaber;                    
                case 32:
                    return _zollmeister;                  
                case 33:
                    return _hauptmann;                   
            }
            return 0;
        }
        #endregion
    }
}
