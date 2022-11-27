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
            // laengeStrafeInMonaten = 18 / 10 = 1 (abgerundet)

            // Beispiel mit 5 Deliktpunkten (mittlere Schwere der Schuld):
            // deliktMultiplikator: 5 / 100 * 10 + 1 = 1,5
            // gesundheitsaenderung = 15 * 1,5 = 23 * -1 = -23
            // laengeStrafeInMonaten = 23 / 10 = 2 (abgerundet)

            // Beispiel mit 10 Deliktpunkten (hohe Schwere der Schuld):
            // deliktMultiplikator: 10 / 100 * 10 + 1 = 2,0
            // gesundheitsaenderung = 15 * 2,0 = 30 * -1 = -30
            // laengeStrafeInMonaten = 30 / 10 = 3 (abgerundet)

            double faktor = 15d;
            double deliktMultiplikator = (Convert.ToDouble(deliktpunkte) / 100d * 10d) + 1d;
            int gesundheitsaenderung = Convert.ToInt32(Math.Abs(Math.Round(faktor * deliktMultiplikator, 0))) * -1;

            SW.Dynamisch.GetSpWithID(opferID).ErhoeheGesundheit(gesundheitsaenderung);

            int laengeStrafeInMonaten = Convert.ToInt32(Math.Abs(Math.Floor(gesundheitsaenderung / 10d)));
            if (laengeStrafeInMonaten < 1)
                laengeStrafeInMonaten = 1;
            if (laengeStrafeInMonaten > 11)
                laengeStrafeInMonaten = 11;

            string laengeStrafe = laengeStrafeInMonaten.ToString() + (laengeStrafeInMonaten == 1 ? " Monat" : " Monate");

            return $"{SW.Dynamisch.GetSpWithID(opferID).GetName()} muss {laengeStrafe} im Kerker verbringen.\nDie Gesundheit von {SW.Dynamisch.GetSpWithID(opferID).GetName()} hat entsprechend gelitten.";
        }
    }
}
