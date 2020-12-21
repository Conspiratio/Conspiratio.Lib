using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafePranger : Strafe
    {
        public StrafePranger() : base("Pranger")
        {
        }

        public override string StrafeExecute(int opferID, int deliktpunkte)
        {
            double faktor = 50d;
            double deliktMultiplikator = (Convert.ToDouble(deliktpunkte) / 100d) + 1d;
            int ansehensaenderung = Convert.ToInt32(Math.Abs(Math.Round(faktor * deliktMultiplikator, 0))) * -1;

            if (opferID < SW.Statisch.GetMinKIID())
            {
                SW.Dynamisch.GetHumWithID(opferID).ErhoehePermaAnsehen(ansehensaenderung);
            }
            else
            {
                SW.Dynamisch.GetSpWithID(opferID).ErhoeheAnsehen(ansehensaenderung);
            }

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss einen Tag am Pranger verbringen.\nDas Ansehen von " + SW.Dynamisch.GetSpWithID(opferID).GetName() + " hat deutlich gelitten";
        }
    }
}
