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

        public int GetProduktion(int stadtID, int rohstoffID, int fertigkeit, ref int qualitaetProzent)
        {
            if (_produktionStaetten == 0)
                return 0;

            int verharbeiter = SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetArbeiter();
            int verhwerkstaetten = SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetWerkstaetten();
            int ProdmaxmitWS = _produktionStaetten * SW.Dynamisch.GetRohstoffwithID(rohstoffID).GetWSProdProWS();
            int Prodmitrichtigerarbeiteranzahl = (ProdmaxmitWS * 9) / 10;
            int vorlaeufigeProduktion = 0;
            // Erst mal nehmen, dann teilen. Die Klammer stand frueher um die Division, und weil sie
            // ganzzahlig ist, ergab sie bei Wolle, Fell und Rum (ein Arbeiter je zwei Werkstaetten)
            // null - der Bedarf dieser drei Waren war damit null, die Bedingung darunter immer
            // erfuellt und die volle Produktion ohne einen einzigen Arbeiter zu haben. Die uebrigen
            // achtzehn Waren haben Werkstaetten = 1, wo die Klammer nicht auffiel.
            int benArbeiter = _produktionStaetten * verharbeiter / verhwerkstaetten;

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

            // Die Produktionsfertigkeit hebt den unteren Rand der Schwankung, nicht den oberen: Der
            // Meister hat weniger Fehlchargen, keine besseren Gluecksjahre. Bei voller Fertigkeit liegt
            // die Schwankung damit in [0, PlusMinus] - ein schlechtes Jahr gibt es nicht mehr, ein
            // ueberdurchschnittliches wird aber auch nicht besser. Fertigkeit 0 laesst alles wie bisher.
            int bonus = PlusMinus * fertigkeit / Personen.HumSpieler.MaxProduktionsfertigkeit;
            Schwankung = Math.Min(PlusMinus, Schwankung + bonus);
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
            // Ohne Menge gibt es keine Schwankung - und ohne Schwankung keine Qualitätsaussage: Die
            // Rechnung unten teilte durch PlusMinus und ergab 0/0 = NaN, was Convert.ToInt32 mit einer
            // OverflowException quittierte. Betroffen war jeder Produktionsplatz, der auf
            // „Produzieren“ steht, aber (noch) keine Arbeiter hat - im Client mit zwei Klicks
            // hergestellt, denn Arbeiter und Stätten sind zwei getrennte Zahlenknöpfe. Der Zug endete
            // dann im Jahresbuch mit einem Absturz. 50 ist der Wert ohne Ausschlag, also genau das,
            // was eine ausgebliebene Schwankung bedeutet.
            qualitaetProzent = PlusMinus == 0
                ? 50
                : Convert.ToInt32((Convert.ToDouble(Schwankung) / (Convert.ToDouble(PlusMinus) / 5d) + 5) * 10);

            return Produktion;
        }
    }
}
