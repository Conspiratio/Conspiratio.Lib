using System.Collections.Generic;
using System.Linq;

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
    /// (<see cref="FechtDuellManager.BerechneKiFeindseligkeitChance"/>) – und, genau wie diese, auf einer
    /// kleinen Auswahl der feindseligsten KIs statt auf allen ~390. Die Formel liefert eine Chance je
    /// <em>Kandidat</em>; würfelte man sie für jede KI der Spielwelt einzeln aus, käme das Hundertfache
    /// an Ereignissen heraus (gemessen: 4–14 Aktionen pro Zug statt der ~0,07 der KI-Beleidigung).
    /// </summary>
    public class AggressionManager
    {
        public const int SabotageDauerJahre = 5;

        /// <summary>
        /// Wie viele der feindseligsten KIs pro Zug überhaupt eine Aktion in Betracht ziehen.
        /// Mehr als eine (anders als bei <see cref="FechtDuellManager.PruefeKiBeleidigtSpieler"/>), damit
        /// mehrere KIs im selben Zug unabhängig voneinander zuschlagen können – aber wenige genug, dass
        /// die Ereignisrate im Rahmen einer seltenen Zugmeldung bleibt.
        /// </summary>
        public const int MaxKandidatenProZug = 3;

        /// <summary>
        /// Prüft für die feindseligsten KIs (höchstens <see cref="MaxKandidatenProZug"/>, ohne
        /// <paramref name="ausgenommenId"/>) unabhängig voneinander, ob sie in diesem Zug den aktiven
        /// Menschen angreifen. Jede KI führt höchstens eine Aktion aus.
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

            foreach (int i in WaehleFeindseligsteKandidaten(menschId, ausgenommenId, MaxKandidatenProZug))
            {
                var ki = SW.Dynamisch.GetKIwithID(i);
                int chance = FechtDuellManager.BerechneKiFeindseligkeitChance(ki.GetBeziehungZuKIX(menschId), ki.GetBosheit());

                if (SW.Statisch.Rnd.Next(0, 100) >= chance)
                    continue;

                bool laeuftSchonSabotage = mensch.GetGegnerischeSabotage(i).GetDauer() > 0;
                bool waehleAnschwaerzen = laeuftSchonSabotage || SW.Statisch.Rnd.Next(0, 2) == 0;

                if (waehleAnschwaerzen)
                {
                    int adressat = WaehleAnschwaerzenAdressat(i);

                    if (adressat == 0)
                        continue; // kein KI-Amtsträger vorhanden, bei dem man anschwärzen könnte

                    // Von hier aus ist das Opfer x immer der Mensch und der Adressat y immer eine andere
                    // KI. Damit können weder der "bei sich selbst"-Zweig noch der "Y berichtet es der
                    // Opfer-KI"-Zweig greifen: eine Meldung != null heißt hier also genau "geglaubt".
                    // Deren Wortlaut ist aber aus Sicht des Anklägers geschrieben ("Euren Worten"), taugt
                    // also nicht für den Menschen, der hier das Opfer ist – daher eine eigene Meldung.
                    if (SW.Dynamisch.AnschwaerzenAusfuehren(i, menschId, adressat) != null)
                    {
                        var anschwaerzenErgebnis = new AggressionsErgebnis();
                        anschwaerzenErgebnis.TaeterId = i;
                        anschwaerzenErgebnis.Aktion = AggressionsAktion.Anschwaerzen;
                        anschwaerzenErgebnis.Meldung = BaueOpferMeldung(i, adressat);
                        ergebnisse.Add(anschwaerzenErgebnis);
                    }

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

        /// <summary>
        /// Die Meldung, die der angeschwärzte Mensch zu lesen bekommt – aus seiner Sicht formuliert und
        /// mit beiden Beteiligten benannt (Gegenstück zu der aus Anklägersicht geschriebenen Meldung von
        /// <see cref="DynamischeSpieldaten.AnschwaerzenAusfuehren"/>).
        /// </summary>
        private static string BaueOpferMeldung(int anklaegerId, int adressatId)
        {
            var anklaeger = SW.Dynamisch.GetSpWithID(anklaegerId);
            string adressatName = SW.Dynamisch.GetSpWithID(adressatId).GetKompletterName();

            return anklaeger.GetKompletterName() + " hat Euch bei " + adressatName + " angeschwärzt – und " +
                   adressatName + " schenkt " + (anklaeger.GetMaennlich() ? "seinen" : "ihren") + " Worten Glauben.";
        }

        /// <summary>
        /// Die <paramref name="anzahl"/> feindseligsten KIs (niedrigste Beziehung zum Menschen zuerst),
        /// ohne <paramref name="ausgenommenId"/>. Spiegelt die Auswahl aus
        /// <see cref="FechtDuellManager.PruefeKiBeleidigtSpieler"/> (dort die eine feindseligste KI),
        /// nur eben für mehrere Kandidaten.
        /// </summary>
        private static List<int> WaehleFeindseligsteKandidaten(int menschId, int ausgenommenId, int anzahl)
        {
            var kandidaten = new List<KeyValuePair<int, int>>();

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (i == ausgenommenId)
                    continue;

                kandidaten.Add(new KeyValuePair<int, int>(i, SW.Dynamisch.GetKIwithID(i).GetBeziehungZuKIX(menschId)));
            }

            return kandidaten.OrderBy(k => k.Value).Take(anzahl).Select(k => k.Key).ToList();
        }

        /// <summary>
        /// Der KI-Amtsträger mit der besten Beziehung zum Ankläger, außer diesem selbst; 0, wenn es
        /// keinen gibt. Angeschwärzt wird bei Würdenträgern – bei jemandem ohne Amt hätte es weder
        /// Gewicht noch Folgen.
        /// </summary>
        private static int WaehleAnschwaerzenAdressat(int anklaegerId)
        {
            int besterAdressat = 0;
            int besteBeziehung = int.MinValue;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (i == anklaegerId)
                    continue;

                var ki = SW.Dynamisch.GetKIwithID(i);

                if (ki.GetAmtID() == 0)
                    continue;

                int beziehungZumAnklaeger = ki.GetBeziehungZuKIX(anklaegerId);

                if (beziehungZumAnklaeger > besteBeziehung)
                {
                    besteBeziehung = beziehungZumAnklaeger;
                    besterAdressat = i;
                }
            }

            return besterAdressat;
        }
    }
}
