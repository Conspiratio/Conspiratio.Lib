using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Wickelt die beantragten Amtsenthebungen am Jahresende ab (Migration von
    /// Main.AmtsenthebungenDurchfuehren aus dem WinForms-Client).
    ///
    /// Einen Antrag stellen kann ein menschlicher Spieler über seine Untergebenen oder eine KI, der ein
    /// Untergebener zu unsympathisch geworden ist (<see cref="DynamischeSpieldaten.KIAktionenDurchfuehren"/>).
    /// Über den Antrag stimmen die bis zu drei Wähler des betroffenen Amtes ab; eine einfache Mehrheit
    /// setzt das Opfer mit sofortiger Wirkung ab, wodurch das Amt frei wird und im Folgejahr zur Wahl steht.
    ///
    /// Die Abstimmung läuft in drei Schritten, damit die Ansicht menschliche Wähler dazwischen befragen kann:
    /// <see cref="ErmittleVerfahren"/> liefert die offenen Verfahren, die Ansicht setzt
    /// <see cref="AmtsenthebungsWaehler.StimmtDafuer"/> für Menschen und ruft <see cref="ErgaenzeKiStimmen"/>
    /// für den Rest, <see cref="WerteAus"/> zählt aus und vollzieht das Ergebnis.
    /// </summary>
    public class AmtsenthebungsManager
    {
        /// <summary>
        /// Bis zu welcher Sympathie eine KI einer Absetzung zustimmt. Bewusst großzügiger als
        /// <c>GetMaxAbsetzSympathie()</c>, ab der eine KI eine Absetzung überhaupt erst *beantragt*:
        /// Mitmachen ist leichter als selbst den Antrag zu stellen (so auch im Original).
        /// </summary>
        private const int SympathiegrenzeZustimmung = 30;

        /// <summary>
        /// Liefert die offenen Amtsenthebungsverfahren. Anträge gegen jemanden, der sein Amt inzwischen
        /// ohnehin verloren hat, werden dabei stillschweigend verworfen.
        /// </summary>
        [PublicAPI]
        public List<AmtsenthebungsVerfahren> ErmittleVerfahren()
        {
            var verfahren = new List<AmtsenthebungsVerfahren>();

            for (int slot = 1; slot < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); slot++)
            {
                var antrag = SW.Dynamisch.GetAmtsenthebungX(slot);

                if (antrag.OpferID == 0)
                    continue;

                // Das Opfer hat sein Amt zwischenzeitlich verloren (Tod, Wahl, andere Enthebung) –
                // der Antrag ist gegenstandslos.
                if (SW.Dynamisch.GetSpWithID(antrag.OpferID).GetAmtID() == 0)
                {
                    LeereSlot(slot);
                    continue;
                }

                var waehler = new List<AmtsenthebungsWaehler>();

                // Die Wählerplätze sind im Original sparsam belegt und mussten dort erst nach vorne
                // geschoben werden; hier fallen leere Plätze einfach aus der Liste.
                foreach (int waehlerId in antrag.GetWaehler())
                {
                    if (waehlerId == 0)
                        continue;

                    waehler.Add(new AmtsenthebungsWaehler(
                        waehlerId,
                        SW.Dynamisch.GetSpWithID(waehlerId).GetKompletterName(),
                        istMensch: waehlerId < SW.Statisch.GetMinKIID()));
                }

                // Ohne Wähler kann nicht abgestimmt werden; der Antrag verfällt.
                if (waehler.Count == 0)
                {
                    LeereSlot(slot);
                    continue;
                }

                var opfer = SW.Dynamisch.GetSpWithID(antrag.OpferID);

                verfahren.Add(new AmtsenthebungsVerfahren(slot, antrag.OpferID, opfer.GetKompletterName(),
                                                          opfer.GetName(), opfer.GetSeinIhr(), waehler,
                                                          antrag.AntragstellerID,
                                                          ErmittleAntragstellerName(antrag.AntragstellerID)));
            }

            return verfahren;
        }

        /// <summary>
        /// Lässt die noch unentschiedenen Wähler abstimmen: Eine KI, die das Opfer nicht leiden kann, stimmt
        /// für die Absetzung. Bereits abgegebene Stimmen bleiben unangetastet – sowohl die eines menschlichen
        /// Wählers als auch eine, die dieser Aufruf zuvor schon gesetzt hat.
        /// </summary>
        [PublicAPI]
        public void ErgaenzeKiStimmen(AmtsenthebungsVerfahren verfahren)
        {
            foreach (var waehler in verfahren.Waehler)
            {
                if (waehler.HatGestimmt)
                    continue;

                // Ein menschlicher Wähler, den die Ansicht nicht gefragt hat, enthält sich.
                bool dafuer = !waehler.IstMensch &&
                              SW.Dynamisch.GetKIwithID(waehler.SpielerId).GetBeziehungZuKIX(verfahren.OpferId) < SympathiegrenzeZustimmung;

                waehler.StimmtDafuer = dafuer;
            }
        }

        /// <summary>
        /// Zählt die Stimmen aus, vollzieht bei einer Mehrheit die Absetzung und schließt das Verfahren ab.
        /// Die KI-Stimmen werden dabei ergänzt, falls die Ansicht das nicht bereits getan hat.
        /// </summary>
        [PublicAPI]
        public AmtsenthebungsErgebnis WerteAus(AmtsenthebungsVerfahren verfahren)
        {
            ErgaenzeKiStimmen(verfahren);

            int dafuer = 0;

            foreach (var waehler in verfahren.Waehler)
                if (waehler.StimmtDafuer)
                    dafuer++;

            // Einfache Mehrheit wie im Original: mehr als die Hälfte der abgegebenen Stimmen
            // (bei ganzzahliger Teilung, d. h. 1 von 1, 2 von 2, 2 von 3).
            bool abgesetzt = dafuer > verfahren.Waehler.Count / 2;

            if (abgesetzt)
                SW.Dynamisch.AmtVonXfreigeben(verfahren.OpferId);

            LeereSlot(verfahren.SlotId);

            string meldung = abgesetzt
                ? "Damit wurde " + verfahren.OpferKurzname + " mit sofortiger Wirkung des Amtes enthoben!"
                : "Damit behält " + verfahren.OpferName + " " + verfahren.OpferSeinIhr + " Amt bei.";

            return new AmtsenthebungsErgebnis(abgesetzt, meldung);
        }

        /// <summary>
        /// Der Name des Antragstellers für die Anzeige. Die 0 steht für „ohne benennbaren Antragsteller“ –
        /// so kommt das Feld sowohl aus Spielständen, die vor seiner Einführung entstanden sind, als auch
        /// aus einem gerichtlich angesetzten Verfahren, hinter dem keine Person steht.
        /// </summary>
        private static string ErmittleAntragstellerName(int antragstellerId)
        {
            if (antragstellerId == 0)
                return "";

            return SW.Dynamisch.GetSpWithID(antragstellerId)?.GetKompletterName() ?? "";
        }

        private static void LeereSlot(int slot)
        {
            SW.Dynamisch.SetAmtsenthebungDaten(slot, 0, 0, 0, 0);
        }
    }

    /// <summary>Ein offenes Amtsenthebungsverfahren samt seiner Wähler.</summary>
    public class AmtsenthebungsVerfahren
    {
        public AmtsenthebungsVerfahren(int slotId, int opferId, string opferName, string opferKurzname,
                                       string opferSeinIhr, List<AmtsenthebungsWaehler> waehler,
                                       int antragstellerId = 0, string antragstellerName = "")
        {
            SlotId = slotId;
            OpferId = opferId;
            OpferName = opferName;
            OpferKurzname = opferKurzname;
            OpferSeinIhr = opferSeinIhr;
            Waehler = waehler;
            AntragstellerId = antragstellerId;
            AntragstellerName = antragstellerName;
        }

        /// <summary>Platz im Amtsenthebungs-Array; wird beim Auswerten geleert.</summary>
        public int SlotId { get; }

        public int OpferId { get; }

        /// <summary>Voller Name mit Titel und Amt, für die Überschrift.</summary>
        public string OpferName { get; }

        /// <summary>Rufname ohne Titel, für die Ergebnismeldung.</summary>
        public string OpferKurzname { get; }

        /// <summary>"sein" oder "ihr", passend zum Opfer.</summary>
        public string OpferSeinIhr { get; }

        /// <summary>Die tatsächlich vorhandenen Wähler (ein bis drei), ohne Leerstellen.</summary>
        public List<AmtsenthebungsWaehler> Waehler { get; }

        /// <summary>Wer den Antrag gestellt hat; 0, wenn niemand Bestimmtes dahintersteht.</summary>
        public int AntragstellerId { get; }

        /// <summary>Name des Antragstellers, leer bei <see cref="AntragstellerId"/> 0.</summary>
        public string AntragstellerName { get; }

        /// <summary>
        /// Die Eröffnungszeile des Verfahrens. Nennt den Antragsteller, sofern bekannt – ohne ihn weiß
        /// das Opfer nicht, gegen wen es sich wehren müsste.
        /// </summary>
        public string GetAntragText()
        {
            if (string.IsNullOrEmpty(AntragstellerName))
                return "Eine Absetzung von " + OpferName + " wurde beantragt.";

            return AntragstellerName + " hat die Absetzung von " + OpferName + " beantragt.";
        }

        /// <summary>
        /// Ist ein menschlicher Spieler beteiligt (als Opfer oder als Wähler)? Nur dann zeigt der Client
        /// das Verfahren; rein unter KIs läuft es unsichtbar ab.
        /// </summary>
        public bool MenschlichBeteiligt
        {
            get
            {
                if (OpferId < SW.Statisch.GetMinKIID())
                    return true;

                foreach (var waehler in Waehler)
                    if (waehler.IstMensch)
                        return true;

                return false;
            }
        }
    }

    /// <summary>Ein Wähler in einem Amtsenthebungsverfahren und seine Stimme.</summary>
    public class AmtsenthebungsWaehler
    {
        public AmtsenthebungsWaehler(int spielerId, string name, bool istMensch)
        {
            SpielerId = spielerId;
            Name = name;
            IstMensch = istMensch;
        }

        public int SpielerId { get; }

        public string Name { get; }

        public bool IstMensch { get; }

        private bool _stimmtDafuer;

        /// <summary>
        /// Stimmt dieser Wähler für die Absetzung? Bei Menschen von der Ansicht gesetzt, bei KIs von
        /// <see cref="AmtsenthebungsManager.ErgaenzeKiStimmen"/>. Das Setzen gilt als Stimmabgabe und ist
        /// endgültig – ein späteres Ergänzen der KI-Stimmen überschreibt sie nicht mehr.
        /// </summary>
        public bool StimmtDafuer
        {
            get => _stimmtDafuer;
            set
            {
                _stimmtDafuer = value;
                HatGestimmt = true;
            }
        }

        /// <summary>Wurde für diesen Wähler bereits eine Stimme abgegeben?</summary>
        public bool HatGestimmt { get; private set; }
    }

    /// <summary>Ausgang eines Amtsenthebungsverfahrens.</summary>
    public class AmtsenthebungsErgebnis
    {
        public AmtsenthebungsErgebnis(bool abgesetzt, string meldung)
        {
            Abgesetzt = abgesetzt;
            Meldung = meldung;
        }

        public bool Abgesetzt { get; }

        public string Meldung { get; }
    }
}
