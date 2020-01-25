namespace Conspiratio.Lib.Gameplay.Titel
{
    public class Titel
    {
        /// <summary>
        /// Rang des Titels (Reihenfolge), startet bei 0 für den ersten Titel.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Männliche Bezeichnung des Titels
        /// </summary>
        public string NameMaennlich { get; }

        /// <summary>
        /// Weibliche Bezeichnung des Titels
        /// </summary>
        public string NameWeiblich { get; }

        /// <summary>
        /// Hat aktuell noch keine Auswirkung. Ist wahrscheinlich für einen Ansehensbonus durch den Titel gedacht gewesen.
        /// </summary>
        public int BonusAnsehen { get; }

        public Titel(int id, string nameMaennlich, string nameWeiblich, int bonusAnsehen)
        {
            ID = id;
            NameMaennlich = nameMaennlich;
            NameWeiblich = nameWeiblich;
            BonusAnsehen = bonusAnsehen;
        }

        /// <summary>
        /// Liefert die Bezeichnung des Titels abhängig vom Geschlecht.
        /// </summary>
        /// <param name="maennlich">Gibt an, ob der Träger des Titels männlich ist</param>
        /// <returns>Bezeichnung des Titels</returns>
        public string GetName(bool maennlich)
        {
            if (maennlich)
            {
                return NameMaennlich;
            }
            else
            {
                return NameWeiblich;
            }
        }
    }
}
