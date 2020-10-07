namespace Conspiratio.Lib.Gameplay.Niederlassung
{
    /// <summary>
    /// Stellt alle vorhandenen Aktionsarten eines Produktionsslots in einer Stadt dar.
    /// </summary>
    public enum EnumProduktionsslotAktionsart
    {
        /// <summary>
        /// Kein Auftrag ausgewählt
        /// </summary>
        KeinAuftrag = 0,

        /// <summary>
        /// Ware herstellen
        /// </summary>
        Produzieren = 1,

        /// <summary>
        /// Ware in anderer Stadt einmalig verkaufen (exportieren)
        /// </summary>
        Verkaufen = 2,

        /// <summary>
        /// Ware in anderer Stadt permanent verkaufen (exportieren)
        /// </summary>
        PermanentVerkaufen = 3
    }
}
