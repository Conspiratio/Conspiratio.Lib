namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Die einzelnen Kostenpositionen der Jahresabrechnung eines Spielers.
    /// <see cref="Gesamtkosten"/> enthält die tatsächlich abgezogene Summe (Verkaufssteuern
    /// gehen dort ggf. durch Steuerhinterziehungs-Privilegien reduziert ein, angezeigt wird
    /// aber immer der volle Steuerbetrag).
    /// </summary>
    public class AbrechnungsErgebnis
    {
        public int Arbeiterkosten { get; internal set; }

        public int Betriebskosten { get; internal set; }

        public int Transportkosten { get; internal set; }

        public int Verkaufssteuern { get; internal set; }

        public int Informantenkosten { get; internal set; }

        public int Saboteurekosten { get; internal set; }

        public int Kreditzinsen { get; internal set; }

        public int Kirchenzehnt { get; internal set; }

        public int Zollkosten { get; internal set; }

        public int Sold { get; internal set; }

        public int Gesamtkosten { get; internal set; }
    }
}
