using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivHenkersHand : Privileg
    {

        public PrivHenkersHand() : base("Hand des Henkers", 20)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Mit Eurem Amt als Henker gilt Ihr als unehrliche Person. Ihr werdet von anderen gemieden und falls Ihr jemanden berührt, so wird auch diese Person unehrlich und verliert an Ansehen.");

            if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetHenkersHand() == false)
            {
                SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(13);
            }
            else
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr habt dieses Jahr schon einmal Euer Amt missbraucht...");
            }
        }
    }
}
