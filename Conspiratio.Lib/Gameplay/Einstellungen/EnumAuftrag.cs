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
        Kriegsherr,

        // Weitere Aufträge – ans Ende angehängt, damit die bestehenden Enum-Werte (und damit alte
        // Spielstände) stabil bleiben. Die Anzeige-Gruppierung erfolgt über die Schwierigkeit, nicht
        // über die Enum-Reihenfolge.
        Kaufmann,          // leicht
        Familienvater,     // leicht
        Wahlsieger,        // mittel
        Kriegsheld,        // mittel
        Karawanenschreck,  // schwer
        Meuchelmoerder     // schwer
    }
}
