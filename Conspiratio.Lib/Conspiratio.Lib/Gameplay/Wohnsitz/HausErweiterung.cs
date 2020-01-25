namespace Conspiratio.Lib.Gameplay.Wohnsitz
{
    public class HausErweiterung
    {
        /// <summary>
        /// Gibt die ID bzw. Stufe des Hauses an, für das diese Erweiterung gebaut werden kann. Hat Einfluss auf den Kaufpreis.
        /// </summary>
        public int HausID { get; }

        /// <summary>
        /// HausErweiterungID: Gibt die ID der Hauserweiterung an. Diese ist pro Haus eindeutig (nicht pro Wohnsitz des Spielers).
        /// </summary>
        public int HausErweiterungID { get; }

        /// <summary>
        /// Name: Gibt die Bezeichnung der Hauserweiterung an, die an den Namen des Hauses gehängt wird, z.B. mit 'weitläufigen Ländereien'.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Name: Gibt die Bezeichnung der Hauserweiterung für den Kauf an, z.B. 'weitläufige Ländereien'.
        /// </summary>
        public string NameFuerKauf { get; }

        /// <summary>
        /// Ansehensbonus: Gibt den Ansehensbonus (oder Malus) der Hauswerweiterung an.
        /// </summary>
        public int Ansehensbonus { get; }

        /// <summary>
        /// Gesundheitsbonus: Gibt den Gesundheitsbonus (oder Malus) der Hauswerweiterung an.
        /// </summary>
        public int Gesundheitsbonus { get; }

        /// <summary>
        /// Kaufpreis: Gibt den Kaufpreis der Hauswerweiterung an.
        /// </summary>
        public int Kaufpreis { get; }

        public HausErweiterung(int hausID, int hausErweiterungID, string name, string nameFuerKauf, int ansehensbonus, int gesundheitsbonus)
        {
            HausID = hausID;
            HausErweiterungID = hausErweiterungID;
            Name = name;
            NameFuerKauf = nameFuerKauf;
            Ansehensbonus = ansehensbonus;
            Gesundheitsbonus = gesundheitsbonus;
            Kaufpreis = ((150 * Ansehensbonus + 200 * gesundheitsbonus) * (HausID + 3) * (HausID + 5)) / 20;
        }
    }
}
