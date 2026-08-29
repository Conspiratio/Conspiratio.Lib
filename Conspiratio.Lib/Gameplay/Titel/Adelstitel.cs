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
        /// Ansehensbonus des Titels: Bürger 5, Edelmann 10, Ritter 20, Landherr 35, Freiherr 55,
        /// Baron 75, Graf 100, Fürst 125, Herzog 150. Fliesst über
        /// <see cref="Personen.Spieler.GetStandesAnsehen"/> in die Beurteilung einer Person ein
        /// (Gericht und Geschworene des Schuldenprozesses) - und ausdrücklich nur dort, nicht in
        /// die Wahl.
        /// </summary>
        public int BonusAnsehen { get; }

        /// <summary>
        /// Anteil von <see cref="AbTaler"/> (in Prozent), den der standesgemäße Aufwand eines Titels
        /// jährlich kostet.
        ///
        /// Bewusst aus der Talerschwelle abgeleitet statt als eigene Tabelle: So können Rang und Kosten
        /// nicht auseinanderlaufen, und wer die Schwelle gerade hält, braucht genau diese Rendite, nur
        /// um sie zu halten. Die beiden titellosen Ränge (Herr, Bürger) haben Schwelle 0 und kosten
        /// damit von selbst nichts – der Spielanfang bleibt unberührt.
        /// </summary>
        public const int AufwandProzent = 10;

        /// <summary>
        /// Was das Führen dieses Titels den Träger im Jahr kostet (Hofhaltung, Gesinde,
        /// Repräsentation). Fällt in der Jahresabrechnung an, siehe
        /// <c>AbrechnungsManager.ErstelleAbrechnungFuerAktivenSpieler</c>.
        /// </summary>
        public int GetJahresaufwand()
        {
            return AbTaler * AufwandProzent / 100;
        }

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
