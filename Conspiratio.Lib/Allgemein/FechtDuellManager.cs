using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Ergebnis eines Duells (für Anzeige und Auswertung).</summary>
    public class DuellErgebnis
    {
        public bool SpielerHatGewonnen { get; set; }
        public bool AmtVerloren { get; set; }
        public int VerliererId { get; set; }
        public string Meldung { get; set; }
    }

    /// <summary>
    /// Kapselt Fechtunterricht und Duelle (Issue #17, Vorbild „Die Fugger 2"): Der Spieler kann gegen
    /// steigende Kosten seine Fechtfähigkeit verbessern und einen Amtsträger wegen einer Beleidigung zum
    /// Duell fordern. Der Verlierer verliert Gesundheit; sinkt sie unter eine Schwelle, verliert er sein
    /// Amt (Neuwahl). Das gilt symmetrisch – auch der herausfordernde Spieler kann verlieren.
    /// </summary>
    public class FechtDuellManager
    {
        // Fechtunterricht
        public const int GrundpreisFechtstunde = 1000;
        public const int PreisSteigerungProStunde = 750;
        public const int FaehigkeitProStunde = 5;

        // Duell
        public const int AmtsverlustSchwelle = 30;      // Gesundheit darunter → Amt weg
        private const int SchadenMin = 30;
        private const int SchadenMax = 51;              // Rnd.Next-Obergrenze (exklusiv) → 30..50

        // --- Fechtunterricht --------------------------------------------------------------------------

        /// <summary>Kosten der nächsten Fechtstunde (steigt mit jeder bereits genommenen Stunde).</summary>
        [PublicAPI]
        public int GetKostenNaechsteFechtstunde() =>
            GrundpreisFechtstunde + SW.Dynamisch.GetAktHum().FechtstundenGenommen * PreisSteigerungProStunde;

        /// <summary>Kann der Spieler die nächste Fechtstunde bezahlen?</summary>
        [PublicAPI]
        public bool KannFechtstundeBezahlen() =>
            SW.Dynamisch.GetAktHum().GetTaler() >= GetKostenNaechsteFechtstunde();

        /// <summary>Anzeigetext für den Fechtunterricht (aktuelle Fähigkeit + Kosten der nächsten Stunde).</summary>
        [PublicAPI]
        public string GetAngebotstext()
        {
            var s = SW.Dynamisch.GetAktHum();
            return "Eure Fechtfähigkeit beträgt " + s.Fechtfaehigkeit + ".\n\n" +
                   "Die nächste Fechtstunde kostet " + GetKostenNaechsteFechtstunde().ToStringGeld() +
                   " und hebt Eure Fähigkeit um " + FaehigkeitProStunde + ".";
        }

        /// <summary>Nimmt eine Fechtstunde: zieht die Kosten ab und hebt die Fechtfähigkeit. False, wenn zu teuer.</summary>
        [PublicAPI]
        public bool NimmFechtstunde(out string meldung)
        {
            var s = SW.Dynamisch.GetAktHum();
            int kosten = GetKostenNaechsteFechtstunde();

            if (s.GetTaler() < kosten)
            {
                meldung = "Ihr könnt Euch diese Fechtstunde nicht leisten.";
                return false;
            }

            s.ErhoeheTaler(-kosten);
            s.Fechtfaehigkeit += FaehigkeitProStunde;
            s.FechtstundenGenommen++;

            meldung = "Ihr habt eine Fechtstunde genommen. Eure Fechtfähigkeit beträgt nun " + s.Fechtfaehigkeit + ".";
            return true;
        }

        // --- Duell ------------------------------------------------------------------------------------

        /// <summary>Prüft, ob der aktive Spieler das angegebene Ziel zum Duell fordern kann.</summary>
        [PublicAPI]
        public bool KannDuellFordern(int zielId, out string grund)
        {
            grund = "";
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.DuellGefuehrtDiesesJahr)
            {
                grund = "Ihr habt in diesem Jahr bereits ein Duell ausgetragen.";
                return false;
            }

            if (zielId == SW.Dynamisch.GetAktiverSpieler())
            {
                grund = "Ihr könnt Euch nicht selbst zum Duell fordern.";
                return false;
            }

            if (SW.Dynamisch.GetSpWithID(zielId).GetAmtID() == 0)
            {
                grund = "Nur einen Amtsträger könnt Ihr wegen einer Beleidigung zum Duell fordern.";
                return false;
            }

            return true;
        }

        /// <summary>Bestätigungsfrage vor dem Duell.</summary>
        [PublicAPI]
        public string GetDuellFrage(int zielId) =>
            "Wollt Ihr " + SW.Dynamisch.GetSpWithID(zielId).GetKompletterName() +
            " wegen einer Beleidigung zum Duell fordern?\nDer Verlierer trägt eine Verletzung davon.";

        /// <summary>
        /// Trägt das Duell aus: bestimmt den Sieger (Fechtfähigkeit gegen eine aus der KI-Bosheit
        /// abgeleitete Gegnerstärke plus Zufall), zieht dem Verlierer Gesundheit ab und lässt ihn – fällt
        /// er unter <see cref="AmtsverlustSchwelle"/> – sein Amt verlieren (Neuwahl). Merkt das Duell als
        /// „dieses Jahr geführt" vor. Die Bestätigung ist Sache des Aufrufers.
        /// </summary>
        [PublicAPI]
        public DuellErgebnis FuehreDuellDurch(int zielId)
        {
            var herausforderer = SW.Dynamisch.GetAktHum();
            int herausfordererId = SW.Dynamisch.GetAktiverSpieler();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            string zielName = ziel.GetKompletterName();

            herausforderer.DuellGefuehrtDiesesJahr = true;

            // Gegnerstärke: KI aus ihrer Bosheit (15..40), menschliches Ziel fester Basiswert.
            int gegnerStaerke = zielId >= SW.Statisch.GetMinKIID()
                ? 15 + SW.Dynamisch.GetKIwithID(zielId).GetBosheit() / 4
                : 30;

            int chance = 50 + (herausforderer.Fechtfaehigkeit - gegnerStaerke);
            if (chance < 5) chance = 5;
            if (chance > 95) chance = 95;

            bool spielerGewinnt = SW.Statisch.Rnd.Next(0, 100) < chance;

            int verliererId = spielerGewinnt ? zielId : herausfordererId;
            var verlierer = SW.Dynamisch.GetSpWithID(verliererId);

            // Amtsname vor einem möglichen Amtsverlust festhalten.
            int verliererAmt = verlierer.GetAmtID();
            string amtName = verliererAmt != 0
                ? SW.Statisch.GetAmtwithID(verliererAmt).GetAmtsname(verlierer.GetMaennlich())
                : "";

            verlierer.ErhoeheGesundheit(-SW.Statisch.Rnd.Next(SchadenMin, SchadenMax));

            bool amtVerloren = verliererAmt != 0 && verlierer.GetGesundheit() < AmtsverlustSchwelle;
            if (amtVerloren)
                SW.Dynamisch.AmtVonXfreigeben(verliererId);

            return new DuellErgebnis
            {
                SpielerHatGewonnen = spielerGewinnt,
                AmtVerloren = amtVerloren,
                VerliererId = verliererId,
                Meldung = BaueMeldung(spielerGewinnt, zielName, verlierer, amtVerloren, amtName)
            };
        }

        private static string BaueMeldung(bool spielerGewinnt, string zielName, Spieler verlierer, bool amtVerloren, string amtName)
        {
            string text = spielerGewinnt
                ? "Ihr fordert " + zielName + " und geht als Sieger aus dem Duell hervor! " + verlierer.GetName() + " trägt eine schwere Verletzung davon."
                : "Ihr fordert " + zielName + ", doch die gegnerische Klinge ist überlegen. Ihr tragt eine schwere Verletzung davon.";

            if (amtVerloren)
            {
                text += spielerGewinnt
                    ? "\n\n" + verlierer.GetName() + " ist so schwer verletzt, dass das Amt als " + amtName + " niedergelegt werden muss – es wird neu besetzt."
                    : "\n\nEure Verletzung wiegt so schwer, dass Ihr Euer Amt als " + amtName + " niederlegen müsst.";
            }

            return text;
        }
    }
}
