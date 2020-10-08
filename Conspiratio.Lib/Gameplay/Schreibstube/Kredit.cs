using System;

namespace Conspiratio.Lib.Gameplay.Schreibstube
{
    [Serializable]
    public class Kredit
    {
        // Maximal 4 Kredite zur Zeit
        private int _dauer;
        private int _taler;
        private int _zinsen;
        private int _KIID;

        public Kredit(int taler, int dauer, int zinsen)
        {
            _taler = taler;
            _dauer = dauer;
            _zinsen = zinsen;
        }

        public void ReduziereDauer()
        {
            _dauer--;

            if (_dauer == 0)
                DeleteKredit();
        }

        public void DeleteKredit()
        {
            _dauer = 0;
            _taler = 0;
            _zinsen = 0;
            _KIID = 0;
        }

        public int GetDauer()
        {
            return _dauer;
        }

        public int GetTaler()
        {
            return _taler;
        }

        public int GetZinsen()
        {
            return _zinsen;
        }

        public int GetKIID()
        {
            return _KIID;
        }

        public void SetDauer(int dauer)
        {
            _dauer = dauer;
        }

        public void SetTaler(int taler)
        {
            _taler = taler;
        }

        public void SetZinsen(int zinsen)
        {
            _zinsen = zinsen;
        }

        public void SetKIID(int KIID)
        {
            _KIID = KIID;
        }
    }
}
