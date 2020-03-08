using System;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Schreibstube
{
    [Serializable]
    public class WahlAbhalten
    {
        public int AmtID { get; set; }
        public int GebietID { get; set; }  // Stadt 1-14, Land 1-4, Reich 1
        public int Stufe { get; set; }     // Stadt 0, Land 1, Reich 2

        public int Waehler1 { get; set; }
        public int Waehler2 { get; set; }
        public int Waehler3 { get; set; }

        public int[] Kandidaten { get; set; } = new int[SW.Statisch.GetMaxWahlKandidaten()];

        public WahlAbhalten(int amtID, int gebietID, int stufe, int waehler1, int waehler2, int waehler3)
        {
            AmtID = amtID;
            GebietID = gebietID;
            Stufe = stufe;

            Waehler1 = waehler1;
            Waehler2 = waehler2;
            Waehler3 = waehler3;

            Kandidaten = new int[SW.Statisch.GetMaxWahlKandidaten()];
        }

        // Gibt an ob die Wahl schon angelegt ist, oder nicht
        public bool IstDieWahlVoll()
        {
            return !(AmtID == 0 && GebietID == 0 && Stufe == 0 && 
                     Waehler1 == 0 && Waehler2 == 0 && Waehler3 == 0 && 
                     Kandidaten[0] == 0 && Kandidaten[1] == 0 && Kandidaten[2] == 0 && Kandidaten[3] == 0 && Kandidaten[4] == 0 && Kandidaten[5] == 0 && Kandidaten[6] == 0 && Kandidaten[7] == 0 && Kandidaten[8] == 0 && Kandidaten[9] == 0 && Kandidaten[10] == 0);
        }

        public int[] GetWaehlerAlsArray()
        {
            int[] w = new int[3];

            w[0] = Waehler1;
            w[1] = Waehler2;
            w[2] = Waehler3;

            return w;
        }

        public void NullSetzen()
        {
            Waehler1 = 0;
            Waehler2 = 0;
            Waehler3 = 0;
            AmtID = 0;
            GebietID = 0;
            Stufe = 0;

            for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
                Kandidaten[i] = 0;
        }
    }
}
