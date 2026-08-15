using System;

namespace Conspiratio.Lib.Gameplay.Personen
{
    /// <summary>
    /// Spielübergreifende Kennzahlen eines <see cref="Profil"/>s, die pro Einzelspiel keinen Sinn ergeben
    /// (Anzahl/Höchstwerte über alle Spiele).
    /// </summary>
    [Serializable]
    public class ProfilMeta
    {
        /// <summary>Anzahl der gewerteten Spiele.</summary>
        public int SpieleGesamt { get; set; }

        /// <summary>Summe der gespielten Spieljahre.</summary>
        public int GespielteJahre { get; set; }

        /// <summary>Je erreichtes Höchstvermögen (Maximum über alle Spiele).</summary>
        public int HoechstesVermoegen { get; set; }

        /// <summary>Je erreichtes höchstes Amt (Maximum über alle Spiele).</summary>
        public int HoechstesAmt { get; set; }

        // Architektonisch vorgesehen, in v1 NICHT befüllt und NICHT angezeigt – werden mit dem künftigen
        // Auftrags-System aktiviert (dann gibt es eine Sieg-/Niederlage-Bedingung).
        public int SpieleGewonnen { get; set; }
        public int SpieleVerloren { get; set; }
    }
}
