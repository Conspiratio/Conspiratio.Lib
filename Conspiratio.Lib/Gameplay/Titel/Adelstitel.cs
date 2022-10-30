using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Titel
{
    public class Adelstitel
    {
        /// <summary>
        /// Rang des Titels (Reihenfolge), startet bei 0 für den ersten Titel.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Männliche Bezeichnung des Titels
        /// </summary>
        public string NameMaennlich { get; }

        /// <summary>
        /// Weibliche Bezeichnung des Titels
        /// </summary>
        public string NameWeiblich { get; }

        /// <summary>
        /// Die Talermenge, die zur Verleihung dieses Titels mindestens notwendig ist.
        /// </summary>
        public int AbTaler { get; }

        /// <summary>
        /// Hat aktuell noch keine Auswirkung. Ist wahrscheinlich für einen Ansehensbonus durch den Titel gedacht gewesen.
        /// </summary>
        public int BonusAnsehen { get; }

        public Adelstitel(int id, string nameMaennlich, string nameWeiblich, int abTaler, int bonusAnsehen)
        {
            Id = id;
            NameMaennlich = nameMaennlich;
            NameWeiblich = nameWeiblich;
            AbTaler = abTaler;
            BonusAnsehen = bonusAnsehen;
        }

        /// <summary>
        /// Liefert die Bezeichnung des Titels abhängig vom Geschlecht.
        /// </summary>
        /// <param name="maennlich">Gibt an, ob der Träger des Titels männlich ist</param>
        /// <returns>Bezeichnung des Titels</returns>
        public string GetName(bool maennlich)
        {
            return maennlich ? NameMaennlich : NameWeiblich;
        }

        /// <summary>
        /// Gibt zurück, ob der übergebene Spieler berechtigt ist, den Titel zu tragen.
        /// </summary>
        /// <param name="dynamischeSpieldaten">Objekt mit den dynamischen Spieldaten</param>
        /// <param name="spielerId">Id des Spielers</param>
        /// <returns>Spieler ist berechtigt (true) oder nicht (false)</returns>
        public virtual bool IstSpielerFuerTitelBerechtigt(DynamischeSpieldaten dynamischeSpieldaten, int spielerId)
        {
            return dynamischeSpieldaten.GetSpWithID(spielerId).GetTaler() >= AbTaler;
        }
    }
}
