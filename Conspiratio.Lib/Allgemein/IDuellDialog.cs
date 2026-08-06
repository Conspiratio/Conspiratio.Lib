using System.Threading.Tasks;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Vollbild-Inszenierung eines Duells im Morgengrauen (Issue #17): Der Client spielt den Kampf als
    /// kurze Szene ab (Nebel verhüllt den Schlagabtausch) und zeigt anschließend den Ausgang. Die Logik
    /// ist zu diesem Zeitpunkt bereits ausgewertet – der Dialog stellt sie nur noch dar.
    /// </summary>
    public interface IDuellDialog
    {
        /// <param name="spielerGewinnt">Hat der aktive (menschliche) Spieler das Duell gewonnen?</param>
        /// <param name="gegnerName">Vollständiger Name des Gegners.</param>
        /// <param name="amtVerloren">Muss der Verlierer sein Amt niederlegen?</param>
        /// <param name="amtName">Name dieses Amtes (leer, wenn kein Amt verloren geht).</param>
        Task ShowDuell(bool spielerGewinnt, string gegnerName, bool amtVerloren, string amtName);
    }
}
