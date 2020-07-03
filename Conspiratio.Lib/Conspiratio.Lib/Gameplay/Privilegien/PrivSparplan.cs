using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivSparplan : Privileg
    {
        /// <summary>
        /// Gibt an, um welchen Faktor die Baukosten sinken.
        /// </summary>
        public double FaktorReduzierung { get; } = 0.7;

        public PrivSparplan() : base("Sparplan", 15)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) + " könnt Ihr die Planung Eurer Bauten selbst übernehmen und dadurch den ein oder anderen Taler sparen.");
        }
    }
}
