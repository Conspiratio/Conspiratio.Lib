namespace Conspiratio.Lib.Gameplay.Aemter
{
    public class Amt
    {
        int AmtsID;
        string Amtsname;
        int BonusAnsehen;
        int BonusBestechlich;
        int BonusAggressiv;
        int BonusEinfluss;
        int BonusHinterhaeltig;
        int Waehler1AmtID, Waehler2AmtID, Waehler3AmtID;
        int Einkommen;
        int AmtsStufe;

        public Amt(int id, string nam, int bbest, int baggro, int beinf, int bhint, int w1, int w2, int w3, int stuf, int bonans, int eink)
        {
            AmtsID = id;
            Amtsname = nam;
            BonusBestechlich = bbest; //Bosheit
            BonusAggressiv = baggro;
            BonusEinfluss = beinf;
            BonusHinterhaeltig = bhint;
            Waehler1AmtID = w1;
            Waehler2AmtID = w2;
            Waehler3AmtID = w3;
            AmtsStufe = stuf;
            BonusAnsehen = bonans;
            Einkommen = eink;
        }

        public int getAmtsID()        { return AmtsID; }
        public int getEinkommen()     { return Einkommen; }
        public int getAmtsStufe()     { return AmtsStufe; }
        public int getBonusBest()     { return BonusBestechlich; }
        public int getBonusAggr()     { return BonusAggressiv; }
        public int getBonusEinf()     { return BonusEinfluss; }
        public int getBonusHint()     { return BonusHinterhaeltig; }
        public int getWaehler1AmtID() { return Waehler1AmtID; }
        public int getWaehler2AmtID() { return Waehler2AmtID; }
        public int getWaehler3AmtID() { return Waehler3AmtID; }
        public int getBonusAnsehen()  { return BonusAnsehen; }

        public string getAmtsname(bool maennlich)
        {
            if (maennlich)
            {
                return Amtsname.Substring(0, Amtsname.IndexOf(";"));
            }
            return Amtsname.Substring(Amtsname.IndexOf(";") + 1);
        }
    }
}
