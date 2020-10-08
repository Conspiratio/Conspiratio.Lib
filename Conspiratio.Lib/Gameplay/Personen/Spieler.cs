using System;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Schreibstube;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public abstract class Spieler
    {
        #region Variablen

        protected string Name;
        protected bool Maennlich;
        protected int Taler;
        protected int Religion;
        protected int Alter;
        protected int VerheiratetMit;
        protected int VerbleibendeJahre;
        protected int Deliktpunkte;
        protected bool WirdBereitsVerklagt;
        protected AmtsInfo Amtsinformationen;

        private int _wahlTeilnahme;
        private int _gesundheit;
        private int _ansehen;
        private int _titel;
        private bool[] _privilegien = new bool[SW.Statisch.GetMaxPriv()];

        #endregion

        #region Konstruktor
        public Spieler(int taler, string name, bool maennlich, int verheiratetMit, int verbleibendeJahre)
        {
            Name = name;
            Taler = taler;
            Maennlich = maennlich;
            Amtsinformationen = new AmtsInfo(0, 0);
            VerheiratetMit = verheiratetMit;
            VerbleibendeJahre = verbleibendeJahre;
            _gesundheit = SW.Statisch.GetMaxGesundheit();
        }
        #endregion

        #region Getter und Setter

        public void SetVerbleibendeJahre(int jahre)
        {
            VerbleibendeJahre = jahre;
        }

        public int GetVerbleibendeJahre()
        {
            return VerbleibendeJahre;
        }

        public void SetAmt(int amtsID, int slrid)
        {
            Amtsinformationen.SetAll(amtsID, slrid);
        }

        public int GetAmtID()
        {
            return Amtsinformationen.GetAmtsID();
        }

        public int GetAmtGebiet()
        {
            return Amtsinformationen.GetGebietsID();
        }

        public void SetPrivilegX(int x, bool trueOrFalse)
        {
            _privilegien[x] = trueOrFalse;
        }

        public bool CheckPrivilegX(int x)
        {
            return _privilegien[x];
        }

        public AmtsInfo GetAmtsInformationen()
        {
            return Amtsinformationen;
        }

        public void ErhoeheTaler(int veraenderungswert)
        {
            Taler += veraenderungswert;
        }

        /// <summary>
        /// Vollständigen Namen inklusive Titel und Amt mit Ortsnamen ermitteln.
        /// </summary>
        /// <returns>Vollständiger Name inklusive Titel und Amt mit Ortsnamen</returns>
        public string GetKompletterName()
        {
            // Titeltext
            string titelText = SW.Statisch.GetTitelX(_titel).GetName(Maennlich);
            string amtText = GetAmtNameUndOrt();

            return $"{titelText} {Name}, {amtText}";
        }

        public string GetAmtNameUndOrt()
        {
            if (Amtsinformationen.GetAmtsID() == 0)
                return SW.Statisch.GetAmtwithID(Amtsinformationen.GetAmtsID()).GetAmtsname(Maennlich);

            string ortsbeiname = SW.Dynamisch.GetStadtwithID(Amtsinformationen.GetGebietsID()).GetBeinameFuerAmt();

            if (SW.Dynamisch.GetStufeVonAmtmitIDx(Amtsinformationen.GetAmtsID()) == 1)
                ortsbeiname = SW.Dynamisch.GetLandWithID(Amtsinformationen.GetGebietsID()).GetBeinameFuerAmt();

            if (SW.Dynamisch.GetStufeVonAmtmitIDx(Amtsinformationen.GetAmtsID()) == 2)
                ortsbeiname = SW.Dynamisch.GetReichWithID(Amtsinformationen.GetGebietsID()).GetBeinameFuerAmt();

            return SW.Statisch.GetAmtwithID(Amtsinformationen.GetAmtsID()).GetAmtsname(Maennlich) + ortsbeiname;
        }

        public string GetCompleteNameOhneTitel()
        {
            string amtText = GetAmtNameUndOrt();
            return $"{Name}, {amtText}";
        }

        public int GetWahlTeilnahme()
        {
            return _wahlTeilnahme;
        }

        public void SetWahlTeilnahme(int x)
        {
            _wahlTeilnahme = x;
        }

        public string GetName()
        {
            return Name;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public bool GetWirdBereitsVerklagt()
        {
            return WirdBereitsVerklagt;
        }

        public void SetWirdBereitsVerklagt(bool verklagen)
        {
            WirdBereitsVerklagt = verklagen;
        }

        public void SetAmtID(int amtID)
        {
            Amtsinformationen.SetAmtsID(amtID);
        }

        public void SetAmtGebiet(int gebiet)
        {
            Amtsinformationen.SetGebietsID(gebiet);
        }

        public bool GetMaennlich()
        {
            return Maennlich;
        }

        public void SetMaennlich(bool maennlich)
        {
            Maennlich = maennlich;
        }

        public void SetTaler(int taler)
        {
            Taler = taler;
        }

        public int GetTaler()
        {
            return Taler;
        }

        public string GetTalerFormatiert()
        {
            return Taler.ToStringGeld();
        }

        public void SetVerheiratet(int spielerID)
        {
            VerheiratetMit = spielerID;
        }

        public int GetVerheiratet()
        {
            return VerheiratetMit;
        }

        public int GetDeliktpunkte()
        {
            return Deliktpunkte;
        }

        public void SetDeliktpunkte(int punkte)
        {
            Deliktpunkte = punkte;
        }

        public int GetAlter()
        {
            return Alter;
        }

        public void SetAlter(int x)
        {
            Alter = x;
        }

        public void AlterPlusEins()
        {
            Alter++;
            VerbleibendeJahre--;
        }

        public void ErhoeheGesundheit(int wert)
        {
            _gesundheit += wert;

            if (_gesundheit > SW.Statisch.GetMaxGesundheit())
                _gesundheit = SW.Statisch.GetMaxGesundheit();
            else if (_gesundheit < 0)
                _gesundheit = 0;
        }

        public void SetGesundheit(int wert)
        {
            _gesundheit = wert;
        }

        public int GetGesundheit()
        {
            return _gesundheit;
        }

        public int GetAnsehen()
        {
            return _ansehen;
        }

        public void ErhoeheAnsehen(int wert)
        {
            _ansehen += wert;

            if (_ansehen < 0)
                _ansehen = 0;
        }

        public void SetAnsehen(int wert)
        {
            _ansehen = wert;
        }

        public void SetTitel(int wert)
        {
            _titel = wert;
        }

        public int GetTitel()
        {
            return _titel;
        }

        public string GetTitelGegendert()
        {
            return SW.Statisch.GetTitelX(_titel).GetName(Maennlich);
        }

        public void SetReligion(int wert)
        {
            Religion = wert;
        }

        public int GetReligion()
        {
            return Religion;
        }

        public abstract int GetGesamtVermoegen(int SpielerID);  // Wird in den erbenden Klassen überschrieben

        public int BeurteileGesundheitIntWert()
        {
            int gfaz = 0;

            if (_gesundheit < 25)
            {
                gfaz = 0;
            }
            else if (_gesundheit < 40)
            {
                gfaz = 1;
            }
            else if (_gesundheit < 55)
            {
                gfaz = 2;
            }
            else if (_gesundheit < 70)
            {
                gfaz = 3;
            }
            else if (_gesundheit < 85)
            {
                gfaz = 4;
            }
            else
            {
                gfaz = 5;
            }

            return gfaz;
        }

        public string BeurteileGesundheitString()
        {
            int gfaz = BeurteileGesundheitIntWert();
            switch (gfaz)
            {
                case 0:
                    return "miserabel";
                case 1:
                    return "schlecht";
                case 2:
                    return "angeschlagen";
                case 3:
                    return "akzeptabel";
                case 4:
                    return "gut";
                case 5:
                    return "ausgezeichnet";
            }
            return "";
        }

        public string GetSeinerIhrer()
        {
            if (GetMaennlich())
                return "seiner";
            else
                return "ihrer";
        }

        public string GetSeinenIhren()
        {
            if (GetMaennlich())
                return "seinen";
            else
                return "ihren";
        }

        public string GetSeinIhr()
        {
            if (GetMaennlich())
                return "sein";
            else
                return "ihr";
        }
        #endregion
    }
}
