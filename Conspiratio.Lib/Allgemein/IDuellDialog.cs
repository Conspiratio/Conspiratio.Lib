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
        /// <summary>
        /// Trägt das Duell aus – interaktiv als Wortgefecht oder (nach Wahl des Clients) als reine
        /// Animation – und liefert, ob der aktive Spieler gewonnen hat. Die Szene bleibt danach offen,
        /// damit der Aufrufer die Folgen anwenden und sie mit <see cref="ZeigeAusgang"/> anzeigen kann.
        /// </summary>
        /// <param name="gefecht">Regelwerk des Wortgefechts (Runden, Sprüche, Wertung).</param>
        /// <param name="gegnerName">Vollständiger Name des Gegners.</param>
        Task<bool> SpieleWortgefecht(WortgefechtManager gefecht, string gegnerName);

        /// <summary>Zeigt den Ausgang in der noch offenen Szene und wartet, bis der Spieler sie schließt.</summary>
        /// <param name="spielerGewinnt">Hat der aktive (menschliche) Spieler das Duell gewonnen?</param>
        /// <param name="gegnerName">Vollständiger Name des Gegners.</param>
        /// <param name="amtVerloren">Muss der Verlierer sein Amt niederlegen?</param>
        /// <param name="amtName">Name dieses Amtes (leer, wenn kein Amt verloren geht).</param>
        Task ZeigeAusgang(bool spielerGewinnt, string gegnerName, bool amtVerloren, string amtName);
    }
}
