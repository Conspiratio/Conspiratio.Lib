using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivProzessInitiieren : Privileg
    {
        public PrivProzessInitiieren() : base("Prozess initiieren", 9)
        {
        }

        public override void PrivExecute()
        {
            if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetKlagtSpielerMitIDXAn() == 0)
            {
                SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(8);
            }
            else
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr habt bereits genug mit Eurem Prozess gegen " + SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetKlagtSpielerMitIDXAn()).GetKompletterName() + " zu tun und könnt daher keinen weiteren Prozess in­i­ti­ie­ren.");
            }
        }
    }
}
