using System;

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
    }
}
