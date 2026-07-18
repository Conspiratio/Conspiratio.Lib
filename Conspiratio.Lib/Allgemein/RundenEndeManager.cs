using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt Ereignisse des Rundenendes (einmal pro Jahr, nachdem der letzte Spieler seinen Zug
    /// beendet hat). Bislang: die Todesfälle unter den KI-Spielern. Ein Todesfall gibt – über KIstirbt –
    /// das Amt des Verstorbenen frei, wodurch im nächsten Jahr neue Wahlen zustande kommen.
    /// </summary>
    public class RundenEndeManager
    {
        /// <summary>
        /// Gibt an, ob die Todesfälle dem Spieler angezeigt werden sollen (Option aus der Spielerstellung).
        /// Die Tode treten unabhängig davon ein.
        /// </summary>
        [PublicAPI]
        public bool SollenTodesfaelleAngezeigtWerden()
        {
            return SW.Dynamisch.TodesfaelleAnzeigen;
        }

        /// <summary>
        /// Führt die Todesfälle unter den KI-Spielern durch: zuerst die feststehenden Tode (GetStirbt),
        /// dann die Zufallstode nach der Sterbeformel (verbleibende Lebensjahre plus Zufall −1..+1 ≤ 0).
        /// Jeder Tote verliert über KIstirbt sein Amt (wodurch eine Wahl entsteht), wird verjüngt und
        /// seine Delikte verfallen bis auf eins.
        /// </summary>
        /// <returns>Je Verstorbenem eine Meldung "Name (Alter†)" in der Reihenfolge der Verarbeitung.</returns>
        [PublicAPI]
        public List<string> FuehreKiTodesfaelleDurch()
        {
            var meldungen = new List<string>();

            // Feststehende Tode zuerst
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (SW.Dynamisch.GetKIwithID(i).GetStirbt())
                {
                    meldungen.Add(GetTodesmeldung(i));
                    SW.Dynamisch.KIstirbt(i);
                }
            }

            // Dann die Zufallstode
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (StirbtNachSterbeformel(i))
                {
                    meldungen.Add(GetTodesmeldung(i));
                    SW.Dynamisch.KIstirbt(i);
                }
            }

            return meldungen;
        }

        /// <summary>
        /// Die Sterbeformel des Originals: verbleibende Lebensjahre plus ein Zufallswert von −1 bis +1;
        /// fällt das Ergebnis auf null oder darunter, stirbt der Spieler.
        /// </summary>
        private static bool StirbtNachSterbeformel(int spielerId)
        {
            return SW.Dynamisch.GetSpXlebtNochSoVielJahre(spielerId) + SW.Statisch.Rnd.Next(-1, 2) <= 0;
        }

        private static string GetTodesmeldung(int kiId)
        {
            var ki = SW.Dynamisch.GetSpWithID(kiId);
            return ki.GetKompletterName() + " (" + ki.GetAlter() + "†)";
        }
    }
}
