using System;
using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    /// <summary>
    /// Kapselt die jährlichen Zufallsereignisse eines Spielerzugs (Extraktion aus dem WinForms-Client,
    /// Methode RandomEreignisse): je ein zufälliges Finanz-, Ansehens- und Gesundheitsereignis (mit
    /// Auswirkung auf Taler, Ansehen bzw. Gesundheit) sowie ein datengesteuertes Datumsereignis. Die
    /// Auswirkungen werden angewendet und die anzuzeigenden Meldungen der Reihe nach zurückgegeben.
    /// </summary>
    public class ZufallsereignisseManager
    {
        /// <summary>Eine einzelne anzuzeigende Zufallsereignis-Meldung (Überschrift und Text).</summary>
        public class Ereignismeldung
        {
            public Ereignismeldung(string ueberschrift, string text)
            {
                Ueberschrift = ueberschrift;
                Text = text;
            }

            public string Ueberschrift { get; }

            public string Text { get; }
        }

        /// <summary>
        /// Ermittelt die diesjährigen Zufallsereignisse, wendet ihre Auswirkungen an und liefert die
        /// anzuzeigenden Meldungen. Im Startjahr passiert nichts (leere Liste).
        /// </summary>
        public List<Ereignismeldung> ErmittleEreignisse()
        {
            var meldungen = new List<Ereignismeldung>();

            if (SW.Dynamisch.GetAktuellesJahr() == SW.Statisch.StartJahr)
                return meldungen;

            var spieler = SW.Dynamisch.GetAktHum();
            bool maennlich = spieler.GetMaennlich();

            int val1 = SW.Statisch.Rnd.Next(0, 100); // Vermögen
            int val2 = SW.Statisch.Rnd.Next(0, 100); // Ansehen
            int val3 = SW.Statisch.Rnd.Next(0, 100); // Gesundheit
            int wert1 = SW.Statisch.Rnd.Next(10, 31);
            int wert2 = SW.Statisch.Rnd.Next(1, 11);
            int wert3 = SW.Statisch.Rnd.Next(1, 11);

            int gesamtVermoegen = spieler.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler());

            // Bei fehlendem Vermögen (oder Schulden) wird vom Startkapital ausgegangen.
            if (gesamtVermoegen <= 0)
                gesamtVermoegen = SW.Statisch.GetStartgold();

            int vwert = wert1 * gesamtVermoegen / 700;

            // --- Vermögen ---
            // Der Betrag wird vorab angewendet; bei einem Nicht-Ereignis (Default) wieder ausgeglichen.
            spieler.ErhoeheTaler(val1 < 50 ? vwert : -vwert);

            string finanzText = VermoegensText(val1, vwert, maennlich);

            if (finanzText == null)
                spieler.ErhoeheTaler(val1 < 50 ? -vwert : vwert);
            else
                meldungen.Add(new Ereignismeldung("Finanziell", finanzText));

            // --- Ansehen ---
            spieler.ErhoehePermaAnsehen(val2 < 50 ? wert2 : -wert2);

            string ansehenText;

            if (val2 == 2)
            {
                // Sonderfall: das Ereignis kostet zusätzlich Geld (halbierter Finanzwert) und erhöht das Ansehen stärker.
                vwert /= 2;
                spieler.ErhoeheTaler(-vwert);
                spieler.ErhoehePermaAnsehen(wert2 / 2);

                ansehenText = maennlich
                    ? $"Ihr kauft Euch einen eleganten Gehstock für {vwert.ToStringGeld()}."
                    : $"Ihr kauft Euch einen eleganten, tragbaren Sonnenschirm für {vwert.ToStringGeld()}.";
            }
            else
            {
                ansehenText = AnsehensText(val2, maennlich);
            }

            if (ansehenText == null)
                spieler.ErhoehePermaAnsehen(val2 < 50 ? -wert2 : wert2);
            else
                meldungen.Add(new Ereignismeldung("Ansehen", ansehenText));

            // --- Gesundheit ---
            spieler.ErhoeheGesundheit(val3 < 50 ? wert3 : -wert3);

            string gesundheitText = GesundheitsText(val3);

            if (gesundheitText == null)
                spieler.ErhoeheGesundheit(val3 < 50 ? -wert3 : wert3);
            else
                meldungen.Add(new Ereignismeldung("Gesundheit", gesundheitText));

            // --- Datumsereignis ---
            var datumsMeldung = ErmittleDatumsereignis(spieler, vwert, wert2, wert3);

            if (datumsMeldung != null)
                meldungen.Add(datumsMeldung);

            return meldungen;
        }

        /// <summary>
        /// Wählt aus den aktuell gültigen Datumsereignissen zufällig eines aus, wendet seine Multiplikatoren
        /// auf Taler, Ansehen und Gesundheit an, merkt es sich beim Spieler und liefert die Meldung.
        /// </summary>
        private static Ereignismeldung ErmittleDatumsereignis(HumSpieler spieler, int vwert, int wert2, int wert3)
        {
            var gueltigeEreignisse = SW.Statisch.Datumsereignisse
                .Where(e => e.IstEreignisGueltig(spieler.GetReligion(), spieler.EreignisseZuletztPassiert))
                .ToList();

            if (gueltigeEreignisse.Count == 0)
                return null;

            var ereignis = gueltigeEreignisse[SW.Statisch.Rnd.Next(0, gueltigeEreignisse.Count)];

            spieler.ErhoeheTaler(vwert * ereignis.TalerMultiplikator);
            spieler.ErhoehePermaAnsehen(wert2 * ereignis.AnsehenMultiplikator);
            spieler.ErhoeheGesundheit(wert3 * ereignis.GesundheitMultiplikator);

            // Zeitpunkt des letzten Auftretens beim Spieler festhalten.
            List<Ereigniszeitpunkt> zuletzt = spieler.EreignisseZuletztPassiert;

            if (zuletzt == null)
            {
                zuletzt = new List<Ereigniszeitpunkt>();
                spieler.EreignisseZuletztPassiert = zuletzt;
            }

            int index = zuletzt.FindIndex(z => z.EreignisID == ereignis.ID);

            if (index != -1)
                zuletzt[index].Zeitpunkt = DateTime.Now;
            else
                zuletzt.Add(new Ereigniszeitpunkt { EreignisID = ereignis.ID, Zeitpunkt = DateTime.Now });

            string nachricht = ereignis.Nachricht;

            if (ereignis.TalerMultiplikator != 0)
                nachricht = string.Format(nachricht, Math.Abs(vwert * ereignis.TalerMultiplikator).ToStringGeld());

            return new Ereignismeldung(ereignis.Ueberschrift, nachricht);
        }

        /// <summary>Liefert den Text des Finanzereignisses zu <paramref name="val1"/> oder null (kein Ereignis).</summary>
        private static string VermoegensText(int val1, int vwert, bool maennlich)
        {
            switch (val1)
            {
                case 0: return "Bei Silberspekulationen gewinnt Ihr " + vwert.ToStringGeld() + ".";
                case 1: return "Ein Cousin 3. Grades hinterlässt Euch " + vwert.ToStringGeld() + ".";
                case 2: return "Ihr findet einen Geldbeutel mit " + vwert.ToStringGeld() + ". Euer mangelnder Gerechtigkeitssinn lässt Euch das Geld behalten.";
                case 3: return "Bei einer Wette gewinnt Ihr " + vwert.ToStringGeld() + ".";
                case 4: return "Ihr investiert " + (vwert / 2).ToStringGeld() + " in einen gerissenen Unternehmer. Kurz darauf zahlt er Euch das Doppelte zurück.";
                case 5: return "Ein Geldbeutel mit " + vwert.ToStringGeld() + " Eures Nachbarn wurde fälschlicherweise Euch zugeschickt.";
                case 6: return "Ein Euch Unbekannter steckt Euch " + vwert.ToStringGeld() + " zu damit Ihr ein Gerücht verbreitet. Guten Gewissens kommt Ihr diesem Angebot nach.";
                case 7: return "Euer Großonkel greift Euren Unternehmungen mit " + vwert.ToStringGeld() + " unter die Arme.";
                case 8: return "Ihr verkauft eine alte Erzmine für " + vwert.ToStringGeld() + ". Kurz darauf ist die Ader erschöpft.";
                case 9: return "Morgens beim Gassi gehn mit Eurem Hund Struppi verschwindet dieser. Nach einiger Sucherei taucht Struppi wieder auf. Im Maul hat er einen Knochen um den ein verdreckter goldener Armreif hängt. Später könnt Ihr den Armreif für " + vwert.ToStringGeld() + " verkaufen.";
                case 10: return "Neben Euch stiehlt ein gemeiner Dieb einer alten Dame den Geldbeutel. Beim Davonlaufen stößt der Dieb mit Euch zusammen und lässt dadurch unwissentlich den Geldbeutel fallen. Schnell hebt Ihr den Geldbeutel mit " + vwert.ToStringGeld() + " auf. Euer kaum ausgeprägter Gerechtigkeitssinn lässt Euch das Geld behalten.";
                case 11: return "Aus früheren Kirchensteuern bekommt Ihr " + vwert.ToStringGeld() + " zurück.";
                case 12: return "Ein Unbekannter lässt Euch " + vwert.ToStringGeld() + " zukommen.";
                case 13: return "In Eurem Keller findet Ihr ein altes Gemälde. Als Ihr es bei einer Gala ausstellen lasst, kauft es jemand für " + vwert.ToStringGeld() + ".";
                case 14: return "Ihr bekommt von den Schmuggelgeschäften eines erfolgreichen Kaufmannes Wind. Darauf lässt er Euch " + vwert.ToStringGeld() + " als Schweigegeld zukommen...";
                case 15: return "Ihr nehmt an einer Tombola teil und gewinnt " + vwert.ToStringGeld() + "!";

                case 50: return "Bei Erzspekulationen verliert Ihr " + vwert.ToStringGeld() + ".";
                case 51: return "Ihr greift einem Verwandten mit " + vwert.ToStringGeld() + " unter die Arme.";
                case 52: return "Auf dem Marktplatz rempelt Euch ein kleiner Junge beim Fangen spielen mit seinen Freunden an. Später stellt Ihr entsetzt fest, dass er dabei Euren Geldbeutel mit " + vwert.ToStringGeld() + " entwendet hat.";
                case 53: return "Bei einer Wette verliert Ihr " + vwert.ToStringGeld() + ".";
                case 54: return "Ihr investiert " + vwert.ToStringGeld() + " in einen sympathischen Unternehmer. Er verschwindet spurlos.";
                case 55: return "Nach einem Kneipenabend werdet Ihr beim Singen von unanständigen Liedern ertappt und müsst " + vwert.ToStringGeld() + " bezahlen, um die Sache unter den Tisch zu kehren.";
                case 56: return "Ihr spendet " + vwert.ToStringGeld() + " für die Rettung eines Waisenhaus. In der Dankesrede werdet Ihr nicht einmal erwähnt.";
                case 57: return "Ihr verkauft einen Teil Eures Schmucks. Der Käufer ist bereits verschwunden als Ihr merkt, dass sich in dem überreichten Geldbeutel inzwischen nur noch Steine befinden. Ihr verliert insgesamt " + vwert.ToStringGeld() + ".";
                case 58: return "Ein alter Freund aus Kindertagen leiht sich " + vwert.ToStringGeld() + " von Euch. Darauf hört Ihr nie wieder von ihm...";
                case 59: return maennlich
                    ? "Eine junge, hübsche Dame macht Euch schöne Augen. Schon kauft Ihr ihr ein Diamantarmband für " + vwert.ToStringGeld() + ". Sie bedankt sich und geht ihres Weges."
                    : "Ein hübscher Jüngling macht Euch schöne Augen. Schon kauft Ihr ihm ein Rennpferd für " + vwert.ToStringGeld() + ". Er bedankt sich und geht seines Weges.";
                case 60: return maennlich
                    ? "Eine Hellseherin prophezeit Euch großen Reichtum. Als Bezahlung verlangt sie " + vwert.ToStringGeld() + ". Diese Summe sollte laut ihr in Zukunft für einen Mann wie Euch nur eine Kleinigkeit sein."
                    : "Eine Hellseherin prophezeit Euch großen Reichtum. Als Bezahlung verlangt sie " + vwert.ToStringGeld() + ". Diese Summe sollte laut ihr in Zukunft für eine Frau wie Euch nur eine Kleinigkeit sein.";
                case 61: return "Ihr müsst " + vwert.ToStringGeld() + " für Kirchensteuern nachbezahlen.";
                case 62: return "Für " + vwert.ToStringGeld() + " kauft Ihr einem merkwürdigem Mann ein Fläschchen mit magischem Wasser ab. Ihr trinkt es, doch nichts passiert.";
                case 63: return "Für " + vwert.ToStringGeld() + " finanziert Ihr die Ausbildung von Waisenkindern.";
                case 64: return "Bei einem Fest zerstört Ihr eine wertvolle Vase im Wert von " + vwert.ToStringGeld() + ".";
                case 65: return "Ihr steuert " + vwert.ToStringGeld() + " für die Mitgift eines entfernten Verwandten bei...";

                default: return null;
            }
        }

        /// <summary>Liefert den Text des Ansehensereignisses zu <paramref name="val2"/> oder null (kein Ereignis). Fall 2 wird gesondert behandelt.</summary>
        private static string AnsehensText(int val2, bool maennlich)
        {
            switch (val2)
            {
                case 0: return "Ihr entschließt Euch, von nun an öfter einen Kamm zu benutzen.";
                case 1: return maennlich
                    ? "Ihr entschließt Euch, öfters eine modische Schecke zu tragen."
                    : "Ihr entschließt Euch, öfters eine modische Robe zu tragen.";
                case 3: return "Bei jedem Treffen mit Euren Geschäftspartnern seid Ihr bereits 5 Minuten vor dem vereinbarten Termin vor Ort.";
                case 4: return "Euer hoher Vater lehrte Euch einst: Laut sprechen, damit die anderen glauben, Ihr hättet eine Ahnung wovon Ihr redet. Nun wendet Ihr diese Devise erfolgreich bei jedem Gespräch an.";
                case 5: return "Ein namhafter Schneider fertigt Euch Kleidung an.";
                case 6: return "Brust raus! Bauch rein! An Eurer Haltung können sich viele ein Beispiel nehmen.";
                case 7: return maennlich
                    ? "Inspiriert von einem Gemälde eines toten Marschalls, lasst Ihr Euch einen prächtigen Backenbart wachsen."
                    : "Inspiriert von einem Gemälde einer französischen Adeligen, legt Ihr Euch einen aufgeklebten, künstlichen Schönheitsfleck zu.";
                case 8: return maennlich
                    ? "Jeden Morgen veranstaltet Ihr eine Raserei der Rasierklingen. Nur mit einem glatt rasiertem Kinn verlasst Ihr das Haus."
                    : "Jeden Morgen veranstaltet Ihr eine Puderexplosion. Nur mit bleich gepudertem Gesicht verlasst Ihr das Haus.";
                case 9: return "Das Lesen einer prätentiösen Lektüre hat Euren Wortschatz verbessert. Unabhängig vom Thema drückt Ihr Euch wie ein Kenner aus.";
                case 10: return maennlich
                    ? "Bei hitzigen Diskussionen seid Ihr immer derjenige, der einen kühlen Kopf bewahrt."
                    : "Bei hitzigen Diskussionen seid Ihr immer diejenige, die einen kühlen Kopf bewahrt.";
                case 11: return maennlich
                    ? "Ihr kopiert die Gangweise eines galanten Herren."
                    : "Ihr kopiert die Gangweise einer galanten Dame.";
                case 12: return "Ihr arbeitet an Euren Tischmanieren...";
                case 13: return "Eines Morgens bemerkt Ihr Euer langes Nasenhaar im Spiegel. Prompt entledigt Ihr Euch dessen...";
                case 14: return "Jedem Gesprächspartner blickt Ihr offen und ehrlich in die Augen... So als ob Ihr ein guter Mensch wärt.";
                case 15: return "Bei jeder Unterhaltung habt Ihr stets das letzte Wort.";

                case 50: return maennlich
                    ? "Ihr entschließt Euch, von nun an eine Glatze zu tragen. Die Gassenjungen kichern..."
                    : "Ihr entschließt Euch, von nun an auf Eure Perücke zu verzichten. Die Gassenjungen kichern...";
                case 51: return "Ihr entschließt Euch, von nun an öfter Eure alten, ausgeleierten Glücksstiefel zu tragen.";
                case 52: return "Aufgrund einer Beinverletzung gewöhnt Ihr Euch das Hinken an.";
                case 53: return "Ihr seid spät dran für einen wichtigen Geschäftstermin und könnt keine Droschke finden, also Ihr beschließt den langen Weg zu laufen. Völlig durchgeschwitzt erscheint Ihr gerade noch rechtzeitig zum Treffen.";
                case 54: return "Am Marktplatz herumspazierend erinnert Ihr Euch an Eure Kindheitstage. Dabei beginnt Ihr Selbstgespräche zu führen. Alle anwesenden Menschen sehen Euch verdutzt an.";
                case 55: return "Als Ihr an einem regnerischen Tag auf dem Weg zur Kirche seid, fährt neben Euch eine Kutsche durch eine Schlammpfütze und Ihr werdet völlig verdreckt. Ihr besucht dennoch den Gottesdienst...";
                case 56: return "Ihr habt Eure Zunge bei einem heißen Tee verbrannt. Wegen den Schmerzen könnt Ihr nur noch lispeln.";
                case 57: return "Eure herzallerliebste Katze verstirbt. Von tiefster Trauer befallen geht Ihr nur noch gebückt.";
                case 58: return "Zu Gast bei einem Festmahl betretet Ihr feuertrunken die Tanzfläche.";
                case 59: return "Alter und Stress lassen Eure einstige Haarpracht erblassen.";
                case 60: return maennlich
                    ? "Ihr lasst Euch einen ungepflegten Bart wachsen."
                    : "Ihr beschließt, Euren Damenbart nicht mehr zu rasieren.";
                case 61: return "Bei wichtigen Gesprächen kommt Ihr häufig ins Stottern.";
                case 62: return "Bei einem Treffen mit Euren Geschäftspartnern kaut Ihr mit offenem Mund, sprecht zugleich und bekleckert auch noch Eure Kleidung...";
                case 63: return "Ein streunender Köter bellt Euch an. Tierlieb, wie Ihr seid, kuschelt Ihr mit dem verlausten Vieh in aller Öffentlichkeit...";
                case 64: return "In jedem Gespräch stellt Ihr einen Bezug zu Euren Wurstfingern her...";
                case 65: return "Eine angesehene Dame fällt neben Euch hin. Aber anstatt Ihr zu helfen, lacht Ihr sie aus...";

                default: return null;
            }
        }

        /// <summary>Liefert den Text des Gesundheitsereignisses zu <paramref name="val3"/> oder null (kein Ereignis).</summary>
        private static string GesundheitsText(int val3)
        {
            switch (val3)
            {
                case 0: return "Ihr beschließt, den Ratschlägen Eures Medikus nachzukommen.";
                case 1: return "Ihr trinkt seltener Alkohol.";
                case 2: return "Ihr esst des Öfteren Obst.";
                case 3: return "Ihr beschließt dieses Jahr öfters schwimmen zu gehn.";
                case 4: return "Ihr entschließt Euch, dieses Jahr kürzere Wege zu Fuß, statt mit einer Droschke zu bewältigen.";
                case 5: return "Ihr gebt das Rauchen auf.";
                case 6: return "Ihr entschließt Euch diesen Winter keine Kosten bei der Heizung Eures Anwesens zu scheuen.";
                case 7: return "Ihr macht einen großen Bogen um heruntergekommene Bordelle.";
                case 8: return "Ihr vermeidet unnötige Reisen.";
                case 9: return "Ihr entschließt Euch bei Schlechtwetter einen Schal zu tragen.";
                case 10: return "Ihr besucht öfters Badehäuser.";
                case 11: return "In letzter Zeit unternehmt Ihr öfters einen abendlichen Ausritt an der frischen Luft.";
                case 12: return "Entspannt sitzt Ihr im Park und genießt die Sonnenstrahlen. Eigentlich solltet Ihr noch Papierkram erledigen aber Ihr bleibt einfach gemütlich sitzen.";
                case 13: return "Manchmal entschließt Ihr Euch morgens vor dem Frühstück Laufen zu gehen.";
                case 14: return "Beeindruckt von der künstlerischen Darbietung einiger Zigeuner, beginnt Ihr selbst einfache Tricks zu üben.";
                case 15: return "Ihr entwickelt eine Vorliebe für Gemüse...";

                case 50: return "Ihr missachtet die Ratschläge Eures Medikus.";
                case 51: return "Ihr trinkt dieses Jahr öfters über den Durst...";
                case 52: return "Ihr rührt das Gemüse auf Eurem Teller nicht einmal mit eurem Schuhlöffel an.";
                case 53: return "Ihr gebt Eurer Schwimmtraining auf.";
                case 54: return "Ihr entschließt Euch auch kürzere Wegstrecken mit einer Droschke zurückzulegen.";
                case 55: return "Ihr beginnt mit dem Rauchen.";
                case 56: return "Um Geld zu sparen heizt Ihr diesen Winter weniger.";
                case 57: return "Ihr besucht dieses Jahr öfters zweitklassige Bordelle...";
                case 58: return "Ihr reist dieses Jahr sehr viel in einer zugigen Droschke.";
                case 59: return "Um Euch abzuhärten, tragt Ihr nur leichte Kleidung bei jedem Wetter.";
                case 60: return "Aus Scham meidet Ihr die örtlichen Badehäuser.";
                case 61: return "Ihr verlasst das Haus nur, wenn Ihr keine andere Wahl habt.";
                case 62: return "Aus Bequemlichkeit und Eurer geheimen Liebe für Euer Bett beschließt Ihr von nun an sämtlichen Papierkram liegend im Bett zu erledigen.";
                case 63: return "Der Rauch einer Schmiede in der Nachbarschaft verraucht des Öfteren Euren Wohnsitz.";
                case 64: return "Liebevoll bringt Ihr einem Obdachlosen eine warme Decke. Dabei fangt Ihr Euch aber Läuse ein...";
                case 65: return "Ratten nisten sich in Eurem Anwesen ein...";

                default: return null;
            }
        }
    }
}
