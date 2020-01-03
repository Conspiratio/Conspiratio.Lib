using System;
using System.Collections.Generic;
using System.Text;

namespace Conspiratio.Lib.Gameplay.Rohstoffe
{
    [Serializable]
    public class Rohstoff
    {
        private string _name;
        private string _produktionstext;

        private int _preisMin;
        private int _preisStd;
        private int _preisMax;

        private int _WSverhaeltnisArbeiter;
        private int _WSverhaeltnisWS;
        private int _WSKaufpreis;
        private int _WSEinzelpreis;
        private int _WSArbeiterpreis;
        private int _WSProdProWS;

        private int _rohStufe;
        private int _lagermengeProQMeter;

        private string _textQualitaetProduktion;

        /// <summary>
        /// Dient zur Initialisierung aller Werte für einen Rohstoff.
        /// </summary>
        /// <param name="PreisMin">Mindestpreis</param>
        /// <param name="PreisStd">Standardpreis</param>
        /// <param name="PreisMax">Maximalpreis</param>
        /// <param name="Name">Bezeichnung des Rohstoffs</param>
        /// <param name="Produktionstext">Text, der in der Werkstatt zur Produktion angezeigt wird (Beispiel: Erntet Korn;auf:Feld.Feldern)</param>
        /// <param name="WSverhaeltnisArbeiter">Faktor Arbeiter für das Verhältnis Arbeiter zu Werkstätten</param>
        /// <param name="WSverhaeltnisWS">Faktor Werkstätten für das Verhältnis Arbeiter zu Werkstätten</param>
        /// <param name="RohStufe">Rohstoffstufe (1 bis 3) zur Unterteilung auch für die Handelszertifikate und die Lagermenge</param>
        /// <param name="TextQualitaetProduktion">Text, der im Buch auftaucht, wenn die Ware produziert wurde, z.B. Eure Getreideernte verlief {0}</param>
        /// <param name="LagermengeProQMeter">OPTIONAL: Gibt an, wie viel auf einem Qudaratmeter Lagerplatz gelagert werden können 
        /// (standardmäßig abhängig von der Stufe: 1 = 10, 2 = 6, 3 = 3)</param>
        public Rohstoff(int PreisMin, int PreisStd, int PreisMax, string Name, string Produktionstext, int WSverhaeltnisArbeiter,
                        int WSverhaeltnisWS, int RohStufe, string TextQualitaetProduktion, int LagermengeProQMeter = 10)
        {
            _preisMin = PreisMin;
            _preisStd = PreisStd;
            _preisMax = PreisMax;

            _name = Name;
            _rohStufe = RohStufe;

            _produktionstext = Produktionstext;

            _WSverhaeltnisArbeiter = WSverhaeltnisArbeiter;
            _WSverhaeltnisWS = WSverhaeltnisWS;

            int wsgp = 40;

            _WSKaufpreis = 2000;
            _WSEinzelpreis = wsgp * _WSverhaeltnisArbeiter * _WSverhaeltnisWS;
            _WSArbeiterpreis = 50;

            _textQualitaetProduktion = TextQualitaetProduktion;
            _lagermengeProQMeter = LagermengeProQMeter;

            if (_rohStufe >= 3)
            {
                _WSKaufpreis *= 20;
                _WSEinzelpreis *= 3;
                _WSArbeiterpreis *= 2;
                _lagermengeProQMeter = 3;
            }
            else if (_rohStufe >= 2)
            {
                _WSKaufpreis *= 5;
                _WSEinzelpreis *= 2;
                _WSArbeiterpreis = (_WSArbeiterpreis * 2) / 3;
                _lagermengeProQMeter = 6;
            }

            _WSProdProWS = Convert.ToInt32(25 * _WSverhaeltnisArbeiter);
        }

        public int ErmittleBenoetigtenLagerplatz(int menge)
        {
            if ((menge > 0) && (_lagermengeProQMeter > 0))
                return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(menge) / Convert.ToDouble(_lagermengeProQMeter)));
            else
                return 0;
        }

        public int GetWSProdProWS()
        {
            return _WSProdProWS;
        }

        public int GetWSKaufpreis()
        {
            return _WSKaufpreis;
        }

        public int GetWSEinzelpreis()
        {
            return _WSEinzelpreis;
        }

        public int GetWSArbeiterpreis()
        {
            return _WSArbeiterpreis;
        }

        public int GetPreisStd()
        {
            return _preisStd;
        }

        public int GetPreisMin()
        {
            return _preisMin;
        }

        public int GetPreisMax()
        {
            return _preisMax;
        }

        public string GetRohName()
        {
            return _name;
        }

        public string GetProdText()
        {
            return _produktionstext;
        }

        public int GetArbeiter()
        {
            return _WSverhaeltnisArbeiter;
        }

        public int GetWerkstaetten()
        {
            return _WSverhaeltnisWS;
        }

        public int GetLagermengeProQMeter()
        {
            return _lagermengeProQMeter;
        }

        public string GetTextQualitaetProduktion()
        {
            return _textQualitaetProduktion;
        }
    }
}
