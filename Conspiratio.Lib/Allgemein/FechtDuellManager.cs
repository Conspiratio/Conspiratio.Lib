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

        /// <summary>Vollständiger Name des Gegners (für die Vollbild-Inszenierung des Duells).</summary>
        public string GegnerName { get; set; }

        /// <summary>Name des Amtes, das der Verlierer niederlegen muss – leer, wenn keines betroffen ist.</summary>
        public string AmtName { get; set; }
    }

    /// <summary>Reaktion eines Ziels auf eine Beleidigung.</summary>
    public class BeleidigungsReaktion
    {
        /// <summary>Ist das beleidigte Ziel ein menschlicher Spieler (dann entscheidet der Aufrufer per Dialog)?</summary>
        public bool ZielIstMensch { get; set; }

        /// <summary>Verlangt das (KI-)Ziel Satisfaktion (nur relevant, wenn <see cref="ZielIstMensch"/> false)?</summary>
        public bool KiVerlangtSatisfaktion { get; set; }
    }

    /// <summary>
    /// Kapselt Fechtunterricht und Duelle (Issue #17, Vorbild „Die Fugger 2"): Der Spieler kann gegen
    /// steigende Kosten seine Fechtfähigkeit verbessern und einen Amtsträger beleidigen. Der Beleidiger
    /// sinkt im Ansehen des Beleidigten; dieser entscheidet, ob er Satisfaktion verlangt (Duell im
    /// Morgengrauen). Verzichtet er, leidet sein eigenes Ansehen bei den Amtsträgern seiner Amtsstufe.
    /// Der Verlierer eines Duells verliert Gesundheit und – fällt sie unter eine Schwelle – sein Amt.
    /// Selten beleidigt auch eine KI den Spieler (<see cref="PruefeKiBeleidigtSpieler"/>).
    /// </summary>
    public class FechtDuellManager
    {
        // Fechtunterricht
        public const int GrundpreisFechtstunde = 1000;
        public const int PreisSteigerungProStunde = 750;
        public const int FaehigkeitProStunde = 5;

        // Duell / Beleidigung
        public const int AmtsverlustSchwelle = 30;          // Gesundheit darunter → Amt weg
        public const int RelationsverlustBeleidigung = 25;  // Beziehungsverlust des Beleidigers beim Ziel
        public const int AnsehensverlustVerweigerung = 15;  // Ansehensverlust bei verweigerter Satisfaktion
        private const int KiSatisfaktionsBasis = 50;        // KI verlangt Satisfaktion mit 50 % + Bosheit/2

        // KI-Beleidigung des Spielers: Die Chance hängt vor allem an der Beziehung der (feindseligsten)
        // KI zum Spieler und steigt erst, wenn diese unter „neutral" (50) liegt – je niedriger, desto
        // höher. Die Bosheit spielt nur eine kleine Rolle. Die Gewichte sind so gewählt, dass die
        // Prozent-Chance je Zug höchstens etwa 7–8 % erreicht (Beziehung 0 + Bosheit 100), im typischen
        // Fall (mäßig unfreundlichste KI) aber deutlich unter „einmal alle 15 Züge" bleibt.
        private const int NeutraleBeziehung = 50;
        private const int KiBeleidigtGewichtBeziehung = 13;
        private const int KiBeleidigtGewichtBosheit = 1;
        private const int SchadenMin = 30;
        private const int SchadenMax = 51;                  // Rnd.Next-Obergrenze (exklusiv) → 30..50

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

        // --- Beleidigung durch den Spieler ------------------------------------------------------------

        /// <summary>Prüft, ob der aktive Spieler das angegebene Ziel beleidigen (und damit zum Duell fordern) kann.</summary>
        [PublicAPI]
        public bool KannBeleidigen(int zielId, out string grund)
        {
            grund = "";
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.DuellGefuehrtDiesesJahr)
            {
                grund = "Ihr habt in diesem Jahr bereits eine Ehrensache ausgetragen.";
                return false;
            }

            if (zielId == SW.Dynamisch.GetAktiverSpieler())
            {
                grund = "Ihr könnt Euch nicht selbst beleidigen.";
                return false;
            }

            if (SW.Dynamisch.GetSpWithID(zielId).GetAmtID() == 0)
            {
                grund = "Nur einen Amtsträger könnt Ihr wegen einer Ehrensache beleidigen.";
                return false;
            }

            return true;
        }

        /// <summary>Bestätigungsfrage vor der Beleidigung (Pronomen nach Geschlecht des Ziels).</summary>
        [PublicAPI]
        public string GetBeleidigungsFrage(int zielId)
        {
            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            string erSie = ziel.GetMaennlich() ? "Er" : "Sie";

            return "Wollt Ihr " + ziel.GetKompletterName() + " öffentlich beleidigen?\n" +
                   erSie + " könnte Satisfaktion verlangen – dann kommt es im Morgengrauen zum Duell.";
        }

        /// <summary>Frage an ein beleidigtes menschliches Ziel, ob es Satisfaktion (Duell) verlangt.</summary>
        [PublicAPI]
        public string GetSatisfaktionsFrage(int beleidigerId) =>
            "Ihr wurdet von " + SW.Dynamisch.GetSpWithID(beleidigerId).GetKompletterName() +
            " öffentlich beleidigt! Verlangt Ihr Satisfaktion – ein Duell im Morgengrauen?";

        /// <summary>
        /// Trägt die Beleidigung aus: Der Beleidiger sinkt deutlich im Ansehen des Ziels (bei KI-Zielen als
        /// Beziehungsverlust), die Jahres-Sperre wird gesetzt. Für KI-Ziele wird zugleich entschieden, ob
        /// die KI Satisfaktion verlangt (abhängig von ihrer Bosheit); bei menschlichen Zielen entscheidet
        /// der Aufrufer per Dialog.
        /// </summary>
        [PublicAPI]
        public BeleidigungsReaktion Beleidige(int zielId)
        {
            int spielerId = SW.Dynamisch.GetAktiverSpieler();
            SW.Dynamisch.GetAktHum().DuellGefuehrtDiesesJahr = true;

            bool zielIstMensch = zielId < SW.Statisch.GetMinKIID();
            bool kiVerlangt = false;

            if (!zielIstMensch)
            {
                var ki = SW.Dynamisch.GetKIwithID(zielId);
                ki.ErhoeheBeziehungZuX(spielerId, -RelationsverlustBeleidigung);
                kiVerlangt = SW.Statisch.Rnd.Next(0, 100) < KiSatisfaktionsBasis + ki.GetBosheit() / 2;
            }

            return new BeleidigungsReaktion { ZielIstMensch = zielIstMensch, KiVerlangtSatisfaktion = kiVerlangt };
        }

        /// <summary>
        /// Der Beleidigte verzichtet auf Satisfaktion: sein Ansehen leidet bei den Amtsträgern seiner
        /// Amtsstufe. Liefert die Meldung (aus Sicht des aktiven Spielers formuliert).
        /// </summary>
        [PublicAPI]
        public string VerweigereSatisfaktion(int verweigerndeId)
        {
            var verweigernde = SW.Dynamisch.GetSpWithID(verweigerndeId);
            verweigernde.ErhoeheAnsehen(-AnsehensverlustVerweigerung);

            bool istAktiverSpieler = verweigerndeId == SW.Dynamisch.GetAktiverSpieler();
            string stufe = GetAmtsstufenText(verweigernde.GetAmtID());

            if (istAktiverSpieler)
                return "Ihr verzichtet auf Satisfaktion. Euer Ansehen" + stufe + " leidet darunter.";

            return verweigernde.GetName() + " nimmt die Beleidigung schweigend hin und verliert durch den " +
                   "Verzicht auf Satisfaktion an Ansehen" + stufe + ".";
        }

        // --- Beleidigung durch die KI -----------------------------------------------------------------

        /// <summary>
        /// Prüft, ob in diesem Zug (selten) eine KI den aktiven Spieler beleidigt. Liefert die ID der
        /// beleidigenden KI (ein zufälliger KI-Amtsträger) oder 0. Die Entscheidung über Satisfaktion trifft
        /// dann der Spieler; verzichtet er, verliert er über <see cref="VerweigereSatisfaktion"/> an Ansehen.
        /// </summary>
        [PublicAPI]
        public int PruefeKiBeleidigtSpieler()
        {
            int spielerId = SW.Dynamisch.GetAktiverSpieler();

            // Den feindseligsten KI-Amtsträger suchen (niedrigste Beziehung zum Spieler).
            int feindKi = 0;
            int minBeziehung = int.MaxValue;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                var ki = SW.Dynamisch.GetKIwithID(i);
                if (ki.GetAmtID() == 0)
                    continue;

                int bez = ki.GetBeziehungZuKIX(spielerId);
                if (bez < minBeziehung)
                {
                    minBeziehung = bez;
                    feindKi = i;
                }
            }

            if (feindKi == 0)
                return 0;

            // Feindseligkeit zählt erst unterhalb von „neutral"; Bosheit als kleiner Zuschlag.
            var feind = SW.Dynamisch.GetKIwithID(feindKi);
            int feindseligkeit = NeutraleBeziehung - minBeziehung;
            if (feindseligkeit < 0)
                feindseligkeit = 0;

            int chance = (feindseligkeit * KiBeleidigtGewichtBeziehung + feind.GetBosheit() * KiBeleidigtGewichtBosheit) / 100;

            return SW.Statisch.Rnd.Next(0, 100) < chance ? feindKi : 0;
        }

        // --- Duell ------------------------------------------------------------------------------------

        /// <summary>
        /// Trägt das Duell im Morgengrauen aus: bestimmt den Sieger (Fechtfähigkeit gegen die
        /// Gegnerstärke – bei KI aus ihrer Bosheit, bei Menschen deren Fechtfähigkeit – plus Zufall), zieht
        /// dem Verlierer Gesundheit ab und lässt ihn bei zu geringer Gesundheit sein Amt verlieren
        /// (Neuwahl). Beteiligt ist stets der aktive (menschliche) Spieler gegen <paramref name="zielId"/>.
        /// </summary>
        [PublicAPI]
        public DuellErgebnis FuehreDuellDurch(int zielId)
        {
            var herausforderer = SW.Dynamisch.GetAktHum();
            int herausfordererId = SW.Dynamisch.GetAktiverSpieler();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            string zielName = ziel.GetKompletterName();

            int gegnerStaerke = zielId >= SW.Statisch.GetMinKIID()
                ? 15 + SW.Dynamisch.GetKIwithID(zielId).GetBosheit() / 4
                : SW.Dynamisch.GetHumWithID(zielId).Fechtfaehigkeit;

            int chance = 50 + (herausforderer.Fechtfaehigkeit - gegnerStaerke);
            if (chance < 5) chance = 5;
            if (chance > 95) chance = 95;

            bool spielerGewinnt = SW.Statisch.Rnd.Next(0, 100) < chance;

            int verliererId = spielerGewinnt ? zielId : herausfordererId;
            var verlierer = SW.Dynamisch.GetSpWithID(verliererId);

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
                GegnerName = zielName,
                AmtName = amtVerloren ? amtName : "",
                Meldung = BaueMeldung(spielerGewinnt, zielName, verlierer, amtVerloren, amtName)
            };
        }

        private static string BaueMeldung(bool spielerGewinnt, string zielName, Spieler verlierer, bool amtVerloren, string amtName)
        {
            string text = "Im fahlen Licht des Morgengrauens tretet Ihr " + zielName + " zum Duell gegenüber. ";

            text += spielerGewinnt
                ? "Eure Klinge trifft – Ihr geht als Sieger hervor! " + verlierer.GetName() + " trägt eine schwere Verletzung davon."
                : "Doch die gegnerische Klinge ist überlegen. Ihr tragt eine schwere Verletzung davon.";

            if (amtVerloren)
            {
                text += spielerGewinnt
                    ? "\n\n" + verlierer.GetName() + " ist so schwer verletzt, dass das Amt als " + amtName + " niedergelegt werden muss – es wird neu besetzt."
                    : "\n\nEure Verletzung wiegt so schwer, dass Ihr Euer Amt als " + amtName + " niederlegen müsst.";
            }

            return text;
        }

        private static string GetAmtsstufenText(int amtId)
        {
            if (amtId == 0)
                return "";
            if (amtId < SW.Statisch.GetMaxAmtStadtID())
                return " bei den Amtsträgern der Stadtebene";
            if (amtId < SW.Statisch.GetMaxAmtLandID())
                return " bei den Amtsträgern der Landesebene";
            return " bei den Amtsträgern der Reichsebene";
        }
    }
}
