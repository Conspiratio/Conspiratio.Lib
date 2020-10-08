using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivVergifteterWein : Privileg
    {
        public PrivVergifteterWein() : base("Vergifteter Wein", 17)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als Kellermeister steht Euch der Zugang zu den Getränkevorräten mancher Konkurrenten offen. Dadurch könnt Ihr vergünstigt eine Ermordung verüben.");

            // TODO: Prüfen, ob noch aktiv
            //if (SW.Dynamisch.getHumWithID(SW.Dynamisch.getAktiverSpieler()).getWeinVergiftenVonSpMitID() == 0)
            //{
            //    PolitischeWeltkarte pkw = new PolitischeWeltkarte(12);
            //    pkw.ShowDialog();
            //}
            //else
            //{
            //    SW.Dynamisch.BelTextAnzeigen("Ihr habt dieses Jahr schon einen Anschlag auf " + SW.Dynamisch.getSpWithID(SW.Dynamisch.getHumWithID(SW.Dynamisch.getAktiverSpieler()).getWeinVergiftenVonSpMitID()).getCompleteName() + " geplant");
            //}

            //SpE.setBoolKurzSpeicher(true);
        }
    }
}
