using System;

namespace Conspiratio.Lib.Gameplay.Hinterzimmer
{
    [Serializable]
    public class AktiveSabotagen
    {
        private int _kosten;
        private int _dauer;

        public AktiveSabotagen(int kosten, int dauer)
        {
            _kosten = kosten;
            _dauer = dauer;
        }

        public int GetDauer()
        {
            return _dauer;
        }

        public void SetDauer(int dauerInJahren)
        {
            _dauer = dauerInJahren;
        }

        public int GetKosten()
        {
            return _kosten;
        }

        public void SetKosten(int kosten)
        {
            _kosten = kosten;
        }

        public void ReduziereDauerUmEins()
        {
            _dauer--;
        }
    }
}
