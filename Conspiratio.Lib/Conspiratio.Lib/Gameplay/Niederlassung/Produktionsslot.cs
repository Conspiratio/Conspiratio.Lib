using System;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Niederlassung
{
    [Serializable]
    public class Produktionsslot
    {
        private int _taetigkeit;

        private int _produktionRohstoff;
        private int _produktionArbeiter;
        private int _produktionStaetten;

        private int _verkaufRohstoff;
        private int _verkaufAnzahl;
        private int _verkaufStadt;

        private int _gestohlenAnzahl = 0;

        public Produktionsslot(int taetigkeit, int produktionRohstoff, int produktionArbeiter, int produktionStaetten, int verkaufRohstoff, int verkaufAnzahl, int verkaufStadt)
        {
            _taetigkeit = taetigkeit;

            _produktionRohstoff = produktionRohstoff;
            _produktionArbeiter = produktionArbeiter;
            _produktionStaetten = produktionStaetten;

            _verkaufRohstoff = verkaufRohstoff;
            _verkaufAnzahl = verkaufAnzahl;
            _verkaufStadt = verkaufStadt;
        }

        public int GetTaetigkeit()
        {
            return _taetigkeit;
        }

        public int GetProduktionRohstoff()
        {
            return _produktionRohstoff;
        }

        public int GetProduktionArbeiter()
        {
            return _produktionArbeiter;
        }

        public int GetVerkaufAnzahl()
        {
            return _verkaufAnzahl;
        }

        public int GetVerkaufRohstoff()
        {
            return _verkaufRohstoff;
        }

        public int GetVerkaufStadt()
        {
            return _verkaufStadt;
        }

        public int GetProduktionStaetten()
        {
            return _produktionStaetten;
        }

        public int GetGestohlenAnzahl()
        {
            return _gestohlenAnzahl;
        }

        public void SetTaetigkeit(int i)
        {
            _taetigkeit = i;
        }

        public void SetProduktionRohstoff(int i)
        {
            _produktionRohstoff = i;
        }

        public void SetVerkaufStadt(int i)
        {
            _verkaufStadt = i;
        }

        public void SetVerkaufRohstoff(int i)
        {
            _verkaufRohstoff = i;
        }

        public void SetVerkaufAnzahl(int i)
        {
            _verkaufAnzahl = i;
        }

        public void SetProduktionArbeiter(int i)
        {
            _produktionArbeiter = i;
        }

        public void SetProduktionStaetten(int i)
        {
            _produktionStaetten = i;
        }

        public void SetGestohlenAnzahl(int i)
        {
            _gestohlenAnzahl = i;
        }

        public int GetProduktion(int stadtID, int rohstoffID, ref int qualitaetProzent)
        {
            if (_produktionStaetten == 0)
                return 0;

            int verharbeiter = SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetArbeiter();
            int verhwerkstaetten = SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetWerkstaetten();
            int ProdmaxmitWS = _produktionStaetten * SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetWSProdProWS();
            int Prodmitrichtigerarbeiteranzahl = (ProdmaxmitWS * 9) / 10;
            int vorlaeufigeProduktion = 0;
            int benArbeiter = Convert.ToInt32(_produktionStaetten * (verharbeiter / verhwerkstaetten));

            if (_produktionArbeiter >= benArbeiter)
            {
                vorlaeufigeProduktion = Prodmitrichtigerarbeiteranzahl;
            }
            else
            {
                double verh = Convert.ToDouble(_produktionArbeiter) / Convert.ToDouble(benArbeiter);
                vorlaeufigeProduktion = Convert.ToInt32(verh * Prodmitrichtigerarbeiteranzahl);
            }

            // Hauptproduktion
            double factor = SW.Dynamisch.GetStadtwithID(stadtID).GetEffizienzVonRohstoffMitIDX(rohstoffID);
            vorlaeufigeProduktion = Convert.ToInt32(vorlaeufigeProduktion * factor);

            // Plus Minus eine Random Zahl (Schwankung)
            int PlusMinus = Convert.ToInt32(vorlaeufigeProduktion * 0.1);
            int Schwankung = SW.Statisch.Rnd.Next(-PlusMinus, PlusMinus);
            int Produktion = Convert.ToInt32(vorlaeufigeProduktion) + Schwankung;

            /* 
            Rechenweg Beispiel
            Plusminus: 10
            10 / 5 = 2 (Faktor)
            Bei Schwankung: 3 
              3 / 2 =  1,5 + 5 = 6,5 = 65 Qualität in Prozent
            Bei Schwankung: -4 
             -4 / 2 = -2   + 5 = 3   = 30 Qualität in Prozent
            */
            qualitaetProzent = Convert.ToInt32((Convert.ToDouble(Schwankung) / (Convert.ToDouble(PlusMinus) / 5d) + 5) * 10);

            return Produktion;
        }
    }
}
