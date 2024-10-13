using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivAmtNiederlegen : Privileg
    {
        public PrivAmtNiederlegen() : base("Amt niederlegen", 2)
        {
        }

        public override void PrivExecute()
        {
            if (SW.UI.JaNeinFrage.ShowDialogText("Wollt Ihr wirklich\nEuer Amt niederlegen?", "Ja", "Nein") == DialogResultGame.Yes)
            {
                // Absetzungsanträge zurückziehen
                for (int i = 1; i < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); i++)
                {
                    if (SW.Dynamisch.GetAmtsenthebungX(i).GetWaehler()[0] == SW.Dynamisch.GetAktiverSpieler() || SW.Dynamisch.GetAmtsenthebungX(i).GetWaehler()[1] == SW.Dynamisch.GetAktiverSpieler() || SW.Dynamisch.GetAmtsenthebungX(i).GetWaehler()[2] == SW.Dynamisch.GetAktiverSpieler())
                    {
                        SW.Dynamisch.SetAmtsenthebungDaten(i, 0, 0, 0, 0);
                    }
                }

                SW.Dynamisch.BelTextAnzeigen("Ihr habt Euch entschieden, Euer Amt als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " niederzulegen. Damit verliert Ihr auch alle damit verbundenen Privilegien");
                SW.Dynamisch.AmtVonXfreigeben(SW.Dynamisch.GetAktiverSpieler());
            }
        }
    }
}
