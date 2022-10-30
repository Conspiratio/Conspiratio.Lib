using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Titel
{
    public class Fuerst : Adelstitel
    {
        public Fuerst(int id, string nameMaennlich, string nameWeiblich, int abTaler, int bonusAnsehen)
            : base(id, nameMaennlich, nameWeiblich, abTaler, bonusAnsehen)
        {
        }

        /// <inheritdoc/>
        public override bool IstSpielerFuerTitelBerechtigt(DynamischeSpieldaten dynamischeSpieldaten, int spielerId)
        {
            if (!base.IstSpielerFuerTitelBerechtigt(dynamischeSpieldaten, spielerId))
                return false;

            if (!dynamischeSpieldaten.GetHumWithID(spielerId).BesitztSpielerFertigesHaus("Schloss"))
                return false;

            return dynamischeSpieldaten.GetHumWithID(spielerId).GetAnzahlStuetzpunkte(spielerId) >= 1;
        }
    }
}
