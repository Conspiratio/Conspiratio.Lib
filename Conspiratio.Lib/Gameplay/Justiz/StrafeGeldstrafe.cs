using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafeGeldstrafe : Strafe
    {
        public StrafeGeldstrafe() : base("Geldstrafe")
        {
        }

        public override string StrafeExecute(int opferID)
        {
            int geldbetrag = (SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr) * 2000;

            SW.Dynamisch.GetSpWithID(opferID).ErhoeheTaler(-geldbetrag);

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss eine Geldstrafe in der\nHöhe von " + geldbetrag.ToStringGeld() + " entrichten.";
        }
    }
}
