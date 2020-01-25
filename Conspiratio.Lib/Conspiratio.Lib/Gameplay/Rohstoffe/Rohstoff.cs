using System;

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
        /// <param name="preisMin">Mindestpreis</param>
        /// <param name="preisStd">Standardpreis</param>
        /// <param name="preisMax">Maximalpreis</param>
        /// <param name="name">Bezeichnung des Rohstoffs</param>
        /// <param name="produktionstext">Text, der in der Werkstatt zur Produktion angezeigt wird (Beispiel: Erntet Korn;auf:Feld.Feldern)</param>
        /// <param name="werkstattVerhaeltnisArbeiter">Faktor Arbeiter für das Verhältnis Arbeiter zu Werkstätten</param>
        /// <param name="wekstattVerhaeltnisWerkstatt">Faktor Werkstätten für das Verhältnis Arbeiter zu Werkstätten</param>
        /// <param name="rohstoffStufe">Rohstoffstufe (1 bis 3) zur Unterteilung auch für die Handelszertifikate und die Lagermenge</param>
        /// <param name="textQualitaetProduktion">Text, der im Buch auftaucht, wenn die Ware produziert wurde, z.B. Eure Getreideernte verlief {0}</param>
        /// <param name="lagermengeProQMeter">OPTIONAL: Gibt an, wie viel auf einem Qudaratmeter Lagerplatz gelagert werden können 
        /// (standardmäßig abhängig von der Stufe: 1 = 10, 2 = 6, 3 = 3)</param>
        public Rohstoff(int preisMin, int preisStd, int preisMax, string name, string produktionstext, int werkstattVerhaeltnisArbeiter,
                        int wekstattVerhaeltnisWerkstatt, int rohstoffStufe, string textQualitaetProduktion, int lagermengeProQMeter = 10)
        {
            _preisMin = preisMin;
            _preisStd = preisStd;
            _preisMax = preisMax;

            _name = name;
            _rohStufe = rohstoffStufe;

            _produktionstext = produktionstext;

            _WSverhaeltnisArbeiter = werkstattVerhaeltnisArbeiter;
            _WSverhaeltnisWS = wekstattVerhaeltnisWerkstatt;

            int wsgp = 40;

            _WSKaufpreis = 2000;
            _WSEinzelpreis = wsgp * _WSverhaeltnisArbeiter * _WSverhaeltnisWS;
            _WSArbeiterpreis = 50;

            _textQualitaetProduktion = textQualitaetProduktion;
            _lagermengeProQMeter = lagermengeProQMeter;

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
