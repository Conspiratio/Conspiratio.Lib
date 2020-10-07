using System;

namespace Conspiratio.Lib.Gameplay.Hinterzimmer
{
    [Serializable]
    public class AktiveSpionagen
    {
        private int _kosten;  // Eine Spionage ist aktiv wenn die Kosten > 0 sind
        private int _dauer;
        private int _delikte;
        private int _jahr;  // Wie lange diese Information her ist

        public AktiveSpionagen(int kosten)
        {
            _kosten = kosten;
        }

        public void SetKosten(int kosten)
        {
            _kosten = kosten;
        }

        public int GetKosten()
        {
            return _kosten;
        }

        public void SetDauer(int dauer)
        {
            _dauer = dauer;
        }

        public int GetDauer()
        {
            return _dauer;
        }

        public void DauerPlusEins()
        {
            _dauer++;
        }

        public int GetDelikte()
        {
            return _delikte;
        }

        public void DeliktePlusEins()
        {
            _delikte++;
        }

        public void SetDelikte(int X)
        {
            _delikte = X;
        }

        public int GetJahr()
        {
            return _jahr;
        }

        public void SetJahr(int X)
        {
            _jahr = X;
        }
    }
}
