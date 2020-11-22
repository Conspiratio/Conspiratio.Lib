using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafeKerker : Strafe
    {
        public StrafeKerker() : base("Kerker")
        {
        }

        public override string StrafeExecute(int opferID, int deliktpunkte)
        {
            double faktor = 10d;
            double deliktMultiplikator = (Convert.ToDouble(deliktpunkte) / 100d) + 1d;
            int gesundheitsaenderung = Convert.ToInt32(Math.Abs(Math.Round(faktor * deliktMultiplikator, 0))) * -1;

            SW.Dynamisch.GetSpWithID(opferID).ErhoeheGesundheit(gesundheitsaenderung);

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss einen Monat im Kerker verbringen.\nDie Gesundheit von " + SW.Dynamisch.GetSpWithID(opferID).GetName() + " hat gelitten.";
        }
    }
}
