using System;

using Newtonsoft.Json;

namespace Conspiratio.Lib.Gameplay.Einstellungen
{
    /// <summary>
    /// Datenklasse mit allen Einstellungen, die sich auf das Spiel beziehen (z.B. auf den Schwierigkeitsgrad).
    /// Diese Einstellungen sind zu unterscheiden von den Clienteinstellungen, die sich auf die Darstellung des Spiel beziehen (z.B. Musiklautstärke, ob etwas angezeigt werden soll, ...)
    /// </summary>
    [Serializable]
    public class Spieleinstellungen
    {
        /// <summary>
        /// Gibt an, wie hoch die Aggressivität der KI-Spieler sein soll. Dies bezieht sich u.a. auf die Häufigkeit von Anklagen und die Häufigkeit der Aktionen in den Militärstütztpunkten.
        /// </summary>
        public EnumSchwierigkeitsgrad AggressivitaetKISpieler { get; set; } = EnumSchwierigkeitsgrad.Mittel;

        /// <summary>
        /// Aggressivität der KI-Spieler als Prozentwert (1–100, Standard 50). Steuert sämtliche
        /// feindseligen und militärischen KI-Aktivitäten: die Bosheit der KI-Charaktere (und damit
        /// Beleidigungen, Duelle, Sabotage, Anschwärzen und KI-Verbrechen), Anklagen, Gerichtsurteile,
        /// Amtsenthebungen sowie Ausbau und Aktionen der Militärstützpunkte. 50 % entspricht dem
        /// bisherigen Normalwert; alte Spielstände (Wert 0) werden wie 50 % behandelt.
        /// </summary>
        private int _kiAggressivitaetProzent = 50;

        [JsonProperty("KiAggressivitaetProzent")]
        public int KiAggressivitaetProzent
        {
            get => _kiAggressivitaetProzent <= 0 ? 50 : _kiAggressivitaetProzent;
            set => _kiAggressivitaetProzent = value;
        }

        /// <summary>
        /// Der bei der Spielerstellung optional gewählte Auftrag (Mission). Standard
        /// <see cref="EnumAuftrag.KeinAuftrag"/> = freies/endloses Spiel ohne Siegbedingung. Alte
        /// Spielstände ohne dieses Feld deserialisieren mit dem Standard (= kein Auftrag).
        /// </summary>
        public EnumAuftrag Auftrag { get; set; } = EnumAuftrag.KeinAuftrag;
    }
}
