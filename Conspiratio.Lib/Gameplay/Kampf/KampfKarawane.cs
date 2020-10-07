using System;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Einfache Datenklasse zur Abbildung einer Karawane, die in einem Kampf überfallen wird
    /// </summary>
    [Serializable]
    public class KampfKarawane
    {
        /// <summary>
        /// ID des Spielers, dem die Karawane gehört (Spieler oder KI)
        /// </summary>
        public int SpielerID { get; set; }

        /// <summary>
        /// ID der Stadt, von der aus die Karawane gestartet ist
        /// </summary>
        public int StadtID { get; set; }

        /// <summary>
        /// ID der Karawane, die der Spieler in der Stadt ausgewählt hat
        /// </summary>
        public int KarawaneID { get; set; }

        /// <summary>
        /// Nummer des Produktionsslots der 'Verkaufen' Aktion in der Werktstatt
        /// </summary>
        public int ProduktionsslotNr { get; set; }

        /// <summary>
        /// ID des Rohstoffes, der von der Karawane transportiert wird
        /// </summary>
        public int RohstoffID { get; set; }

        /// <summary>
        /// Menge des Rohstoffes, die von der Karawane transportiert wird
        /// </summary>
        public int Menge { get; set; }

        /// <summary>
        /// Warenwert in Talern, der von der Karawane transportiert worden ist
        /// </summary>
        public int Warenwert { get; set; }

        /// <summary>
        /// Initialisert die Werte des Objekts
        /// </summary>
        /// <param name="spielerID">ID des Spielers, dem die Karawane gehört (Spieler oder KI)</param>
        /// <param name="stadtID">ID der Stadt, von der aus die Karawane gestartet ist</param>
        /// <param name="karawaneID">ID der Karawane, die der Spieler in der Stadt ausgewählt hat</param>
        /// <param name="produktionsslotNr">Nummer des Produktionsslots der 'Verkaufen' Aktion in der Werktstatt</param>
        /// <param name="rohstoffID">ID des Rohstoffes, der von der Karawane transportiert wird</param>
        /// <param name="menge">Menge des Rohstoffes, die von der Karawane transportiert wird</param>
        /// <param name="warenwert">Warenwert in Talern, der von der Karawane transportiert worden ist</param>
        public KampfKarawane(int spielerID, int stadtID, int karawaneID, int produktionsslotNr, int rohstoffID, int menge, int warenwert)
        {
            SpielerID = spielerID;
            StadtID = stadtID;
            KarawaneID = karawaneID;
            ProduktionsslotNr = produktionsslotNr;
            RohstoffID = rohstoffID;
            Menge = menge;
            Warenwert = warenwert;
        }
    }
}