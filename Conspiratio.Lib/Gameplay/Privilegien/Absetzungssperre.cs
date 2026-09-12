using System;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    /// <summary>
    /// Vermerk, dass ein bestimmter Antragsteller gegen ein bestimmtes Opfer bereits eine Amtsenthebung
    /// beantragt hat. Solange der Vermerk gilt, kann derselbe Antragsteller gegen dasselbe Opfer keinen
    /// neuen Antrag stellen (<c>DynamischeSpieldaten.IstAbsetzungGesperrt</c>).
    ///
    /// <b>Warum das Antragsjahr und nicht ein fertiges „gesperrt bis“:</b> Die Sperrdauer hängt an der
    /// eingestellten KI-Aggressivität, und die lässt sich im laufenden Spiel verstellen. Mit einem
    /// gespeicherten Enddatum würde eine solche Änderung erst auf künftige Anträge wirken; so wirkt sie
    /// sofort auf alle offenen Sperren.
    /// </summary>
    [Serializable]
    public class Absetzungssperre
    {
        /// <summary>Wer den Antrag gestellt hat.</summary>
        public int AntragstellerId { get; set; }

        /// <summary>Gegen wen er sich richtete.</summary>
        public int OpferId { get; set; }

        /// <summary>In welchem Jahr er gestellt wurde.</summary>
        public int Antragsjahr { get; set; }
    }
}
