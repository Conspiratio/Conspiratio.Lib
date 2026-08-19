using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    public enum AggressionsAktion { Sabotage, Anschwaerzen }

    /// <summary>Ergebnis einer von einer KI ausgelösten feindlichen Aktion gegen den aktiven Menschen.</summary>
    public class AggressionsErgebnis
    {
        public int TaeterId { get; internal set; }
        public AggressionsAktion Aktion { get; internal set; }

        /// <summary>Nur bei <see cref="AggressionsAktion.Anschwaerzen"/> befüllt.</summary>
        public string Meldung { get; internal set; }
    }

    /// <summary>
    /// Aggressive KI (Issue): KI-Spieler mit schlechter Beziehung zum aktiven Menschen setzen
    /// Saboteure gegen dessen Besitz ein oder schwärzen ihn bei anderen Würdenträgern an – verstärkt,
    /// wenn sie Beweise gegen ihn haben. Nutzt dieselbe Feindseligkeits-Formel wie die KI-Beleidigung
    /// (<see cref="FechtDuellManager.BerechneKiFeindseligkeitChance"/>), aber über alle KIs statt nur
    /// Amtsträger, da beide Aktionen kein Amt voraussetzen.
    /// </summary>
    public class AggressionManager
    {
        public const int SabotageDauerJahre = 5;

        /// <summary>
        /// Prüft für jede KI außer <paramref name="ausgenommenId"/> unabhängig, ob sie in diesem Zug
        /// den aktiven Menschen angreift. Jede KI führt höchstens eine Aktion aus.
        /// </summary>
        /// <param name="ausgenommenId">
        /// KI, die diese Runde bereits über <see cref="FechtDuellManager.PruefeKiBeleidigtSpieler"/>
        /// beleidigt hat (oder 0) – bleibt außen vor, damit keine KI zwei feindliche Aktionen im
        /// selben Zug gegen denselben Menschen ausführt.
        /// </param>
        [PublicAPI]
        public List<AggressionsErgebnis> PruefeKiAggression(int ausgenommenId)
        {
            var ergebnisse = new List<AggressionsErgebnis>();
            var mensch = SW.Dynamisch.GetAktHum();
            int menschId = SW.Dynamisch.GetAktiverSpieler();

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (i == ausgenommenId)
                    continue;

                var ki = SW.Dynamisch.GetKIwithID(i);
                int chance = FechtDuellManager.BerechneKiFeindseligkeitChance(ki.GetBeziehungZuKIX(menschId), ki.GetBosheit());

                if (SW.Statisch.Rnd.Next(0, 100) >= chance)
                    continue;


                bool laeuftSchonSabotage = mensch.GetGegnerischeSabotage(i).GetDauer() > 0;

                if (laeuftSchonSabotage)
                {
                    // Anschwärzen-Zweig folgt in Task 6.
                    continue;
                }

                mensch.GetGegnerischeSabotage(i).SetDauer(SabotageDauerJahre);
                var ergebnis = new AggressionsErgebnis();
                ergebnis.TaeterId = i;
                ergebnis.Aktion = AggressionsAktion.Sabotage;
                ergebnisse.Add(ergebnis);
            }

            return ergebnisse;
        }
    }
}
