using System.Threading.Tasks;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Entscheidung eines menschlichen Erpressungsopfers (Issue #13). Anders als eine KI würfelt ein
    /// menschlicher Mitspieler nicht: Ihm werden die Beweise vorgelegt, und er entscheidet selbst, ob er
    /// sich beugt. Lehnt er ab, behält der Erpresser die Beweise für eine Anklage.
    /// </summary>
    public interface IErpressungDialog
    {
        /// <param name="frage">Vorwürfe und Forderung im Klartext.</param>
        /// <returns>True, wenn sich das Opfer der Erpressung beugt.</returns>
        Task<bool> FrageOpfer(string frage);
    }
}
