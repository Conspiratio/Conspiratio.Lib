namespace Conspiratio.Lib.Gameplay.Aemter
{
    public class Amt
    {
        private int _amtsID;
        private string _amtsname;
        private int _bonusAnsehen;
        private int _bonusBestechlich;
        private int _bonusAggressiv;
        private int _bonusEinfluss;
        private int _bonusHinterhaeltig;
        private int _waehler1AmtID;
        private int _waehler2AmtID;
        private int _waehler3AmtID;
        private int _einkommen;
        private int _amtsStufe;

        public Amt(int id, string nam, int bbest, int baggro, int beinf, int bhint, int w1, int w2, int w3, int stuf, int bonans, int eink)
        {
            _amtsID = id;
            _amtsname = nam;
            _bonusBestechlich = bbest;  // Bosheit [Anm. SirTobyB: Keine Ahnung, was mit diesem Kommentar gemeint war]
            _bonusAggressiv = baggro;
            _bonusEinfluss = beinf;
            _bonusHinterhaeltig = bhint;
            _waehler1AmtID = w1;
            _waehler2AmtID = w2;
            _waehler3AmtID = w3;
            _amtsStufe = stuf;
            _bonusAnsehen = bonans;
            _einkommen = eink;
        }

        public int GetAmtsID()        { return _amtsID; }
        public int GetEinkommen()     { return _einkommen; }
        public int GetAmtsStufe()     { return _amtsStufe; }
        public int GetBonusBest()     { return _bonusBestechlich; }
        public int GetBonusAggr()     { return _bonusAggressiv; }
        public int GetBonusEinf()     { return _bonusEinfluss; }
        public int GetBonusHint()     { return _bonusHinterhaeltig; }
        public int GetWaehler1AmtID() { return _waehler1AmtID; }
        public int GetWaehler2AmtID() { return _waehler2AmtID; }
        public int GetWaehler3AmtID() { return _waehler3AmtID; }
        public int GetBonusAnsehen()  { return _bonusAnsehen; }

        public string GetAmtsname(bool maennlich)
        {
            if (maennlich)
                return _amtsname.Substring(0, _amtsname.IndexOf(";"));
            
            return _amtsname.Substring(_amtsname.IndexOf(";") + 1);
        }
    }
}
