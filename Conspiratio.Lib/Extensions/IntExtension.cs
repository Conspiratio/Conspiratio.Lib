namespace Conspiratio.Lib.Extensions
{
    public static class IntExtension
    {
        /// <summary>
        /// Formatiert einen Geldbetrag in einen String mit Tausendertrennzeichen und optional dem Suffix der Währung (z.B. Taler)
        /// </summary>
        /// <param name="Geld">Wert, der formatiert werden soll</param>
        /// <param name="MitWaehrung">OPTIONAL: Gibt an, ob die Währung als Suffix mit angehängt werden soll (z.B. Taler)</param>
        /// <returns>Formatierter Wert</returns>
        public static string ToStringGeld(this int Geld, bool MitWaehrung = true)
        {
            string Ergebnis = Geld.ToString("N0");

            if (MitWaehrung)
                Ergebnis += " Taler";

            return Ergebnis;
        }
    }
}
