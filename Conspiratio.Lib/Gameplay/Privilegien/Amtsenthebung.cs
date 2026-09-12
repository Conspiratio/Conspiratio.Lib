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

        /// <summary>
        /// Wer den Antrag gestellt hat (Spieler-ID), damit das Opfer weiß, gegen wen es sich wehren muss.
        /// 0 heißt „ohne benennbaren Antragsteller“ – so kommt das Feld auch aus Spielständen, die vor
        /// seiner Einführung entstanden sind, und so steht es bei einem gerichtlich angesetzten
        /// Verfahren (<see cref="Justiz.StrafeAmtsenthebung"/>), hinter dem keine Person steht.
        /// </summary>
        public int AntragstellerID;

        public Amtsenthebung(int opferID, int waehler1, int waehler2, int waehler3)
            : this(opferID, waehler1, waehler2, waehler3, 0)
        {
        }

        public Amtsenthebung(int opferID, int waehler1, int waehler2, int waehler3, int antragstellerID)
        {
            OpferID = opferID;
            Waehler1 = waehler1;
            Waehler2 = waehler2;
            Waehler3 = waehler3;
            AntragstellerID = antragstellerID;
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
