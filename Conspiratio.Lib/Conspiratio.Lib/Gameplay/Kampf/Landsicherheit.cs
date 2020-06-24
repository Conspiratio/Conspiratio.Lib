using System;
using System.Collections.Generic;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Diese Klasse enthält Informationen über ein Land, zu den Truppen(aktionen) sowie der Sicherheit/Angriffswahrscheinlich.
    /// </summary>
    [Serializable]
    public class Landsicherheit
    {
        #region Variablen und Properties

        /// <summary>
        /// ID des Landes, auf das sich diese Infos beziehen.
        /// </summary>
        public int LandID { get; }

        /// <summary>
        /// Angriffsrisiko in Prozent (wie hoch ist in diesem Land die Wahrscheinlichkeit eines Angriffs durch Räuber)
        /// </summary>
        public int AngriffsrisikoInProzent { get; set; }

        /// <summary>
        /// Aktionen mit diesem Land als Ziel
        /// </summary>
        public List<StuetzpunktAktion> Aktionen { get; set; }

        #endregion

        #region Konstruktor
        /// <summary>
        /// Initialisiert das Objekt.
        /// </summary>
        /// <param name="landID">ID des Landes, auf das sich diese Infos beziehen.</param>
        /// <param name="angriffsrisikoInProzent">Angriffsrisiko in Prozent (wie hoch ist in diesem Land die Wahrscheinlichkeit eines Angriffs durch Räuber)</param>
        /// <param name="aktionen">Aktionen mit diesem Land als Ziel</param>
        public Landsicherheit(int landID, int angriffsrisikoInProzent = 0, List<StuetzpunktAktion> aktionen = null)
        {
            LandID = landID;
            AngriffsrisikoInProzent = angriffsrisikoInProzent;
            Aktionen = aktionen;

            if (Aktionen == null)
                Aktionen = new List<StuetzpunktAktion>();
        }
        #endregion
    }
}
