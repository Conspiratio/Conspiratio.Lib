using System;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafeGeldstrafe : Strafe
    {
        public StrafeGeldstrafe() : base("Geldstrafe")
        {
        }

        public override string StrafeExecute(int opferID, int deliktpunkte)
        {
            double jahresMultiplikator = SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr;
            double deliktMultiplikator = 2;

            if (jahresMultiplikator > 200)
            {
                jahresMultiplikator = 200; // Hartes Cap ab 200 Jahren Spielzeit, damit der Spieler dann nach einer Verurteilung noch eine Chance hat
                deliktMultiplikator = 1;
            }

            double faktor = 2000;
            
            if (jahresMultiplikator < 10)
                faktor = 180;
            else if (jahresMultiplikator < 15)
                faktor = 800;

            deliktMultiplikator = (Convert.ToDouble(deliktpunkte) * deliktMultiplikator / 100d) + 1d;

            int geldbetrag = Convert.ToInt32(Math.Round(jahresMultiplikator * faktor * deliktMultiplikator, 0));

            SW.Dynamisch.GetSpWithID(opferID).ErhoeheTaler(-geldbetrag);

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss eine Geldstrafe in der\nHöhe von " + geldbetrag.ToStringGeld() + " entrichten.";
        }
    }
}
