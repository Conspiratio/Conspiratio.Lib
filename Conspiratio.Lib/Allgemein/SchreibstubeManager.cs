using System;
using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Logik der Schreibstube aus dem alten WinForms-Client: Kredite beim Geldleiher
    /// aufnehmen, das Kreditbuch mit Tilgung sowie die Anzeige der Gesetze des Königreichs.
    /// </summary>
    public class SchreibstubeManager
    {
        #region Geldleiher und Kreditbuch

        /// <summary>
        /// Prüft, ob der aktive Spieler noch einen weiteren Kredit aufnehmen kann.
        /// </summary>
        [PublicAPI]
        public bool KannKreditNehmen()
        {
            return HatFreienKreditplatz() && GetVerbleibendeAbsagen() > 0;
        }

        /// <summary>
        /// Wie oft je Zug ein Angebot ausgeschlagen werden darf, bevor der Geldverleiher für dieses Jahr
        /// nicht mehr zu sprechen ist.
        ///
        /// <b>Warum es eine Grenze braucht.</b> Ablehnen kostete nichts und würfelte ein neues Angebot,
        /// Höhe und Zinssatz waren also beliebig oft nachziehbar: Wer es wusste, wartete einfach auf
        /// 1 200 Taler zu 10 %, und der Zufall im Angebot war keiner mehr. Mit drei Versuchen bleibt die
        /// Wahl echt, ohne dass ein ungünstiger erster Wurf den Zug entscheidet.
        /// </summary>
        public const int MaxKreditAbsagenProZug = 3;

        /// <summary>
        /// Kleinster aufnehmbarer Betrag. Darunter lohnt der Weg zum Geldverleiher nicht, und ein Kredit
        /// über wenige Taler belegte nur einen der vier Kreditplätze.
        /// </summary>
        public const int MindestKreditbetrag = 100;

        /// <summary>
        /// Zinsaufschlag in Prozentpunkten je Absage in diesem Zug – er hebt die <em>Untergrenze</em> des
        /// gewürfelten Satzes, nicht die Obergrenze. Ein günstiger Satz wird damit unwahrscheinlicher,
        /// ohne dass der schlechteste Fall schlechter würde: Nach drei Absagen liegt die Spanne bei
        /// 13 bis 20 statt 10 bis 20 Prozent, der Erwartungswert also bei 16,5 statt 15. Bewusst klein
        /// gehalten – die Absage soll etwas kosten, aber nicht bestrafen.
        /// </summary>
        public const int ZinsaufschlagJeAbsage = 1;

        /// <summary>Ist überhaupt noch einer der vier Kreditplätze frei?</summary>
        [PublicAPI]
        public bool HatFreienKreditplatz()
        {
            return SW.Dynamisch.GetAktHum().GetEmptyKreditID() != SW.Statisch.GetMaxKredite();
        }

        /// <summary>Wie viele Absagen bleiben dem Spieler in diesem Zug noch?</summary>
        [PublicAPI]
        public int GetVerbleibendeAbsagen()
        {
            return Math.Max(0, MaxKreditAbsagenProZug - SW.Dynamisch.GetAktHum().KreditAbsagenDiesenZug);
        }

        /// <summary>
        /// Vermerkt eine Absage. Danach ist das Angebot verfallen; das nächste wird neu gewürfelt und
        /// fällt tendenziell etwas teurer aus (<see cref="ZinsaufschlagJeAbsage"/>).
        /// </summary>
        [PublicAPI]
        public void LehneKreditAb()
        {
            SW.Dynamisch.GetAktHum().KreditAbsagenDiesenZug++;
        }

        /// <summary>
        /// Erstellt ein zufälliges Kreditangebot: Ein zufälliger KI-Spieler bietet 10 % seines
        /// Vermögens zu einem zufälligen Zinssatz an (halbiert mit Privileg 30), rückzahlbar in 4 bis 7 Jahren.
        /// </summary>
        [PublicAPI]
        public KreditAngebot ErstelleKreditAngebot()
        {
            int kiId = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());
            int summe = Convert.ToInt32(0.1 * SW.Dynamisch.GetKIwithID(kiId).GetTaler());

            // Jede Absage in diesem Zug hebt die Untergrenze des Wurfs, nicht die Obergrenze: Der
            // günstige Satz wird unwahrscheinlicher, der schlechteste Fall bleibt derselbe. Die
            // Untergrenze darf die Obergrenze nicht überholen, sonst wirft Rnd.Next eine Ausnahme.
            int zinsUntergrenze = Math.Min(SW.Statisch.GetKreditZinsMax(),
                                           SW.Statisch.GetKreditZinsMin() +
                                           SW.Dynamisch.GetAktHum().KreditAbsagenDiesenZug * ZinsaufschlagJeAbsage);

            int zins = SW.Statisch.Rnd.Next(zinsUntergrenze, SW.Statisch.GetKreditZinsMax() + 1);

            if (SW.Dynamisch.GetAktHum().CheckPrivilegX(30))
                zins = zins / 2;

            int jahre = SW.Statisch.Rnd.Next(4, 8);

            return new KreditAngebot(kiId, SW.Dynamisch.GetKIwithID(kiId).GetName(), summe, zins, jahre);
        }

        /// <summary>
        /// Nimmt das Kreditangebot in voller Höhe an. Bequemlichkeitsüberladung von
        /// <see cref="NimmKredit(KreditAngebot,int)"/>.
        /// </summary>
        [PublicAPI]
        public void NimmKredit(KreditAngebot angebot)
        {
            NimmKredit(angebot, angebot.Summe);
        }

        /// <summary>
        /// Nimmt vom Kreditangebot den gewünschten Teilbetrag auf: Der Spieler erhält ihn, der
        /// KI-Spieler gibt ihn her. Ist die Kreditaufnahme per Gesetz verboten, gibt es einen
        /// Deliktpunkt.
        ///
        /// <b>Warum der Betrag wählbar ist.</b> Die Angebotshöhe ist ein Zehntel des Vermögens eines
        /// <em>zufälligen</em> Gläubigers und hat mit dem Bedarf des Kreditnehmers nichts zu tun – zu
        /// Spielbeginn sind das 1 200 bis 12 000 Taler, während die einzige lohnende Investition eines
        /// Anfängers (ein auf den Stadtbedarf dimensionierter Werkstattplatz samt Lager) rund 2 600
        /// kostet. Gemessen wirft die 49 % im Jahr ab, der Kredit kostet 10 bis 20 – ein gutes Geschäft.
        /// Nur ließ sich die Summe bisher nicht teilen: Wer 12 000 annahm, um 2 600 zu investieren,
        /// zahlte auf die vollen 12 000 Zinsen, und der Rest lag ungenutzt herum. Bei 20 % waren das
        /// 2 400 Taler im Jahr gegen einen Betrieb, der 1 292 verdient – der Kredit allein verdoppelte
        /// den Verlust. Das war der Grund, aus dem sich Kredite im Frühspiel nie lohnten, nicht der
        /// Zinssatz.
        ///
        /// <paramref name="betrag"/> wird auf <see cref="MindestKreditbetrag"/> bis
        /// <see cref="KreditAngebot.Summe"/> geklemmt.
        /// </summary>
        [PublicAPI]
        public void NimmKredit(KreditAngebot angebot, int betrag)
        {
            betrag = Math.Max(MindestKreditbetrag, Math.Min(betrag, angebot.Summe));

            var spieler = SW.Dynamisch.GetAktHum();
            var kredit = spieler.GetKreditMitID(spieler.GetEmptyKreditID());

            kredit.SetDauer(angebot.Jahre);
            kredit.SetTaler(betrag);
            kredit.SetZinsen(angebot.Zins);
            kredit.SetKIID(angebot.KiId);
            // Rückzahlungsjahr fest verankern, damit es nicht mit dem fortschreitenden Jahr mitwandert.
            kredit.SetRueckzahlungsjahr(SW.Dynamisch.GetAktuellesJahr() + angebot.Jahre);

            spieler.ErhoeheTaler(betrag);
            SW.Dynamisch.GetKIwithID(angebot.KiId).ErhoeheTaler(-betrag);

            // Falls die Kreditaufnahme verboten ist...
            if (SW.Dynamisch.GetGesetzX(0) != 0)
                spieler.ErhoeheGesetzXUmEins(0);

            spieler.GetSpielerStatistik().SgenommeneKredite++;
        }

        /// <summary>
        /// Liefert alle offenen Kredite des aktiven Spielers für das Kreditbuch.
        /// </summary>
        [PublicAPI]
        public List<KreditInfo> GetOffeneKredite()
        {
            var kredite = new List<KreditInfo>();
            var spieler = SW.Dynamisch.GetAktHum();

            for (int i = 0; i < SW.Statisch.GetMaxKredite(); i++)
            {
                var kredit = spieler.GetKreditMitID(i);

                if (kredit.GetDauer() <= 0)
                    continue;

                var kiSpieler = SW.Dynamisch.GetKIwithID(kredit.GetKIID());
                string seineIhre = kiSpieler.GetMaennlich() ? "seine" : "ihre";

                int endjahr = kredit.GetRueckzahlungsjahr();

                // Alter Kredit aus der Zeit vor dieser Korrektur (kein festes Rückzahlungsjahr gespeichert):
                // einmalig aus der Restlaufzeit ableiten und festschreiben, damit es fortan stehen bleibt.
                if (endjahr <= 0)
                {
                    endjahr = kredit.GetDauer() + SW.Dynamisch.GetAktuellesJahr();
                    kredit.SetRueckzahlungsjahr(endjahr);
                }

                kredite.Add(new KreditInfo(i,
                    kiSpieler.GetKompletterName() + " fordert für " + seineIhre + " " + kredit.GetTaler().ToStringGeld() + " " +
                    kredit.GetZinsen() + "% Zinsen bis zum Jahr " + endjahr + "."));
            }

            return kredite;
        }

        /// <summary>
        /// Tilgt den Kredit vollständig (die Zinsen laufen separat über die Jahresabrechnung).
        /// </summary>
        /// <returns>False, wenn der Spieler nicht genügend Taler besitzt.</returns>
        [PublicAPI]
        public bool TilgeKredit(int kreditId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var kredit = spieler.GetKreditMitID(kreditId);

            if (spieler.GetTaler() < kredit.GetTaler())
                return false;

            spieler.ErhoeheTaler(-kredit.GetTaler());
            SW.Dynamisch.GetKIwithID(kredit.GetKIID()).ErhoeheTaler(kredit.GetTaler());

            kredit.SetDauer(0);
            kredit.SetTaler(0);
            kredit.SetKIID(0);
            kredit.SetZinsen(0);
            kredit.SetRueckzahlungsjahr(0);

            return true;
        }

        /// <summary>
        /// Tilgt am Rundenende alle Kredite des aktiven Spielers, deren Rückzahlungsjahr erreicht oder
        /// überschritten ist, zwangsweise – notfalls rutscht das Vermögen dabei ins Minus. Je getilgtem
        /// Kredit wird eine Hinweismeldung zurückgeliefert.
        /// </summary>
        [PublicAPI]
        public List<string> TilgeUeberfaelligeKredite()
        {
            var meldungen = new List<string>();
            var spieler = SW.Dynamisch.GetAktHum();
            int jahr = SW.Dynamisch.GetAktuellesJahr();

            for (int i = 0; i < SW.Statisch.GetMaxKredite(); i++)
            {
                var kredit = spieler.GetKreditMitID(i);

                if (kredit.GetDauer() <= 0)
                    continue;

                int endjahr = kredit.GetRueckzahlungsjahr();

                // Alter Kredit ohne festes Rückzahlungsjahr: einmalig aus der Restlaufzeit festschreiben.
                if (endjahr <= 0)
                {
                    endjahr = kredit.GetDauer() + jahr;
                    kredit.SetRueckzahlungsjahr(endjahr);
                }

                if (jahr < endjahr)
                    continue;

                int betrag = kredit.GetTaler();
                var glaeubiger = SW.Dynamisch.GetKIwithID(kredit.GetKIID());
                string name = glaeubiger.GetKompletterName();

                spieler.ErhoeheTaler(-betrag);   // notfalls ins Minus
                glaeubiger.ErhoeheTaler(betrag);

                kredit.SetDauer(0);
                kredit.SetTaler(0);
                kredit.SetKIID(0);
                kredit.SetZinsen(0);
                kredit.SetRueckzahlungsjahr(0);

                meldungen.Add("Euer Kredit über " + betrag.ToStringGeld() + " bei " + name +
                              " ist fällig geworden und wurde automatisch getilgt." +
                              (spieler.GetTaler() < 0 ? "\nEuer Vermögen ist dadurch ins Minus gerutscht." : ""));
            }

            return meldungen;
        }

        #endregion

        #region Gesetze

        /// <summary>
        /// Liefert die Überschrift einer Gesetzesebene inklusive der Strenge-Bewertung,
        /// z. B. "Finanzgesetze: locker" (Ebene 0 = Finanzen, 1 = Straf, 2 = Kirche).
        /// </summary>
        [PublicAPI]
        public string GetGesetzesEbenenUeberschrift(int ebene)
        {
            int strenge = 0;
            string ueberschrift = "";

            if (ebene == 0)
            {
                // Zuerst von den "An-Aus-Gesetzen" die addieren, die an sind
                strenge = SW.Dynamisch.GetGesetzX(0) + SW.Dynamisch.GetGesetzX(1) + SW.Dynamisch.GetGesetzX(4);

                // Dann die Gesetze mit einem Wert prüfen, ob dieser über oder unter dem Mittelwert liegt
                int gesetz2GrenzeFuerLocker = (SW.Statisch.GetGesetzXDefUntergrenze(2) + SW.Statisch.GetGesetzXDefObergrenze(2)) / 2;
                int gesetz3GrenzeFuerLocker = (SW.Statisch.GetGesetzXDefUntergrenze(3) + SW.Statisch.GetGesetzXDefObergrenze(3)) / 2;

                if (SW.Dynamisch.GetGesetzX(2) <= gesetz2GrenzeFuerLocker)
                    strenge++;

                if (SW.Dynamisch.GetGesetzX(3) <= gesetz3GrenzeFuerLocker)
                    strenge++;

                ueberschrift = "Finanzgesetze: ";
            }
            else if (ebene == 1)
            {
                strenge = SW.Dynamisch.GetGesetzX(20) + SW.Dynamisch.GetGesetzX(21) + SW.Dynamisch.GetGesetzX(22) + SW.Dynamisch.GetGesetzX(23) + SW.Dynamisch.GetGesetzX(24);
                ueberschrift = "Strafgesetze: ";
            }
            else if (ebene == 2)
            {
                strenge = SW.Dynamisch.GetGesetzX(40) + SW.Dynamisch.GetGesetzX(41) + SW.Dynamisch.GetGesetzX(42) + SW.Dynamisch.GetGesetzX(43) + SW.Dynamisch.GetGesetzX(44);
                ueberschrift = "Kirchengesetze: ";
            }

            if (strenge < 2)
                return ueberschrift + "sehr locker";

            if (strenge < 3)
                return ueberschrift + "locker";

            if (strenge < 4)
                return ueberschrift + "neutral";

            if (strenge < 5)
                return ueberschrift + "repressiv";

            return ueberschrift + "sehr repressiv";
        }

        /// <summary>
        /// Liefert die zehn Gesetzestexte einer Ebene (0 = Finanzen, 1 = Straf, 2 = Kirche).
        /// </summary>
        [PublicAPI]
        public List<string> GetGesetzesTexte(int ebene)
        {
            var texte = new List<string>();

            for (int i = 0; i < 10; i++)
                texte.Add(SW.Dynamisch.GetGesetzXinText(i + ebene * 20));

            return texte;
        }

        #endregion

        /// <summary>
        /// Prüft, ob es freie Ämter gibt, auf die sich der aktive Spieler bewerben kann.
        /// </summary>
        [PublicAPI]
        public bool GibtEsFreieAemter()
        {
            return SW.Dynamisch.GetAnzahlFreieAemterFuerSpX(SW.Dynamisch.GetAktiverSpieler()) > 0;
        }
    }

    /// <summary>
    /// Ein Kreditangebot eines KI-Geldverleihers.
    /// </summary>
    public class KreditAngebot
    {
        public KreditAngebot(int kiId, string anbieterName, int summe, int zins, int jahre)
        {
            KiId = kiId;
            AnbieterName = anbieterName;
            Summe = summe;
            Zins = zins;
            Jahre = jahre;
        }

        public int KiId { get; }

        public string AnbieterName { get; }

        public int Summe { get; }

        public int Zins { get; }

        public int Jahre { get; }

        /// <summary>
        /// Der Angebotstext wie im Original-Geldleiher.
        /// </summary>
        public string GetAngebotsText()
        {
            return AnbieterName + " bietet Euch " + Summe.ToStringGeld() + " zu " + Zins + "% Zinsen jährlich, rückzahlbar bis zum Jahre " +
                   (SW.Dynamisch.GetAktuellesJahr() + Jahre) + ". Wollt Ihr annehmen?";
        }

        /// <summary>Das Jahr, in dem die Summe in einem Betrag fällig wird.</summary>
        public int Rueckzahlungsjahr => SW.Dynamisch.GetAktuellesJahr() + Jahre;

        /// <summary>
        /// Der Angebotstext ohne die Frage am Ende – für die Ansicht, in der der Betrag erst noch
        /// gewählt wird und die Frage deshalb an den Knöpfen hängt.
        /// </summary>
        public string GetAngebotsKopf()
        {
            return AnbieterName + " streckt Euch bis zu " + Summe.ToStringGeld() + " vor,\n" +
                   "zu " + Zins + " % Zinsen jährlich, fällig im Jahre " + Rueckzahlungsjahr + ".";
        }

        /// <summary>
        /// Was der gewählte Betrag kostet: Zinsen im Jahr, Zinsen insgesamt und die Summe, die am Ende
        /// in einem Betrag zurückzuzahlen ist. Die Rückzahlung wird erzwungen, notfalls ins Minus –
        /// deshalb steht sie hier neben den Zinsen und nicht im Kleingedruckten.
        /// </summary>
        public string GetKostenText(int betrag)
        {
            int zinsenJeJahr = betrag * Zins / 100;

            return "Zinsen: " + zinsenJeJahr.ToStringGeld() + " im Jahr, " +
                   (zinsenJeJahr * Jahre).ToStringGeld() + " über " + Jahre + " Jahre.\n" +
                   "Im Jahre " + Rueckzahlungsjahr + " werden " + betrag.ToStringGeld() + " in einem Betrag fällig.";
        }
    }

    /// <summary>
    /// Ein offener Kredit im Kreditbuch.
    /// </summary>
    public class KreditInfo
    {
        public KreditInfo(int kreditId, string beschreibung)
        {
            KreditId = kreditId;
            Beschreibung = beschreibung;
        }

        public int KreditId { get; }

        public string Beschreibung { get; }
    }
}
