using System;

namespace Conspiratio.Lib.Gameplay.Gebiete
{
    [Serializable]
    public class Reich : Gebiet
    {
        #region Variablen

        private int _hofrat1, _hofrat2, _hofrat3;
        private int _justizminister, _finanzminister;
        private int _regent;

        private int _erzdiakon, _inquisitor;
        private int _erzbischof;

        private int _offizier1, _offizier2, _offizier3;
        private int _marschall1, _marschall2;
        private int _feldmarschall;
        #endregion

        #region Konstruktor
        public Reich(string name): base (name)
        {
        }
        #endregion

        #region Getter

        public int GetHofrat1()
        {
            return _hofrat1;
        }

        public int GetHofrat2()
        {
            return _hofrat2;
        }

        public int GetHofrat3()
        {
            return _hofrat3;
        }

        public int GetJustizminister()
        {
            return _justizminister;
        }

        public int GetFinanzminister()
        {
            return _finanzminister;
        }

        public int GetRegent()
        {
            return _regent;
        }

        public int GetErzdiakon()
        {
            return _erzdiakon;
        }

        public int GetInquisitor()
        {
            return _inquisitor;
        }

        public int GetErzbischof()
        {
            return _erzbischof;
        }

        public int GetOffizier1()
        {
            return _offizier1;
        }

        public int GetOffizier2()
        {
            return _offizier2;
        }

        public int GetOffizier3()
        {
            return _offizier3;
        }

        public int GetMarschall1()
        {
            return _marschall1;
        }

        public int GetMarschall2()
        {
            return _marschall2;
        }

        public int GetFeldmarschall()
        {
            return _feldmarschall;
        }

        #endregion

        #region SetAmtXtoY
        public override void SetAmtXtoY(int x, int y)
        {
            switch (x)
            {
                case 34:
                    _hofrat1 = y;
                    break;
                case 35:
                    _hofrat2 = y;
                    break;
                case 36:
                    _hofrat3 = y;
                    break;
                case 37:
                    _justizminister = y;
                    break;
                case 38:
                    _finanzminister = y;
                    break;
                case 39:
                    _regent = y;
                    break;
                case 40:
                    _inquisitor = y;
                    break;
                case 41:
                    _erzdiakon = y;
                    break;
                case 42:
                    _erzbischof = y;
                    break;
                case 43:
                    _offizier1 = y;
                    break;
                case 44:
                    _offizier2 = y;
                    break;
                case 45:
                    _offizier3 = y;
                    break;
                case 46:
                    _marschall1 = y;
                    break;
                case 47:
                    _marschall2 = y;
                    break;
                case 48:
                    _feldmarschall = y;
                    break;
            }
        }
        #endregion

        #region GetAmtX
        public override int GetAmtX(int x)
        {
            switch (x)
            {
                case 34:
                    return _hofrat1;
                case 35:
                    return _hofrat2;
                case 36:
                    return _hofrat3;
                case 37:
                    return _justizminister;
                case 38:
                    return _finanzminister;
                case 39:
                    return _regent;
                case 40:
                    return _inquisitor;
                case 41:
                    return _erzdiakon;
                case 42:
                    return _erzbischof;
                case 43:
                    return _offizier1;
                case 44:
                    return _offizier2;
                case 45:
                    return _offizier3;
                case 46:
                    return _marschall1;
                case 47:
                    return _marschall2;
                case 48:
                    return _feldmarschall;
            }
            return 0;
        }
        #endregion
    }
}
