namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Das Ergebnis des Jahresbuchs eines Spielers zu Jahresbeginn: Was wurde im letzten Jahr
    /// produziert, exportiert, gestohlen oder mangels Lagerraum verloren?
    /// Alle Arrays sind mit der Rohstoff-ID indiziert.
    /// </summary>
    public class BuchErgebnis
    {
        public BuchErgebnis(int maxRohstoffId)
        {
            ExportierteWaren = new int[maxRohstoffId];
            ExportErloese = new int[maxRohstoffId];
            GestohleneWaren = new int[maxRohstoffId];
            ProduzierteWaren = new int[maxRohstoffId];
            VerloreneWaren = new int[maxRohstoffId];
            ProduktionsQualitaetProzent = new int[maxRohstoffId];
        }

        public int[] ExportierteWaren { get; }

        public int[] ExportErloese { get; }

        public int[] GestohleneWaren { get; }

        public int[] ProduzierteWaren { get; }

        public int[] VerloreneWaren { get; }

        public int[] ProduktionsQualitaetProzent { get; }

        public bool EtwasExportiert { get; internal set; }

        public bool EtwasGestohlen { get; internal set; }

        public bool EtwasProduziert { get; internal set; }

        public bool EtwasVerloren { get; internal set; }
    }
}
