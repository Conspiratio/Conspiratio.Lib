using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Einfache Datenklasse zur Abbildung eines Kampfes
    /// </summary>
    [Serializable]
    public class Kampf
    {
        /// <summary>
        /// ID des Spielers, der den Kampf begonnen hat, also Angreifer ist (Spieler oder KI)
        /// </summary>
        public int SpielerIDAngreifer { get; set; }

        /// <summary>
        /// ID des Spielers, der im Kampf verteidigt (Spieler oder KI), muss bei Überfällen nicht vorhanden sein
        /// </summary>
        public int SpielerIDVerteidiger { get; set; }

        /// <summary>
        /// Moral der Truppen des Angreifers
        /// </summary>
        public int MoralAngreifer { get; set; }

        /// <summary>
        /// Moral der Truppen des Verteidigers
        /// </summary>
        public int MoralVerteidiger { get; set; }

        /// <summary>
        /// Truppen des Angreifers
        /// </summary>
        public List<Einheit> TruppenAngreifer { get; set; }

        /// <summary>
        /// Truppen des Verteidigers
        /// </summary>
        public List<Einheit> TruppenVerteidiger { get; set; }

        /// <summary>
        /// ID des Stützpunktes des Angreifers
        /// </summary>
        public int StuetzpunktIDAngreifer { get; set; }

        /// <summary>
        /// ID des Stützpunktes des Verteidigers
        /// </summary>
        public int StuetzpunktIDVerteidiger { get; set; }

        /// <summary>
        /// Index (Nummer) der Aktion des Stützpunktes des Angreifers
        /// </summary>
        public int AktionIndexAngreifer { get; set; }

        /// <summary>
        /// Index (Nummer) der Aktion des Stützpunktes des Verteidigers
        /// </summary>
        public int AktionIndexVerteidiger { get; set; }

        /// <summary>
        /// ID des Landes, in dem der Kampf stattfindet
        /// </summary>
        public int LandID { get; set; }

        /// <summary>
        /// Art des Kampfes
        /// </summary>
        public EnumKampfArt KampfArt { get; set; }

        /// <summary>
        /// Karawane, die überfallen wird (sofern es sich um einen Überfall handelt)
        /// </summary>
        public KampfKarawane Karawane { get; set; }

        /// <summary>
        /// Ein leerer Konstruktor, um Objektinitialisierung verwenden zu können, erhöht die Lesbarkeit bei vielen Parametern.
        /// </summary>
        public Kampf()
        {

        }

        /// <summary>
        /// Initialisert die Werte des Objekts
        /// </summary>
        /// <param name="spielerIDAngreifer">ID des Spielers, der den Kampf begonnen hat, also Angreifer ist (Spieler oder KI)</param>
        /// <param name="spielerIDVerteidiger">ID des Spielers, der im Kampf verteidigt (Spieler oder KI), muss bei Überfällen nicht vorhanden sein</param>
        /// <param name="moralAngreifer">Moral der Truppen des Angreifers</param>
        /// <param name="moralVerteidiger">Moral der Truppen des Verteidigers</param>
        /// <param name="truppenAngreifer">Truppen des Angreifers</param>
        /// <param name="truppenVerteidiger">Truppen des Verteidigers</param>
        /// <param name="stuetzpunktIDAngreifer">ID des Stützpunktes des Angreifers</param>
        /// <param name="stuetzpunktIDVerteidiger">ID des Stützpunktes des Verteidigers</param>
        /// <param name="aktionIndexAngreifer">Index (Nummer) der Aktion des Stützpunktes des Angreifers</param>
        /// <param name="aktionIndexVerteidiger">Index (Nummer) der Aktion des Stützpunktes des Verteidigers</param>
        /// <param name="landID">ID des Landes, in dem der Kampf stattfindet</param>
        /// <param name="kampfArt">Art des Kampfes</param>
        /// <param name="karawane">Karawane, die überfallen wird (sofern es sich um einen Überfall handelt)</param>
        public Kampf(int spielerIDAngreifer, int spielerIDVerteidiger, int moralAngreifer, int moralVerteidiger, List<Einheit> truppenAngreifer, List<Einheit> truppenVerteidiger, 
                    int stuetzpunktIDAngreifer, int stuetzpunktIDVerteidiger, int aktionIndexAngreifer, int aktionIndexVerteidiger, int landID, EnumKampfArt kampfArt, KampfKarawane karawane)
        {
            SpielerIDAngreifer = spielerIDAngreifer;
            SpielerIDVerteidiger = spielerIDVerteidiger;
            MoralAngreifer = moralAngreifer;
            MoralVerteidiger = moralVerteidiger;
            TruppenAngreifer = truppenAngreifer;
            TruppenVerteidiger = truppenVerteidiger;
            StuetzpunktIDAngreifer = stuetzpunktIDAngreifer;
            StuetzpunktIDVerteidiger = stuetzpunktIDVerteidiger;
            AktionIndexAngreifer = aktionIndexAngreifer;
            AktionIndexVerteidiger = aktionIndexVerteidiger;
            LandID = landID;
            KampfArt = kampfArt;
            Karawane = karawane;
        }
    }
}
