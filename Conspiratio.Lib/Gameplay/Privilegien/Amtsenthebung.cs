using System;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    [Serializable]
    public class Amtsenthebung
    {
        public int OpferID;
        public int Waehler1;
        public int Waehler2;
        public int Waehler3;

        public Amtsenthebung(int opferID, int waehler1, int waehler2, int waehler3)
        {
            OpferID = opferID;
            Waehler1 = waehler1;
            Waehler2 = waehler2;
            Waehler3 = waehler3;
        }

        public int[] GetWaehler()
        {
            int[] w = new int[3];

            w[0] = Waehler1;
            w[1] = Waehler2;
            w[2] = Waehler3;

            return w;
        }
    }
}
