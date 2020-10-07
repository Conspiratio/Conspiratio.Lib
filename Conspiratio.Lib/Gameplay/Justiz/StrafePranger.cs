using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafePranger : Strafe
    {
        public StrafePranger() : base("Pranger")
        {
        }

        public override string StrafeExecute(int opferID)
        {
            if (opferID < SW.Statisch.GetMinKIID())
            {
                SW.Dynamisch.GetHumWithID(opferID).ErhoehePermaAnsehen(-50);
            }
            else
            {
                SW.Dynamisch.GetSpWithID(opferID).ErhoeheAnsehen(-50);
            }

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss einen Tag am Pranger verbringen.\nDas Ansehen von " + SW.Dynamisch.GetSpWithID(opferID).GetName() + " hat gelitten";
        }
    }
}
