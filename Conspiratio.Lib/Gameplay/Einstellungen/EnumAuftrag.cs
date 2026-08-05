namespace Conspiratio.Lib.Gameplay.Einstellungen
{
    /// <summary>
    /// Der bei der Spielerstellung optional gewählte Auftrag (Mission, Vorbild „Die Fugger 2").
    /// <see cref="KeinAuftrag"/> (= 0, Standard) steht für ein freies, endloses Spiel ohne Siegbedingung;
    /// alte Spielstände ohne dieses Feld werden dadurch automatisch als „ohne Auftrag" behandelt.
    /// Die Aufträge sind nach Schwierigkeit gruppiert (siehe <c>AuftragManager</c>).
    /// </summary>
    public enum EnumAuftrag
    {
        KeinAuftrag = 0,

        // leicht
        Aufsteiger,
        KleinerWohlstand,

        // mittel
        HerrDesDoms,
        Maezen,
        Baumeister,

        // schwer
        Talerrennen,
        Kriegsherr
    }
}
