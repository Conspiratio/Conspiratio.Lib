namespace Conspiratio.Lib.Gameplay.Wohnsitz
{
    public class HausZustandsbezeichnung
    {
        /// <summary>
        /// Bezeichnung: Gibt die Bezeichnung des Zustands, passend zum Haus, an (z.B. prächtiges)
        /// </summary>
        public string Bezeichnung { get; }

        /// <summary>
        /// VonProzent: Gibt an, ab wie viel Prozent Zustand des Hauses diese Bezeichnung gilt
        /// </summary>
        public int VonProzent { get; }

        /// <summary>
        /// BisProzent: Gibt an, bis wie viel Prozent Zustand des Hauses diese Bezeichnung gilt
        /// </summary>
        public int BisProzent { get; }

        public HausZustandsbezeichnung(string bezeichnung, int vonProzent, int bisProzent)
        {
            Bezeichnung = bezeichnung;
            VonProzent = vonProzent;
            BisProzent = bisProzent;
        }
    }
}
