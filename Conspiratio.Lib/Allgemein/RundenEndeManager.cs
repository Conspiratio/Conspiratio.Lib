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

        /// <summary>
        /// Lässt die KI-Spieler zum Rundenende zufällig Straftaten begehen (Issue #18). Die tatsächlich
        /// begangenen Verbrechen werden je Gesetz in ihrem Delikt-Speicher (`GetBegingVerbrechenX`) geführt,
        /// können von Spionen als Beweise erkannt und bei einer Gerichtsverhandlung herangezogen werden.
        ///
        /// „Mischung" (vom Nutzer gewählt): Die Basis ist zufällig und skaliert mit der Bosheit der KI; der
        /// Speicher ist additiv, sodass real begangene illegale Aktionen (sobald es solche für die KI gibt)
        /// zusätzlich hineinzählen. Alte Delikte verblassen jährlich (rollierendes Fenster), damit sie ohne
        /// Verurteilung nicht unbegrenzt anwachsen.
        /// </summary>
        [PublicAPI]
        public void FuehreKiStraftatenDurch()
        {
            string[] vorwuerfe = SW.Statisch.GetGerichtsGesetzesvorwurf();

            // Gesetze, gegen die überhaupt verstoßen werden kann (mit hinterlegtem Vorwurf-Text).
            var verletzbareGesetze = new List<int>();
            for (int g = 0; g < SW.Statisch.GetMaxGesetze(); g++)
            {
                if (!string.IsNullOrEmpty(vorwuerfe[g]))
                    verletzbareGesetze.Add(g);
            }

            if (verletzbareGesetze.Count == 0)
                return;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                var ki = SW.Dynamisch.GetKIwithID(i);

                // Alte Delikte verblassen (rollierendes Fenster).
                ki.HalbiereDelikte();

                // Bösere KI begehen mehr zufällige Delikte.
                int hoechstzahl = ki.GetBosheit() / 25;
                if (hoechstzahl < 1)
                    hoechstzahl = 1;

                int anzahl = SW.Statisch.Rnd.Next(0, hoechstzahl + 1);

                for (int n = 0; n < anzahl; n++)
                {
                    int gesetz = verletzbareGesetze[SW.Statisch.Rnd.Next(0, verletzbareGesetze.Count)];
                    ki.ErhoeheGesetzXUmEins(gesetz);
                }
            }
        }

        private static string GetTodesmeldung(int kiId)
        {
            var ki = SW.Dynamisch.GetSpWithID(kiId);
            return ki.GetKompletterName() + " (" + ki.GetAlter() + "†)";
        }
    }
}
