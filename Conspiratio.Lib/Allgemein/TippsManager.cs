using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Stellt die Spieltipps für die Anzeige bereit (Migration von TippsAnzeigen): liefert einen
    /// zufälligen Start-Tipp, den Text zu einem Index sowie die Navigation zum nächsten/vorherigen Tipp.
    /// </summary>
    public class TippsManager
    {
        /// <summary>Der höchste gültige Tipp-Index.</summary>
        public int MaxIndex => SW.Statisch.GetTippsMaxIndex();

        /// <summary>Ein zufälliger Start-Index.</summary>
        public int ZufaelligerIndex() => SW.Statisch.Rnd.Next(0, MaxIndex + 1);

        /// <summary>Der Tipp-Text zum angegebenen Index.</summary>
        public string GetTipp(int index) => SW.Statisch.Tipps[index];

        /// <summary>Liefert den nächsten belegten Tipp-Index (oder den aktuellen, wenn keiner mehr folgt).</summary>
        public int NaechsterIndex(int aktuell)
        {
            if (aktuell < MaxIndex && !string.IsNullOrEmpty(SW.Statisch.Tipps[aktuell + 1]))
                return aktuell + 1;

            return aktuell;
        }

        /// <summary>Liefert den vorherigen Tipp-Index (oder den aktuellen, wenn schon der erste erreicht ist).</summary>
        public int VorherigerIndex(int aktuell)
        {
            return aktuell > 0 ? aktuell - 1 : aktuell;
        }
    }
}
