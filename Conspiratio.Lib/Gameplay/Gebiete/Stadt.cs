using System;
using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Gebiete
{
    [Serializable]
    public class Stadt : Gebiet
    {
        #region Variablen

        private int[] _produktionRohstoffID = new int[SW.Statisch.GetMaxWerkstaettenProStadt() + 1];
        private double[] _produktionRohstoffEffizienz = new double[SW.Statisch.GetMaxRohID()];
        private int _reichtum;
        private int _kriminalitaet;
        private int _einwohner;
        private int[] _katastrophe = new int[5];
        private double _umsatzsteuer;
        private int _landID; // Das Land zu dem die Stadt gehört

        // Ämter speichern die ID des Amtsträgers
        // Politische
        private int _ratsherr1, _ratsherr2, _ratsherr3;
        private int _baumeister;
        private int _richter;
        private int _kaemmerer;
        private int _buergermeister;

        // Kirchliche
        private int _priester1, _priester2;
        private int _domherr;

        // Militärische
        private int _stadtwache, _folterknecht, _henker;
        private int _wachkommandant, _kerkermeister;
        private int _stadtkommandant;

        private int[] _rohstoffVorrat;
        private int[] _rohstoffPreis;

        #endregion

        #region Konstruktor
        public Stadt(string name, int pr1, int pr2, int pr3, int pr4, int pr5, int pr6, double h1, double h2, double h3, double h4, double h5, double h6, int c, int r, int k1, int k2, int k3, int k4, int k5, int einwoh, int lid, int rp1, int rp2, int rp3, int rp4, int rp5, int rp6, int rp7, int rp8, int rp9, int rp10, int rp11, int rp12, int rp13, int rp14, int rp15, int rp16, int rp17, int rp18, int rp19, int rp20, int rp21)
            : base(name)
        {
            _produktionRohstoffID[1] = pr1;
            _produktionRohstoffID[2] = pr2;
            _produktionRohstoffID[3] = pr3;
            _produktionRohstoffID[4] = pr4;
            _produktionRohstoffID[5] = pr5;
            _produktionRohstoffID[6] = pr6;

            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                _produktionRohstoffEffizienz[i] = 0.5;
            }

            _produktionRohstoffEffizienz[_produktionRohstoffID[1]] = h1;
            _produktionRohstoffEffizienz[_produktionRohstoffID[2]] = h2;
            _produktionRohstoffEffizienz[_produktionRohstoffID[3]] = h3;
            _produktionRohstoffEffizienz[_produktionRohstoffID[4]] = h4;
            _produktionRohstoffEffizienz[_produktionRohstoffID[5]] = h5;
            _produktionRohstoffEffizienz[_produktionRohstoffID[6]] = h6;

            _reichtum = r;
            _kriminalitaet = c;
            _landID = lid;
            _umsatzsteuer = SW.Statisch.GetStandardUmsatzSteuer();

            _katastrophe[0] = k1; //1 : Sturm
            _katastrophe[1] = k2; //2 : Flut
            _katastrophe[2] = k3; //3 : Brand
            _katastrophe[3] = k4; //4 : Erdbeeben
            _katastrophe[4] = k5; //5 : Pest

            _einwohner = einwoh;

            _rohstoffVorrat = new int[SW.Statisch.GetMaxRohID()];

            _rohstoffPreis = new int[SW.Statisch.GetMaxRohID()];
            _rohstoffPreis[1] = rp1;
            _rohstoffPreis[2] = rp2;
            _rohstoffPreis[3] = rp3;
            _rohstoffPreis[4] = rp4;
            _rohstoffPreis[5] = rp5;
            _rohstoffPreis[6] = rp6;
            _rohstoffPreis[7] = rp7;
            _rohstoffPreis[8] = rp8;
            _rohstoffPreis[9] = rp9;
            _rohstoffPreis[10] = rp10;
            _rohstoffPreis[11] = rp11;
            _rohstoffPreis[12] = rp12;
            _rohstoffPreis[13] = rp13;
            _rohstoffPreis[14] = rp14;
            _rohstoffPreis[15] = rp15;
            _rohstoffPreis[16] = rp16;
            _rohstoffPreis[17] = rp17;
            _rohstoffPreis[18] = rp18;
            _rohstoffPreis[19] = rp19;
            _rohstoffPreis[20] = rp20;
            _rohstoffPreis[21] = rp21;
        }
        #endregion

        #region Getter und Setter

        public int GetRohstoffPreisVonIDX(int X)
        {
            int temp = _rohstoffPreis[X];
            temp -= Convert.ToInt32(_rohstoffVorrat[X] / 1000);

            if (temp < SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin())
                temp = SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin();

            return temp;
        }

        public void SetRohstoffPreisVonIDXToY(int X, int Y)
        {
            _rohstoffPreis[X] = Y;

            if (_rohstoffPreis[X] < SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin())
                _rohstoffPreis[X] = SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin();

            if (_rohstoffPreis[X] > SW.Dynamisch.GetRohstoffwithID(X).GetPreisMax())
                _rohstoffPreis[X] = SW.Dynamisch.GetRohstoffwithID(X).GetPreisMax();
        }

        public void ErhoeheRohstoffPreisVonIDXByY(int X, int Y)
        {
            _rohstoffPreis[X] += Y;

            if (_rohstoffPreis[X] < SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin())
                _rohstoffPreis[X] = SW.Dynamisch.GetRohstoffwithID(X).GetPreisMin();

            if (_rohstoffPreis[X] > SW.Dynamisch.GetRohstoffwithID(X).GetPreisMax())
                _rohstoffPreis[X] = SW.Dynamisch.GetRohstoffwithID(X).GetPreisMax();
        }

        public int GetRohstoffIDXVorrat(int X)
        {
            return _rohstoffVorrat[X];
        }

        public void SetRohstoffVorratWithIDXToY(int X, int Y)
        {
            _rohstoffVorrat[X] = Y;
        }

        public void ErhoeheRohstoffVorratWithIDXByY(int X, int Y)
        {
            _rohstoffVorrat[X] += Y;

            if (_rohstoffVorrat[X] < 0)
                _rohstoffVorrat[X] = 0;
        }

        public int GetLandID()
        {
            return _landID;
        }

        public int GetReichtum()
        {
            return _reichtum;
        }

        public void SetReichtumToX(int X)
        {
            _reichtum = X;
        }

        public int GetKriminalitaet()
        {
            return _kriminalitaet;
        }

        public void SetKriminalitaetAufX(int X)
        {
            _kriminalitaet = X;
        }

        public int GetEinwohner()
        {
            return _einwohner;
        }

        public void SetEinwohnerAufX(int X)
        {
            _einwohner = X;
        }

        public int[] GetKatastrophen()
        {
            return _katastrophe;
        }

        public double GetEffizienzVonRohstoffMitIDX(int x)
        {
            return _produktionRohstoffEffizienz[x];
        }

        public int[] GetHauptproduktion(int anzahlRohstoffe = 3)
        {
            // Die effizientesten (standardmäßig 3) Rohstoffe der Stadt ermitteln
            int[] HPRohstoffe = new int[anzahlRohstoffe];
            double[] HPEffizienz = new double[anzahlRohstoffe];

            for (int Nummer = 0; Nummer < anzahlRohstoffe; Nummer++)
            {
                for (int j = 1; j < SW.Statisch.GetMaxRohID(); j++)
                {
                    if ((_produktionRohstoffEffizienz[j] > HPEffizienz[Nummer]) && (Array.IndexOf(HPRohstoffe, j) < 0))
                    {
                        HPEffizienz[Nummer] = _produktionRohstoffEffizienz[j];
                        HPRohstoffe[Nummer] = j;
                    }
                }
            }

            return HPRohstoffe;
        }

        public int[] GetBedarf(int stadtID)
        {
            int[] Bed = new int[3];
            int[] alleRohstoffeMitPreis = new int[SW.Statisch.GetMaxRohID()];
            int temppreis = 0;
            int stdpreis = 0;

            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                temppreis = SW.Dynamisch.GetStadtwithID(stadtID).GetRohstoffPreisVonIDX(i);
                stdpreis = SW.Dynamisch.GetRohstoffwithID(i).GetPreisStd();
                alleRohstoffeMitPreis[i] =  temppreis - stdpreis;
            }

            int max = 0;
            for (int i = 1; i < SW.Statisch.GetMaxRohStufe1ID(); i++)
            {
                if (alleRohstoffeMitPreis[i] > max)
                {
                    max = alleRohstoffeMitPreis[i];
                    Bed[0] = i;
                }
            }

            max = 0;
            for (int i = SW.Statisch.GetMaxRohStufe1ID(); i < SW.Statisch.GetMaxRohStufe2ID(); i++)
            {
                if (i != Bed[0])
                {
                    if (alleRohstoffeMitPreis[i] > max)
                    {
                        max = alleRohstoffeMitPreis[i];
                        Bed[1] = i;
                    }
                }
            }

            max = 0;
            for (int i = SW.Statisch.GetMaxRohStufe2ID(); i < SW.Statisch.GetMaxRohID(); i++)
            {
                if ((i != Bed[0]) && (i != Bed[1]))
                {
                    if (alleRohstoffeMitPreis[i] > max)
                    {
                        max = alleRohstoffeMitPreis[i];
                        Bed[2] = i;
                    }
                }
            }

            return Bed;
        }

        public int[] GetRohstoffe()
        {
            return _produktionRohstoffID;
        }

        public int GetSingleRohstoff(int i)
        {
            return _produktionRohstoffID[i];
        }

        public int GetRatsherr1()
        {
            return _ratsherr1;
        }
        public int GetRatsherr2()
        {
            return _ratsherr2;
        }
        public int GetRatsherr3()
        {
            return _ratsherr3;
        }
        public int GetBaumeister()
        {
            return _baumeister;
        }
        public int GetRichter()
        {
            return _richter;
        }
        public int GetKaemmerer()
        {
            return _kaemmerer;
        }
        public int GetBuergermeister()
        {
            return _buergermeister;
        }

        public int GetPriester1()
        {
            return _priester1;
        }
        public int GetPriester2()
        {
            return _priester2;
        }
        public int GetDomherr()
        {
            return _domherr;
        }

        public int GetStadtwache()
        {
            return _stadtwache;
        }
        public int GetFolterknecht()
        {
            return _folterknecht;
        }
        public int GetHenker()
        {
            return _henker;
        }
        public int GetWachkommandant()
        {
            return _wachkommandant;
        }
        public int GetKerkermeister()
        {
            return _kerkermeister;
        }
        public int GetStadtkommandant()
        {
            return _stadtkommandant;
        }

        public void SetRatsherr1(int PersonenID)
        {
            _ratsherr1 = PersonenID;
        }
        public void SetRatsherr2(int PersonenID)
        {
            _ratsherr2 = PersonenID;
        }
        public void SetRatsherr3(int PersonenID)
        {
            _ratsherr3 = PersonenID;
        }
        public void SetBaumeister(int PersonenID)
        {
            _baumeister = PersonenID;
        }
        public void SetRichter(int PersonenID)
        {
            _richter = PersonenID;
        }
        public void SetKaemmerer(int PersonenID)
        {
            _kaemmerer = PersonenID;
        }
        public void SetBuergermeister(int PersonenID)
        {
            _buergermeister = PersonenID;
        }

        public void SetPriester1(int PersonenID)
        {
            _priester1 = PersonenID;
        }
        public void SetPriester2(int PersonenID)
        {
            _priester2 = PersonenID;
        }
        public void SetDomherr(int PersonenID)
        {
            _domherr = PersonenID;
        }

        public void SetStadtwache(int PersonenID)
        {
            _stadtwache = PersonenID;
        }
        public void SetFolterknecht(int PersonenID)
        {
            _folterknecht = PersonenID;
        }
        public void SetHenker(int PersonenID)
        {
            _henker = PersonenID;
        }
        public void SetWachkommandant(int PersonenID)
        {
            _wachkommandant = PersonenID;
        }
        public void SetKerkermeister(int PersonenID)
        {
            _kerkermeister = PersonenID;
        }
        public void SetStadtkommandant(int PersonenID)
        {
            _stadtkommandant = PersonenID;
        }

        public double GetUmsatzsteuer()
        {
            return _umsatzsteuer;
        }

        public void SetUmsatzsteuerAufX(double X)
        {
            if (X < SW.Statisch.GetMinUmsatzsteuer())
                _umsatzsteuer = SW.Statisch.GetMinUmsatzsteuer();
            else if (X > SW.Statisch.GetMaxUmsatzsteuer())
                _umsatzsteuer = SW.Statisch.GetMaxUmsatzsteuer();
            else
                _umsatzsteuer = X;
        }

        public void ErhoeheUmsatzsteuer(double i)
        {
            _umsatzsteuer += i;

            if (_umsatzsteuer < SW.Statisch.GetMinUmsatzsteuer())
                _umsatzsteuer = SW.Statisch.GetMinUmsatzsteuer();

            if (_umsatzsteuer > SW.Statisch.GetMaxUmsatzsteuer())
                _umsatzsteuer = SW.Statisch.GetMaxUmsatzsteuer();
        }
        #endregion

        #region SetAmtXtoY
        public override void SetAmtXtoY(int x, int y)
        {
            switch (x)
            {
                case 1:
                    _ratsherr1 = y;
                    break;
                case 2:
                    _ratsherr2 = y;
                    break;
                case 3:
                    _ratsherr3 = y;
                    break;
                case 4:
                    _baumeister = y;
                    break;
                case 5:
                    _richter = y;
                    break;
                case 6:
                    _kaemmerer = y;
                    break;
                case 7:
                    _buergermeister = y;
                    break;
                case 8:
                    _priester1 = y;
                    break;
                case 9:
                    _priester2 = y;
                    break;
                case 10:
                    _domherr = y;
                    break;
                case 11:
                    _stadtwache = y;
                    break;
                case 12:
                    _folterknecht = y;
                    break;
                case 13:
                    _henker = y;
                    break;
                case 14:
                    _wachkommandant = y;
                    break;
                case 15:
                    _kerkermeister = y;
                    break;
                case 16:
                    _stadtkommandant = y;
                    break;
            }
        }
        #endregion

        #region GetAmtX
        public override int GetAmtX(int x)
        {
            switch (x)
            {
                case 1:
                    return _ratsherr1;
                case 2:
                    return _ratsherr2;;
                case 3:
                    return _ratsherr3;
                case 4:
                    return _baumeister;
                case 5:
                    return _richter;
                case 6:
                    return _kaemmerer;
                case 7:
                    return _buergermeister;
                case 8:
                    return _priester1;
                case 9:
                    return _priester2;
                case 10:
                    return _domherr;
                case 11:
                    return _stadtwache;
                case 12:
                    return _folterknecht;
                case 13:
                    return _henker;
                case 14:
                    return _wachkommandant;
                case 15:
                    return _kerkermeister;
                case 16:
                    return _stadtkommandant;
            }
            return 0;
        }
        #endregion
    }
}
