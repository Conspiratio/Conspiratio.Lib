using System;

using Conspiratio.Lib.Gameplay.Schreibstube;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public class KISpieler : Spieler
    {
        #region Variablen
        // Amt: erhöht bzw verringert die Basisdaten zusätzlich
        // alle Ämter werden in Klasse Ämter gespeichert

        // Basisdaten von 1 bis 100 in %
        // Beeinflussen die randoms welche das Spielverhalten
        // von der KI bestimmen
        private int _boese;
        private int _verliebt;
        private bool _nimmtAnWahlTeil;
        private bool _stirbt;

        private int[] _beziehungZuKIMitID;
        #endregion

        #region Konstruktor
        public KISpieler(int taler, string name, bool maennlich, int boese, int verheiratetMit, int verbleibendeJahre): base(taler, name, maennlich, verheiratetMit, verbleibendeJahre)
        {
            this.Taler = taler;
            this.Name = name;
            this.Maennlich = maennlich;
            _boese = boese;
            this.VerheiratetMit = verheiratetMit;

            _beziehungZuKIMitID = new int[SW.Statisch.GetMaxKIID()];
           
            Amtsinformationen = new AmtsInfo(0, 0);
        }
        #endregion

        #region Getter und Setter
        
        public void CreateRndBeziehungen(int own_id)
        {
            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                int rand_wert = SW.Statisch.Rnd.Next(20,81);

                _beziehungZuKIMitID[i] = rand_wert;
            }
        }

        public bool GetStirbt()
        {
            return _stirbt;
        }

        public void SetStirbt(bool value)
        {
            _stirbt = value;
        }

        public void SetVerliebt(int ver)
        {
            _verliebt = ver;
        }

        public int GetBosheit()
        {
            return _boese;
        }

        public void SetBosheit(int best)
        {
            _boese = best;
        }

        public void ErhoeheBeziehungZuX(int x, int wert)
        {
            _beziehungZuKIMitID[x] += wert;

            if (_beziehungZuKIMitID[x] > 100)
                _beziehungZuKIMitID[x] = 100;
            
            if (_beziehungZuKIMitID[x] < 0)
                _beziehungZuKIMitID[x] = 0;
        }

        public void SetBeziehungZuX(int x, int wert)
        {
            _beziehungZuKIMitID[x] = wert;
        }

        public int GetBeziehungZuKIX(int x)
        {
            return _beziehungZuKIMitID[x];
        }

        public int[] GetBeziehungZuAllen()
        {
            return _beziehungZuKIMitID;
        }

        public int GetVerliebt()
        {
            return _verliebt;
        }

        public void ErhoeheVerliebt(int i)
        {
            _verliebt += i;

            if (_verliebt < 0)
                _verliebt = 0;
            
            if (_verliebt > 100)
                _verliebt = 100;
        }

        public bool GetNimmtAnWahlTeil()
        {
            return _nimmtAnWahlTeil;
        }

        public void SetNimmtAnWahlTeil(bool trueOrFalse)
        {
            _nimmtAnWahlTeil = trueOrFalse;
        }

        public override int GetGesamtVermoegen(int spielerID)
        {
            int Gesamtvermoegen = Taler;

            // Stützpunkte
            for (int i = 0; i < SW.Dynamisch.GetStuetzpunkte().Length; i++)
            {
                if (SW.Dynamisch.GetStuetzpunkte()[i].Besitzer == spielerID)
                    Gesamtvermoegen += SW.Dynamisch.GetStuetzpunkte()[i].BerechneWert();
            }

            return Gesamtvermoegen;
        }
        #endregion
    }
}
