using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Kampf.Einheiten;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Einfache Datenklasse zur Abbildung eines Kampf-Ergebnisses
    /// </summary>
    [Serializable]
    public class KampfErgebnis
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
        /// ID des Spielers, der den Kampf gewonnen hat (Spieler oder KI)
        /// </summary>
        public int SpielerIDGewinner { get; set; }

        /// <summary>
        /// Moral der Truppen des Angreifers
        /// </summary>
        public int MoralAngreifer { get; set; }

        /// <summary>
        /// Moral der Truppen des Verteidigers
        /// </summary>
        public int MoralVerteidiger { get; set; }

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
        /// Verluste des Angreifers
        /// </summary>
        public List<Einheit> VerlusteAngreifer { get; set; }

        /// <summary>
        /// Verluste des Verteidigers
        /// </summary>
        public List<Einheit> VerlusteVerteidiger { get; set; }

        /// <summary>
        /// Zusammenfassung des Ergebnisses als Text
        /// </summary>
        public string Zusammenfassung { get; set; }

        /// <summary>
        /// Art des Kampfes
        /// </summary>
        public EnumKampfArt KampfArt { get; set; }

        /// <summary>
        /// Karawane, die überfallen wurde (sofern es sich um einen Überfall handelt)
        /// </summary>
        public KampfKarawane Karawane { get; set; }

        /// <summary>
        /// Initialisert die Werte des Objekts
        /// </summary>
        /// <param name="spielerIDAngreifer">ID des Spielers, der den Kampf begonnen hat, also Angreifer ist (Spieler oder KI)</param>
        /// <param name="spielerIDVerteidiger">ID des Spielers, der im Kampf verteidigt (Spieler oder KI), muss bei Überfällen nicht vorhanden sein</param>
        /// <param name="spielerIDGewinner">ID des Spielers, der den Kampf gewonnen hat (Spieler oder KI)</param>
        /// <param name="moralAngreifer">Moral der Truppen des Angreifers</param>
        /// <param name="moralVerteidiger">Moral der Truppen des Verteidigers</param>
        /// <param name="stuetzpunktIDAngreifer">ID des Stützpunktes des Angreifers</param>
        /// <param name="stuetzpunktIDVerteidiger">ID des Stützpunktes des Verteidigers</param>
        /// <param name="aktionIndexAngreifer">Index (Nummer) der Aktion des Stützpunktes des Angreifers</param>
        /// <param name="aktionIndexVerteidiger">Index (Nummer) der Aktion des Stützpunktes des Verteidigers</param>
        /// <param name="verlusteAngreifer">Verluste des Angreifers</param>
        /// <param name="verlusteVerteidiger">Verluste des Verteidigers</param>
        /// <param name="zusammenfassung">Zusammenfassung des Ergebnisses als Text</param>
        /// <param name="kampfArt">Art des Kampfes</param>
        /// <param name="karawane">Karawane, die überfallen wird (sofern es sich um einen Überfall handelt)</param>
        public KampfErgebnis(int spielerIDAngreifer, int spielerIDVerteidiger, int spielerIDGewinner, int moralAngreifer, int moralVerteidiger, int stuetzpunktIDAngreifer, int stuetzpunktIDVerteidiger,
                             int aktionIndexAngreifer, int aktionIndexVerteidiger, List<Einheit> verlusteAngreifer, List<Einheit> verlusteVerteidiger, string zusammenfassung, 
                             EnumKampfArt kampfArt, KampfKarawane karawane)
        {
            SpielerIDAngreifer = spielerIDAngreifer;
            SpielerIDVerteidiger = spielerIDVerteidiger;
            SpielerIDGewinner = spielerIDGewinner;
            MoralAngreifer = moralAngreifer;
            MoralVerteidiger = moralVerteidiger;
            StuetzpunktIDAngreifer = stuetzpunktIDAngreifer;
            StuetzpunktIDVerteidiger = stuetzpunktIDVerteidiger;
            AktionIndexAngreifer = aktionIndexAngreifer;
            AktionIndexVerteidiger = aktionIndexVerteidiger;
            VerlusteAngreifer = verlusteAngreifer;
            VerlusteVerteidiger = verlusteVerteidiger;
            Zusammenfassung = zusammenfassung;
            KampfArt = kampfArt;
            Karawane = karawane;
        }
    }
}
