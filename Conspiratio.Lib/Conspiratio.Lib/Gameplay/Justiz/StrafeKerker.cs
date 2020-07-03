using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Justiz
{
    public class StrafeKerker : Strafe
    {
        public StrafeKerker() : base("Kerker")
        {
        }

        public override string StrafeExecute(int opferID)
        {
            SW.Dynamisch.GetSpWithID(opferID).ErhoeheGesundheit(-10);

            return SW.Dynamisch.GetSpWithID(opferID).GetName() + " muss einen Monat im Kerker verbringen.\nDie Gesundheit von " + SW.Dynamisch.GetSpWithID(opferID).GetName() + " hat gelitten.";
        }
    }
}
