using System;

namespace Conspiratio.Lib.Gameplay.Schreibstube
{
    /// <summary>
    /// Eine laufende Erpressung (WinForms-Issue #13): Der Erpresser darf bis einschließlich
    /// <see cref="LaufendBis"/> die Amtsprivilegien des Opfers mitnutzen; das Opfer kann seine eigenen
    /// aktiven Amtsprivilegien so lange nicht gebrauchen.
    /// </summary>
    [Serializable]
    public class Erpressung
    {
        public int OpferId { get; set; }

        /// <summary>Letztes Jahr, in dem die Erpressung wirkt.</summary>
        public int LaufendBis { get; set; }
    }
}
