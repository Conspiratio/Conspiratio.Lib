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

        /// <summary>Der standesgemäße Aufwand des Adelstitels, siehe <c>Adelstitel.GetJahresaufwand</c>.</summary>
        public int Hofhaltung { get; internal set; }

        /// <summary>
        /// Jaehrlicher Unterhalt der besessenen Werkstaetten, progressiv in ihrer Zahl. Haengt bewusst
        /// nicht an der Produktion: Sonst waere Horten gratis und Wirtschaften teuer.
        /// </summary>
        public int Unterhalt { get; internal set; }

        /// <summary>
        /// Jährlicher Unterhalt der betriebenen Produktionsstätten, progressiv in ihrer Zahl - die
        /// Bremse für den reinen Händler. Tritt neben <see cref="Unterhalt"/>, der am Besitz hängt:
        /// Wer Werkstätten hält und nicht produziert, zahlt nur jenen, wer groß produziert auch diesen.
        /// Siehe <c>AbrechnungsManager.KapazitaetsunterhaltProMille</c>.
        /// </summary>
        public int Kapazitaetsunterhalt { get; internal set; }

        /// <summary>
        /// Der Jahreslohn des Faktors, 0 ohne einen solchen. Er steht als eigener Posten da und
        /// nicht im Unterhalt, weil er eine Entscheidung ist und keine Folge des Besitzes: Man kann
        /// ihn jederzeit entlassen, eine Werkstätte dagegen nicht abschalten.
        /// </summary>
        public int Faktorlohn { get; internal set; }

        public int Gesamtkosten { get; internal set; }
    }
}
