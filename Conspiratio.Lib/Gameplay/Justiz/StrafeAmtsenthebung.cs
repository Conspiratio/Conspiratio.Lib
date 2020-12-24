using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafeAmtsenthebung : Strafe
    {
        public StrafeAmtsenthebung() : base("Amtsenthebung")
        {
        }

        public override string StrafeExecute(int opferID, int deliktpunkte)
        {
            string meldung;

            if (deliktpunkte > 5)
            {
                // Direkt des Amtes entheben
                SW.Dynamisch.AmtVonXfreigeben(opferID);
                meldung = $"Aufgrund der Schwere der Schuld wird {SW.Dynamisch.GetSpWithID(opferID).GetName()} mit sofortiger Wirkung des Amtes enthoben!";
            }
            else
            {
                // Amtsenthebungsverfahren ansetzen
                SW.Dynamisch.SetAmtsenthebungVonID(opferID);
                meldung = $"Es wird ein Amtsenthebungsverfahren für {SW.Dynamisch.GetSpWithID(opferID).GetName()} in die Wege geleitet.";
            }

            return meldung;
        }
    }
}