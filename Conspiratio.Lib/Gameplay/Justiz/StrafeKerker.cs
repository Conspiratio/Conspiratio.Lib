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
            // Beispiel mit 2 Deliktpunkten (niederige Schwere der Schuld):
            // deliktMultiplikator: 2 / 100 * 10 + 1 = 1,2
            // gesundheitsaenderung = 15 * 1,2 = 18 * -1 = -18

            // Beispiel mit 5 Deliktpunkten (mittlere Schwere der Schuld):
            // deliktMultiplikator: 5 / 100 * 10 + 1 = 1,5
            // gesundheitsaenderung = 15 * 1,5 = 23 * -1 = -23

            // Beispiel mit 10 Deliktpunkten (sehr hohe Schwere der Schuld):
            // deliktMultiplikator: 10 / 100 * 10 + 1 = 2,0
            // gesundheitsaenderung = 15 * 2,0 = 30 * -1 = -30

            double faktor = 15d;
            double deliktMultiplikator = (Convert.ToDouble(deliktpunkte) / 100d * 10d) + 1d;
            int gesundheitsaenderung = Convert.ToInt32(Math.Abs(Math.Round(faktor * deliktMultiplikator, 0))) * -1;

            SW.Dynamisch.GetSpWithID(opferID).ErhoeheGesundheit(gesundheitsaenderung);

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss einen Monat im Kerker verbringen.\nDie Gesundheit von " + SW.Dynamisch.GetSpWithID(opferID).GetName() + " hat gelitten.";
        }
    }
}
