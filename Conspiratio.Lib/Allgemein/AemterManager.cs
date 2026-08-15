using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Gameplay.Schreibstube;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt das Ämtersystem aus dem alten WinForms-Client: die Bewerbung um freie Ämter
    /// (BewerbForm/BewerbInfos) sowie die Auszählung der Wahlen am Jahresende (MoeglWahlenAbhalten/
    /// WahlMitIDxAbhalten). Die Eignungsprüfung (Titel, Häuser, Religion) übernimmt die Lib bereits.
    /// </summary>
    public class AemterManager
    {
        #region Bewerbung

        /// <summary>
        /// Prüft, ob es freie Ämter gibt, auf die sich der aktive Spieler bewerben kann.
        /// </summary>
        [PublicAPI]
        public bool GibtEsFreieAemter()
        {
            return SW.Dynamisch.GetAnzahlFreieAemterFuerSpX(SW.Dynamisch.GetAktiverSpieler()) > 0;
        }

        /// <summary>
        /// Liefert die freien Ämter, auf die sich der aktive Spieler bewerben kann (wie im Original maximal zehn).
        /// </summary>
        [PublicAPI]
        public List<BewerbungsAngebot> GetBewerbungsangebote()
        {
            var angebote = new List<BewerbungsAngebot>();

            int aktiv = SW.Dynamisch.GetAktiverSpieler();
            int max = SW.Dynamisch.GetAnzahlFreieAemterFuerSpX(aktiv);

            if (max > 10)
                max = 10;

            int[] wahlIds = SW.Dynamisch.GetFreieAemterFuerSpX(aktiv);

            for (int i = 0; i < max; i++)
            {
                var wahl = SW.Dynamisch.GetWahlX(wahlIds[i]);
                string amtName = SW.Statisch.GetAmtwithID(wahl.AmtID).GetAmtsname(true);
                string gebietName = SW.Dynamisch.GetGebietwithID(wahl.GebietID, wahl.Stufe).GetGebietsName();

                angebote.Add(new BewerbungsAngebot(wahlIds[i], amtName, gebietName, IstFuerWahlAngemeldet(wahlIds[i], aktiv)));
            }

            return angebote;
        }

        /// <summary>
        /// Die Ankündigung der zu Zugbeginn neu zu besetzenden Ämter für den aktiven Spieler (wie im
        /// Original). Liefert <c>null</c>, wenn es aktuell keine freien Ämter gibt, auf die er sich
        /// bewerben könnte. Die Ämter werden mit ihrem Ort aufgelistet.
        /// </summary>
        [PublicAPI]
        public string GetFreieAemterAnkuendigung()
        {
            var angebote = GetBewerbungsangebote();

            if (angebote.Count == 0)
                return null;

            string text = "Folgende Ämter sind neu zu besetzen:\n";

            foreach (var angebot in angebote)
                text += "\n" + angebot.AmtName + " in " + angebot.GebietName;

            text += "\n\nBewerbt Euch in der Schreibstube um ein Amt.";

            return text;
        }

        /// <summary>
        /// Meldet den aktiven Spieler für die angegebene Wahl an oder – falls er dort bereits aufgestellt ist –
        /// wieder ab. Ein Spieler kann sich für mehrere freie Ämter gleichzeitig bewerben; gewinnt er später
        /// eines, werden seine übrigen Bewerbungen automatisch zurückgezogen (siehe <see cref="VergebeAmt"/>).
        /// </summary>
        [PublicAPI]
        public BewerbungsErgebnis WahlAnmeldungUmschalten(int wahlId)
        {
            int aktiv = SW.Dynamisch.GetAktiverSpieler();
            string amtName = SW.Statisch.GetAmtwithID(SW.Dynamisch.GetWahlX(wahlId).AmtID).GetAmtsname(true);

            // Bereits für diese Wahl gemeldet -> Bewerbung zurückziehen
            if (IstFuerWahlAngemeldet(wahlId, aktiv))
            {
                EntferneKandidatAusWahl(wahlId, aktiv);
                return new BewerbungsErgebnis(false, amtName);
            }

            // An der ersten freien Kandidatenstelle eintragen
            int[] kandidaten = SW.Dynamisch.GetWahlX(wahlId).GetKandidaten();

            for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
            {
                if (kandidaten[i] == 0)
                {
                    SW.Dynamisch.GetWahlX(wahlId).SetKandidatenXAufY(i, aktiv);
                    break;
                }
            }

            return new BewerbungsErgebnis(true, amtName);
        }

        /// <summary>Ob der Spieler als Kandidat der angegebenen Wahl eingetragen ist.</summary>
        private static bool IstFuerWahlAngemeldet(int wahlId, int spielerId)
        {
            foreach (int kandidat in SW.Dynamisch.GetWahlX(wahlId).GetKandidaten())
            {
                if (kandidat == spielerId)
                    return true;
            }

            return false;
        }

        private static void EntferneKandidatAusWahl(int wahlId, int spielerId)
        {
            int[] kandidaten = SW.Dynamisch.GetWahlX(wahlId).GetKandidaten();

            for (int i = 0; i < kandidaten.Length; i++)
            {
                if (kandidaten[i] == spielerId)
                {
                    SW.Dynamisch.GetWahlX(wahlId).SetKandidatenXAufY(i, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// Liefert die Wähler und Mitbewerber einer Wahl für die Infoanzeige (BewerbInfos).
        /// </summary>
        [PublicAPI]
        public WahlDetails GetWahlDetails(int wahlId)
        {
            int[] waehlerIds = ErmittleWaehlerSpielerIds(wahlId);
            var waehlerNamen = new List<string>();
            bool istLoswahl = waehlerIds[0] == 0 && waehlerIds[1] == 0 && waehlerIds[2] == 0;

            if (!istLoswahl)
            {
                foreach (int waehlerId in waehlerIds)
                {
                    if (waehlerId != 0)
                        waehlerNamen.Add(SW.Dynamisch.GetSpWithID(waehlerId).GetKompletterName());
                }
            }

            var mitbewerberNamen = new List<string>();
            int[] kandidaten = SW.Dynamisch.GetWahlX(wahlId).GetKandidaten();

            for (int i = 0; i < SW.Statisch.GetKITeilnehmerProWahl(); i++)
            {
                if (kandidaten[i] != 0)
                    mitbewerberNamen.Add(SW.Dynamisch.GetSpWithID(kandidaten[i]).GetKompletterName());
            }

            return new WahlDetails(waehlerNamen, mitbewerberNamen, istLoswahl);
        }

        #endregion

        #region Wahl-Auszählung am Jahresende

        /// <summary>
        /// Liefert die Wahlen, an denen ein menschlicher Spieler beteiligt ist (als Kandidat oder Wähler)
        /// und die daher interaktiv ausgezählt werden. Sortiert nach Amtsstufe absteigend (höchstes Amt
        /// zuerst): Bewirbt sich ein Spieler für mehrere Ämter, wird so zuerst das höchste ausgezählt – gewinnt
        /// er es, ziehen sich seine übrigen Bewerbungen automatisch zurück.
        /// </summary>
        [PublicAPI]
        public List<int> GetWahlenMitMenschlicherBeteiligung()
        {
            var wahlIds = new List<int>();

            for (int i = 1; i < SW.Statisch.GetMaxAnzahlWahlen(); i++)
            {
                if (!SW.Dynamisch.GetWahlX(i).IstDieWahlVoll())
                    continue;

                if (HatMenschlicheBeteiligung(i))
                    wahlIds.Add(i);
            }

            return wahlIds
                .OrderByDescending(id => SW.Statisch.GetAmtwithID(SW.Dynamisch.GetWahlX(id).AmtID).GetAmtsStufe())
                .ToList();
        }

        /// <summary>
        /// Ob an der Wahl ein menschlicher Spieler beteiligt ist (als Kandidat oder Wähler). Wird zur
        /// Auszählung erneut geprüft, da ein Spieler nach dem Gewinn eines höheren Amts als Kandidat wegfällt.
        /// </summary>
        [PublicAPI]
        public bool HatMenschlicheBeteiligung(int wahlId)
        {
            // Menschlicher Kandidat?
            foreach (int kandidat in SW.Dynamisch.GetWahlX(wahlId).GetKandidaten())
            {
                if (kandidat != 0 && kandidat < SW.Statisch.GetMinKIID())
                    return true;
            }

            // Menschlicher Wähler?
            foreach (int waehlerId in ErmittleWaehlerSpielerIds(wahlId))
            {
                if (waehlerId != 0 && waehlerId < SW.Statisch.GetMinKIID())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Baut die Ansicht einer Wahl auf: Amt, Gebiet, Kandidaten (in Anzeigereihenfolge) und die
        /// aufgelösten Wähler-Spieler-IDs (kompaktiert). Gibt es keine Wähler, wird per Los entschieden.
        /// </summary>
        [PublicAPI]
        public WahlAnsicht ErstelleWahlAnsicht(int wahlId)
        {
            var wahl = SW.Dynamisch.GetWahlX(wahlId);

            string amtName = SW.Statisch.GetAmtwithID(wahl.AmtID).GetAmtsname(true);
            string gebietName = SW.Dynamisch.GetGebietwithID(wahl.GebietID, wahl.Stufe).GetGebietsName();

            var kandidaten = new List<WahlKandidat>();

            foreach (int kandidatId in wahl.GetKandidaten())
            {
                if (kandidatId != 0)
                    kandidaten.Add(new WahlKandidat(kandidatId, SW.Dynamisch.GetSpWithID(kandidatId).GetKompletterName()));
            }

            int[] waehlerIds = ErmittleWaehlerSpielerIds(wahlId);
            var waehler = new List<WahlWaehler>();

            foreach (int waehlerId in waehlerIds)
            {
                if (waehlerId != 0)
                {
                    waehler.Add(new WahlWaehler(waehlerId,
                        SW.Dynamisch.GetSpWithID(waehlerId).GetKompletterName(),
                        waehlerId < SW.Statisch.GetMinKIID()));
                }
            }

            return new WahlAnsicht(wahlId, amtName, gebietName, kandidaten, waehler);
        }

        /// <summary>
        /// Ermittelt, für welchen Kandidaten (Index in die Kandidatenliste) ein KI-Wähler stimmt:
        /// den "Sympathisanten" mit der höchsten Summe aus Beziehung, Ansehensbonus und Religionssympathie.
        /// </summary>
        [PublicAPI]
        public int ErmittleKiStimme(int waehlerId, IReadOnlyList<int> kandidatenIds)
        {
            int best = 0;
            int stimme = 0;

            for (int kc = 0; kc < kandidatenIds.Count; kc++)
            {
                int bezieh = SW.Dynamisch.GetKIwithID(waehlerId).GetBeziehungZuKIX(kandidatenIds[kc]);
                int ansehbon = SW.Dynamisch.GetSpWithID(kandidatenIds[kc]).GetAnsehen() / 10;
                int relsympathie = SW.Dynamisch.GetRelSympathieVonXzuY(waehlerId, kandidatenIds[kc]);
                int value = bezieh + ansehbon + relsympathie;

                if (value > best)
                {
                    // Übernahme des Originalverhaltens: der Vergleichswert (best) wird auf die reine Beziehung gesetzt
                    best = bezieh;
                    stimme = kc;
                }
            }

            return stimme;
        }

        /// <summary>
        /// Wertet eine Wahl aus, vergibt das Amt an den Gewinner und räumt die Wahl ab.
        /// Ohne Stimmen (Loswahl) entscheidet das Los; bei Stimmengleichheit ebenfalls.
        /// </summary>
        /// <param name="wahlId">Die auszuwertende Wahl</param>
        /// <param name="stimmenIndizes">Je Wähler der Index des gewählten Kandidaten; leer bei einer Loswahl</param>
        [PublicAPI]
        public WahlErgebnis WerteWahlAus(int wahlId, IReadOnlyList<int> stimmenIndizes)
        {
            var wahl = SW.Dynamisch.GetWahlX(wahlId);
            int[] kandidaten = wahl.GetKandidaten();

            int kandAnzahl = 0;
            for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
            {
                if (kandidaten[i] != 0)
                    kandAnzahl++;
            }

            bool warLoswahl = false;
            int gewinnerIndex;

            if (stimmenIndizes == null || stimmenIndizes.Count == 0)
            {
                // Keine Wähler -> reine Loswahl
                warLoswahl = true;
                gewinnerIndex = SW.Statisch.Rnd.Next(0, kandAnzahl);
            }
            else
            {
                int[] hatStimmen = new int[SW.Statisch.GetMaxWahlKandidaten()];

                foreach (int index in stimmenIndizes)
                    hatStimmen[index]++;

                int max = 0;
                gewinnerIndex = 0;

                for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
                {
                    if (hatStimmen[i] > max)
                    {
                        max = hatStimmen[i];
                        gewinnerIndex = i;
                    }
                }

                // Haben mehrere die Höchstzahl an Stimmen, entscheidet das Los unter ihnen
                int maxCounter = 0;
                for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
                {
                    if (hatStimmen[i] == max)
                        maxCounter++;
                }

                if (maxCounter > 1)
                {
                    warLoswahl = true;
                    int losIndex = SW.Statisch.Rnd.Next(0, maxCounter);
                    int zaehler = 0;

                    for (int i = 0; i < SW.Statisch.GetMaxWahlKandidaten(); i++)
                    {
                        if (hatStimmen[i] == max)
                        {
                            if (zaehler == losIndex)
                                gewinnerIndex = i;

                            zaehler++;
                        }
                    }
                }
            }

            int gewinnerId = kandidaten[gewinnerIndex];
            string amtName = SW.Statisch.GetAmtwithID(wahl.AmtID).GetAmtsname(true);
            string gebietName = SW.Dynamisch.GetGebietwithID(wahl.GebietID, wahl.Stufe).GetGebietsName();
            string gewinnerName = SW.Dynamisch.GetSpWithID(gewinnerId).GetKompletterName();

            // Statistik (Issue #19): Wahlteilnahmen der menschlichen Kandidaten und den Sieg zählen
            // (vor VergebeAmt, das die Kandidatenliste der Wahl abräumt).
            foreach (int kandidatId in kandidaten)
            {
                if (kandidatId != 0 && kandidatId < SW.Statisch.GetMinKIID())
                    SW.Dynamisch.GetHumWithID(kandidatId).GetSpielerStatistik().SWahlenTeilgenommen++;
            }

            if (gewinnerId != 0 && gewinnerId < SW.Statisch.GetMinKIID())
                SW.Dynamisch.GetHumWithID(gewinnerId).GetSpielerStatistik().SWahlenGewonnen++;

            VergebeAmt(wahl, gewinnerId);

            return new WahlErgebnis(gewinnerId, gewinnerName, amtName, gebietName, warLoswahl);
        }

        /// <summary>
        /// Füllt alle verbleibenden vollen Wahlen (ohne menschliche Beteiligung) mit einem zufälligen
        /// KI-Gewinner auf, wie die zweite Schleife von MoeglWahlenAbhalten im Original.
        /// </summary>
        [PublicAPI]
        public void FuelleRestlicheAemter()
        {
            for (int i = 1; i < SW.Statisch.GetMaxAnzahlWahlen(); i++)
            {
                var wahl = SW.Dynamisch.GetWahlX(i);

                if (!wahl.IstDieWahlVoll())
                    continue;

                // IstDieWahlVoll heißt nur "die Wahl ist angelegt", nicht "es haben sich genug beworben".
                // Blind einen der KI-Plätze auszuwürfeln traf daher auch leere Plätze (Kandidat 0) und
                // führte in VergebeAmt zu einer NullReferenceException, die den ganzen Jahreswechsel abbrach.
                var bewerber = new List<int>();

                for (int k = 0; k < SW.Statisch.GetKITeilnehmerProWahl(); k++)
                {
                    if (wahl.GetKandidaten()[k] != 0)
                        bewerber.Add(wahl.GetKandidaten()[k]);
                }

                // Ohne Bewerber bleibt das Amt unbesetzt und die Wahl steht im nächsten Jahr erneut an.
                if (bewerber.Count == 0)
                    continue;

                VergebeAmt(wahl, bewerber[SW.Statisch.Rnd.Next(bewerber.Count)]);
            }
        }

        /// <summary>
        /// Vergibt das Amt der Wahl an den Gewinner: gibt dessen altes Amt frei, trägt das neue ein,
        /// setzt alle Wahlteilnahmen der Kandidaten zurück und räumt die Wahl ab.
        /// </summary>
        private static void VergebeAmt(WahlAbhalten wahl, int gewinnerId)
        {
            if (SW.Dynamisch.GetSpWithID(gewinnerId).GetAmtID() != 0)
                SW.Dynamisch.AmtVonXfreigeben(gewinnerId);

            SW.Dynamisch.AmtAufStufeXGebietYidZanWvergeben(wahl.Stufe, wahl.GebietID, wahl.AmtID, gewinnerId);

            foreach (int kandidatId in wahl.GetKandidaten())
            {
                if (kandidatId != 0)
                    SW.Dynamisch.GetSpWithID(kandidatId).SetWahlTeilnahme(0);
            }

            wahl.NullSetzen();

            // Der Gewinner zieht alle weiteren offenen Bewerbungen zurück: Ein Spieler kann sich für mehrere
            // Ämter gleichzeitig bewerben, hält am Ende aber nur das (höchste) tatsächlich gewonnene.
            SW.Dynamisch.SpielerAusAllenWahlenEntfernen(gewinnerId);
        }

        /// <summary>
        /// Löst die drei Wähler-Amtsstellen einer Wahl in die konkreten Spieler-IDs auf (Stadt-, Land- oder
        /// Reichsebene) und rückt sie – wie das Original – auf die vorderen Array-Stellen.
        /// </summary>
        private static int[] ErmittleWaehlerSpielerIds(int wahlId)
        {
            var wahl = SW.Dynamisch.GetWahlX(wahlId);
            int[] waehlerX = new int[SW.Statisch.GetMaxWahlWaehler()];

            for (int i = 0; i < SW.Statisch.GetMaxWahlWaehler(); i++)
            {
                int waehlerAmtId = wahl.GetWaehler()[i];

                // Stadtebene
                if (waehlerAmtId < SW.Statisch.GetMaxAmtStadtID())
                {
                    waehlerX[i] = SW.Dynamisch.GetStadtwithID(wahl.GebietID).GetAmtX(waehlerAmtId);
                }
                // Landesebene
                else if (waehlerAmtId < SW.Statisch.GetMaxAmtLandID())
                {
                    int gebid = wahl.GebietID;
                    int lid;

                    // z. B. wählt ein Vogt den Bürgermeister -> das Gebiet ist noch mit der Stadt-ID angegeben
                    if (wahl.AmtID < SW.Statisch.GetMaxAmtStadtID())
                        lid = SW.Dynamisch.GetLandIDzuStadtX(gebid);
                    else
                        lid = gebid;

                    waehlerX[i] = SW.Dynamisch.GetLandWithID(lid).GetAmtX(waehlerAmtId);
                }
                // Reichsebene
                else
                {
                    waehlerX[i] = SW.Dynamisch.GetReichWithID(1).GetAmtX(waehlerAmtId);
                }
            }

            // Wähler dürfen im Array nicht mittig leer sein -> auf die vorderen Stellen rücken (wie im Original)
            if (waehlerX[0] == 0 && waehlerX[1] != 0 && waehlerX[2] != 0)
            {
                waehlerX[0] = waehlerX[1];
                waehlerX[1] = waehlerX[2];
                waehlerX[2] = 0;
            }
            else if (waehlerX[0] == 0 && waehlerX[1] == 0 && waehlerX[2] != 0)
            {
                waehlerX[0] = waehlerX[2];
                waehlerX[2] = 0;
            }
            else if (waehlerX[0] == 0 && waehlerX[1] != 0 && waehlerX[2] == 0)
            {
                waehlerX[0] = waehlerX[1];
                waehlerX[1] = 0;
            }
            else if (waehlerX[0] != 0 && waehlerX[1] == 0 && waehlerX[2] != 0)
            {
                waehlerX[1] = waehlerX[2];
                waehlerX[2] = 0;
            }

            return waehlerX;
        }

        #endregion
    }

    /// <summary>Ein freies Amt, auf das sich der Spieler bewerben kann.</summary>
    public class BewerbungsAngebot
    {
        public BewerbungsAngebot(int wahlId, string amtName, string gebietName, bool istAngemeldet)
        {
            WahlId = wahlId;
            AmtName = amtName;
            GebietName = gebietName;
            IstAngemeldet = istAngemeldet;
        }

        public int WahlId { get; }

        public string AmtName { get; }

        public string GebietName { get; }

        public bool IstAngemeldet { get; }
    }

    /// <summary>Das Ergebnis einer An- bzw. Abmeldung zu einer Wahl.</summary>
    public class BewerbungsErgebnis
    {
        public BewerbungsErgebnis(bool angemeldet, string amtName)
        {
            Angemeldet = angemeldet;
            AmtName = amtName;
        }

        /// <summary>True, wenn der Spieler nun aufgestellt ist; false, wenn er seine Bewerbung zurückgezogen hat.</summary>
        public bool Angemeldet { get; }

        public string AmtName { get; }
    }

    /// <summary>Wähler und Mitbewerber einer Wahl für die Infoanzeige.</summary>
    public class WahlDetails
    {
        public WahlDetails(List<string> waehlerNamen, List<string> mitbewerberNamen, bool istLoswahl)
        {
            WaehlerNamen = waehlerNamen;
            MitbewerberNamen = mitbewerberNamen;
            IstLoswahl = istLoswahl;
        }

        public List<string> WaehlerNamen { get; }

        public List<string> MitbewerberNamen { get; }

        /// <summary>True, wenn die Wahl mangels Wählern durch ein Los entschieden wird.</summary>
        public bool IstLoswahl { get; }
    }

    /// <summary>Ein Kandidat einer Wahl.</summary>
    public class WahlKandidat
    {
        public WahlKandidat(int spielerId, string name)
        {
            SpielerId = spielerId;
            Name = name;
        }

        public int SpielerId { get; }

        public string Name { get; }
    }

    /// <summary>Ein Wähler einer Wahl.</summary>
    public class WahlWaehler
    {
        public WahlWaehler(int spielerId, string name, bool istMensch)
        {
            SpielerId = spielerId;
            Name = name;
            IstMensch = istMensch;
        }

        public int SpielerId { get; }

        public string Name { get; }

        public bool IstMensch { get; }
    }

    /// <summary>Die Ansicht einer abzuhaltenden Wahl.</summary>
    public class WahlAnsicht
    {
        public WahlAnsicht(int wahlId, string amtName, string gebietName, List<WahlKandidat> kandidaten, List<WahlWaehler> waehler)
        {
            WahlId = wahlId;
            AmtName = amtName;
            GebietName = gebietName;
            Kandidaten = kandidaten;
            Waehler = waehler;
        }

        public int WahlId { get; }

        public string AmtName { get; }

        public string GebietName { get; }

        public List<WahlKandidat> Kandidaten { get; }

        /// <summary>Die aufgelösten Wähler in Reihenfolge; ist die Liste leer, entscheidet das Los.</summary>
        public List<WahlWaehler> Waehler { get; }

        public bool IstLoswahl => Waehler.Count == 0;
    }

    /// <summary>Das Ergebnis einer ausgewerteten Wahl.</summary>
    public class WahlErgebnis
    {
        public WahlErgebnis(int gewinnerId, string gewinnerName, string amtName, string gebietName, bool warLoswahl)
        {
            GewinnerId = gewinnerId;
            GewinnerName = gewinnerName;
            AmtName = amtName;
            GebietName = gebietName;
            WarLoswahl = warLoswahl;
        }

        public int GewinnerId { get; }

        public string GewinnerName { get; }

        public string AmtName { get; }

        public string GebietName { get; }

        /// <summary>True, wenn die Entscheidung (mangels Wählern oder bei Stimmengleichheit) durch ein Los fiel.</summary>
        public bool WarLoswahl { get; }
    }
}
