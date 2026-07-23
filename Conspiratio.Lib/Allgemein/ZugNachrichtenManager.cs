using System;
using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Zugnachrichten-Ereignisse am Ende des Zugs eines Spielers (extrahiert aus dem
    /// alten WinForms-Client): Gesetzesverstöße mit Strafen, Statistik, Amtseinkommen, Anwesen,
    /// die Sterbeprüfung samt Entfernung des Spielers sowie der Schuldenprozess mit Schuldturm.
    /// </summary>
    public class ZugNachrichtenManager
    {
        private const int AnzahlGeschworene = 11;

        /// <summary>
        /// Prüft die Gesetzesverstöße des aktiven Spielers (Höchstzahl Anwesen, maximale Taler,
        /// Schlösserverbot), verbucht Strafen und Deliktpunkte.
        /// </summary>
        /// <returns>Die anzuzeigenden Strafmeldungen (leer, wenn es keine Verstöße gab).</returns>
        [PublicAPI]
        public List<string> PruefeVerbrechen()
        {
            var meldungen = new List<string>();
            var spieler = SW.Dynamisch.GetAktHum();

            // #2 Höchstzahl Anwesen
            int anzahlAnwesen = spieler.GetAnzahlHaeuser();
            int erlaubt = SW.Dynamisch.GetGesetzX(2);

            if (anzahlAnwesen > erlaubt)
            {
                spieler.ErhoeheGesetzXUmEins(2);

                int geldabzug = Convert.ToInt32(spieler.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler()) * (0.15 * (anzahlAnwesen - erlaubt)));

                if (geldabzug < 1000)
                    geldabzug = 1000;

                spieler.ErhoeheTaler(-geldabzug);
                meldungen.Add("Ihr besitzt " + anzahlAnwesen + " Anwesen und habt damit die maximal erlaubte Anzahl\nan Anwesen überschritten. Ihr müsst daher " + geldabzug.ToStringGeld() + " Strafe zahlen");
            }

            // #3 Maximale Taler
            if (spieler.GetTaler() > SW.Dynamisch.GetGesetzX(3) * 100000)
            {
                spieler.ErhoeheGesetzXUmEins(3);

                int geldabzug = Convert.ToInt32(spieler.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler()) * 0.15);

                string meldung = "Ihr besitzt " + spieler.GetTaler().ToStringGeld() + " und habt damit die maximale Anzahl an Taler überschritten.\n\nIhr müsst daher " + geldabzug.ToStringGeld() + " Strafe zahlen";
                spieler.ErhoeheTaler(-geldabzug);
                meldungen.Add(meldung);
            }

            // #42 Schlösserverbot
            if (SW.Dynamisch.GetGesetzX(42) > 0)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    if (spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId).GetHausID() == SW.Statisch.GetMaxHausID() - 1)
                        spieler.ErhoeheGesetzXUmEins(42);
                }
            }

            return meldungen;
        }

        /// <summary>
        /// Zählt laufende Sabotagen und Spionagen in der Spielerstatistik mit.
        /// </summary>
        [PublicAPI]
        public void ErweitereStatistik()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            for (int i = 0; i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (spieler.GetAktiveSabotage(i).GetDauer() > 0)
                    spieler.GetSpielerStatistik().HiSabotagen++;

                if (spieler.GetAktiveSpionage(i).GetKosten() > 0)
                    spieler.GetSpielerStatistik().HiSpionagen++;
            }
        }

        /// <summary>
        /// Schreibt dem aktiven Spieler sein Amtseinkommen gut.
        /// </summary>
        /// <returns>Das erhaltene Einkommen oder 0, wenn er kein Amt bekleidet.</returns>
        [PublicAPI]
        public int KassiereAmtseinkommen()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.GetAmtID() == 0)
                return 0;

            int einkommen = SW.Statisch.GetAmtwithID(spieler.GetAmtID()).GetEinkommen();
            spieler.ErhoeheTaler(einkommen);

            return einkommen;
        }

        /// <summary>
        /// Aktualisiert die Anwesen des aktiven Spielers: Bauzeiten laufen ab, der Zustand
        /// bestehender Anwesen verschlechtert sich und beauftragte Renovierungen werden ausgeführt.
        /// </summary>
        /// <returns>Die Meldungen über fertiggestellte Anwesen.</returns>
        [PublicAPI]
        public List<string> AktualisiereAnwesen()
        {
            var meldungen = new List<string>();
            var spieler = SW.Dynamisch.GetAktHum();

            for (int stadtId = SW.Statisch.GetMinStadtID(); stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                var haus = spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId);

                if (haus.GetRestlicheBauzeit() > 0)
                {
                    haus.SetRestlicheBauzeit(haus.GetRestlicheBauzeit() - 1);

                    if (haus.GetRestlicheBauzeit() == 0)
                        meldungen.Add(haus.GetNameInklPronomen(false) + " in " + SW.Dynamisch.GetStadtwithID(stadtId).GetGebietsName() + " wurde fertiggestellt.");
                }
                else if (haus.GetHausID() != 0 && haus.GetStadtID() != 0)  // Ist das Haus vorhanden?
                {
                    // Zustand verringern
                    haus.ZustandInProzent -= 8;  // 8 % Abzug

                    // Renovierung durchführen
                    if (haus.InDiesemJahrRenovieren)
                    {
                        haus.ZustandInProzent = 100;
                        haus.InDiesemJahrRenovieren = false;
                    }
                }
            }

            return meldungen;
        }

        /// <summary>
        /// Prüft, ob der aktive Spieler in diesem Jahr stirbt (ab 36 Jahren über die Sterbeformel,
        /// oberhalb des Maximalalters immer).
        /// </summary>
        [PublicAPI]
        public bool StirbtAktiverSpieler()
        {
            int alter = SW.Dynamisch.GetAktHum().GetAlter();

            if (alter <= 35)
                return false;

            if (alter > SW.Statisch.GetMaxAlter())
                return true;

            int lebtNochSoVieleJahre = SW.Dynamisch.GetSpXlebtNochSoVielJahre(SW.Dynamisch.GetAktiverSpieler()) + SW.Statisch.Rnd.Next(-1, 2);

            return lebtNochSoVieleJahre <= 0;
        }

        /// <summary>
        /// Liefert einen zufälligen Todesursachen-Text (mit einer kleinen Chance auf die besondere Todesursache 0).
        /// </summary>
        [PublicAPI]
        public string GetZufaelligeTodesursache()
        {
            int todestext = SW.Statisch.Rnd.Next(1, 10);

            if (SW.Statisch.Rnd.Next(1, 200) == 1)
                todestext = 0;

            return SW.Statisch.GetTexteTodesursachenX(todestext);
        }

        /// <summary>
        /// Führt den Tod des aktiven Spielers durch und entfernt ihn aus dem Spiel.
        /// </summary>
        /// <returns>True, wenn kein menschlicher Spieler mehr im Spiel ist (das Spiel ist vorbei).</returns>
        [PublicAPI]
        public bool FuehreTodDesAktivenSpielersDurch()
        {
            // TODO: Testament und Erbfolge migrieren (aktuell erbt immer das Erzbistum, der Spieler scheidet aus)
            return SW.Dynamisch.EntferneAktivenSpielerAusDemSpiel();
        }

        /// <summary>
        /// Prüft, ob sich der aktive Spieler wegen zu hoher Schulden vor seinen Gläubigern
        /// verantworten muss (abhängig von Schuldenhöhe und Ansehen).
        /// </summary>
        [PublicAPI]
        public bool MussSichVorGlaeubigernVerantworten()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            return spieler.GetTaler() <= -100 &&
                   spieler.GetTaler() < SW.Statisch.GetMaxSchulden() - spieler.GetAnsehen() * 10;
        }

        /// <summary>
        /// Führt den Schuldenprozess durch: 11 zufällige Geschworene stimmen über die Schuld ab.
        /// Bei einem Schuldspruch landet der Spieler nächstes Jahr im Schuldturm und verliert
        /// Gesundheit und Ansehen.
        /// </summary>
        [PublicAPI]
        public SchuldenProzessErgebnis FuehreSchuldenProzessDurch()
        {
            var ergebnis = new SchuldenProzessErgebnis();
            var spieler = SW.Dynamisch.GetAktHum();

            // 11 verschiedene KI-Spieler als Geschworene ermitteln
            var geschworene = new int[AnzahlGeschworene];

            for (int i = 0; i < AnzahlGeschworene; i++)
            {
                int kandidat = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());

                while (Array.IndexOf(geschworene, kandidat) >= 0)
                    kandidat = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());

                geschworene[i] = kandidat;
            }

            // Abstimmung durchführen
            int schulden = spieler.GetTaler();
            int ansehen = spieler.GetAnsehen();
            int anzahlSchuldsprueche = 0;

            for (int i = 0; i < AnzahlGeschworene; i++)
            {
                int beziehung = SW.Dynamisch.GetKIwithID(geschworene[i]).GetBeziehungZuKIX(SW.Dynamisch.GetAktiverSpieler());
                int zufall = SW.Statisch.Rnd.Next(0, 16);

                bool stimmtSchuldig = schulden + beziehung * 20 + zufall * 20 + ansehen * 3 < 0;

                if (stimmtSchuldig)
                    anzahlSchuldsprueche++;

                ergebnis.GeschworenenNamen.Add(SW.Dynamisch.GetKIwithID(geschworene[i]).GetName());
                ergebnis.Urteile.Add(stimmtSchuldig);
            }

            ergebnis.Schuldig = anzahlSchuldsprueche > (AnzahlGeschworene - 1) / 2;

            if (ergebnis.Schuldig)
            {
                // Dann landet der Spieler im Kerker
                spieler.SetSitztImKerker(true);
                spieler.SetSpieltKartenGegenSpielerID(0);

                // Von Wahlen ausschließen
                if (spieler.GetWahlTeilnahme() != 0)
                    spieler.SetWahlTeilnahme(0);

                // Gesundheit und Ansehen reduzieren
                spieler.ErhoeheGesundheit(-SW.Statisch.GetKerkerGesundheit());
                spieler.ErhoehePermaAnsehen(-SW.Statisch.GetKerkerAnsehen());
            }

            return ergebnis;
        }

        /// <summary>
        /// Korruptionsgelder (Privileg 21): Als Amtsträger erhält der Spieler kleine "Spenden".
        /// </summary>
        /// <returns>Die anzuzeigende Meldung oder null, wenn keine anfallen.</returns>
        [PublicAPI]
        public string KassiereKorruptionsgelder()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (!spieler.CheckPrivilegX(21))
                return null;

            int betrag = SW.Statisch.Rnd.Next(100, SW.Statisch.GetmaxKorruptionsGelder());
            spieler.ErhoeheTaler(betrag);

            return "Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) +
                   " habt Ihr dieses Jahr " + betrag.ToStringGeld() + " in Form von kleinen 'Spenden' erhalten.";
        }

        /// <summary>
        /// Schmuggelgelder (Privileg 22): Als Amtsträger verdient der Spieler an Schmuggelgeschäften.
        /// </summary>
        /// <returns>Die anzuzeigende Meldung oder null, wenn keine anfallen.</returns>
        [PublicAPI]
        public string KassiereSchmuggelgelder()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (!spieler.CheckPrivilegX(22))
                return null;

            int betrag = SW.Statisch.Rnd.Next(1000, 10000);
            spieler.ErhoeheTaler(betrag);

            return "Als " + SW.Dynamisch.GetAmtsnameVonSPIDx(SW.Dynamisch.GetAktiverSpieler()) +
                   " habt Ihr dieses Jahr " + betrag.ToStringGeld() + " mit Schmuggelgeschäften verdient.";
        }

        /// <summary>
        /// Kerkerklatsch (Privileg 7): Als Kerkermeister werden dem Spieler von einem Gefangenen
        /// Beweise gegen einen zufälligen Amtsträger seiner Amtsstadt zugetragen.
        /// </summary>
        /// <returns>Die anzuzeigende Meldung oder null, wenn keine anfällt.</returns>
        [PublicAPI]
        public string ErmittleKerkerklatsch()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (!spieler.CheckPrivilegX(7))
                return null;

            var amtsstadt = SW.Dynamisch.GetStadtwithID(spieler.GetAmtGebiet());

            int opferAmtId = 0;

            // Einen belegten, fremden Amtsposten der Amtsstadt suchen (Slot 15 ist der eigene).
            for (int versuch = 0; versuch < 1000; versuch++)
            {
                int kandidat = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxAmtStadtID());

                if (kandidat != 15 && amtsstadt.GetAmtX(kandidat) != 0)
                {
                    opferAmtId = kandidat;
                    break;
                }
            }

            if (opferAmtId == 0)
                return null;

            int opferSpielerId = amtsstadt.GetAmtX(opferAmtId);
            int beweismaechtigkeit = SW.Statisch.Rnd.Next(1, 5);

            var spionage = spieler.GetAktiveSpionage(opferSpielerId);
            spionage.SetDelikte(spionage.GetDelikte() + beweismaechtigkeit);

            return "Als Kerkermeister habt Ihr dieses Jahr von einem Eurer Gefangenen\n" +
                   BeweisStaerkeText(beweismaechtigkeit) + " Beweise gegen " +
                   SW.Dynamisch.GetKIwithID(opferSpielerId).GetKompletterName() + " zugetragen bekommen.";
        }

        /// <summary>
        /// Wickelt die laufenden Spionagen des Spielers ab: reduziert deren Dauer, entfernt ausgelaufene
        /// und sammelt – abhängig von den Deliktpunkten des Ziels – neue Beweise ein.
        /// </summary>
        /// <returns>Die zusammengefasste Meldung oder null (wie im Original nur, wenn Beweise gefunden wurden).</returns>
        [PublicAPI]
        public string ErmittleSpionageNachrichten()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var zeilen = new List<string>();

            bool etwasSpioniert = false;
            bool beweiseGefunden = false;

            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (spieler.GetAktiveSpionage(i).GetKosten() <= 0)
                    continue;

                // Dauer reduzieren, um endlose Spionagen bei Amtsverlust des Ziels zu verhindern.
                spieler.GetAktiveSpionage(i).DauerReduzieren();

                if (spieler.GetAktiveSpionage(i).GetDauer() < 0)
                {
                    zeilen.Add("Eure Spionage gegen " + SW.Dynamisch.GetSpWithID(i).GetKompletterName() + " ist ausgelaufen.");
                    spieler.AktiveSpionageEntfernen(i);
                    continue;
                }

                spieler.GetAktiveSpionage(i).SetJahr(SW.Dynamisch.GetAktuellesJahr());

                int opferDelikte = SW.Dynamisch.GetSpWithID(i).GetDeliktpunkte() - spieler.GetAktiveSpionage(i).GetDelikte();

                // Durch Privilegien geschützte Ziele erschweren die Beweisbeschaffung.
                if (SW.Dynamisch.GetSpWithID(i).CheckPrivilegX(18))
                    opferDelikte = opferDelikte * 2 / 3;
                if (SW.Dynamisch.GetSpWithID(i).CheckPrivilegX(19))
                    opferDelikte /= 2;

                etwasSpioniert = true;

                int zufall = SW.Statisch.Rnd.Next(0, 5);

                if (opferDelikte > zufall)
                {
                    beweiseGefunden = true;

                    int beweismaechtigkeit = SW.Statisch.Rnd.Next(1, zufall + 1);
                    spieler.GetAktiveSpionage(i).SetDelikte(spieler.GetAktiveSpionage(i).GetDelikte() + beweismaechtigkeit);

                    zeilen.Add("Eure Spione haben Euch " + BeweisStaerkeText(beweismaechtigkeit) + " Beweise gegen " +
                               SW.Dynamisch.GetSpWithID(i).GetKompletterName() + " gebracht.");
                }
            }

            if (etwasSpioniert && beweiseGefunden)
                return string.Join("\n\n", zeilen);

            return null;
        }

        /// <summary>
        /// Wickelt die laufenden Sabotagen des Spielers ab: mit einer gewissen Chance richten die
        /// Saboteure beim Ziel einen vermögensabhängigen Schaden an; abgelaufene Sabotagen werden entfernt.
        /// </summary>
        /// <returns>Die zusammengefasste Meldung oder null, wenn nichts sabotiert wurde.</returns>
        [PublicAPI]
        public string ErmittleSabotageNachrichten()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var zeilen = new List<string>();

            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (spieler.GetAktiveSabotage(i).GetDauer() <= 0)
                    continue;

                int chance = 2;

                if (SW.Dynamisch.GetSpWithID(i).CheckPrivilegX(18))
                    chance = 3;
                if (SW.Dynamisch.GetSpWithID(i).CheckPrivilegX(19))
                    chance = 4;

                if (SW.Statisch.Rnd.Next(0, chance) != 1)
                    continue;

                int sabMaechtigkeit = SW.Statisch.Rnd.Next(1, 9);
                int schaden = SW.Dynamisch.GetSpWithID(i).GetGesamtVermoegen(i) * sabMaechtigkeit / 100;

                zeilen.Add("Es gelang Euren Saboteueren bei " + SW.Dynamisch.GetSpWithID(i).GetKompletterName() + " " +
                           SabotageStaerkeText(sabMaechtigkeit) + " Schäden in Höhe von " + schaden + " anzurichten.");

                SW.Dynamisch.GetSpWithID(i).ErhoeheTaler(-schaden);

                spieler.GetAktiveSabotage(i).ReduziereDauerUmEins();

                if (spieler.GetAktiveSabotage(i).GetDauer() <= 0)
                    spieler.AktiveSabotageEntfernen(i);
            }

            return zeilen.Count > 0 ? string.Join("\n\n", zeilen) : null;
        }

        /// <summary>
        /// Führt eine vom Spieler beauftragte Ermordung eines KI-Spielers aus (mit Erfolgschance).
        /// </summary>
        /// <returns>Die anzuzeigende Meldung oder null, wenn kein Auftrag vorlag.</returns>
        [PublicAPI]
        public string FuehreErmordungDurch()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            int zielId = spieler.GetErmordetKISpielerID();

            if (zielId == 0)
                return null;

            spieler.SetErmordetKISpielerID(0);

            if (SW.Statisch.Rnd.Next(0, SW.Statisch.GetErmordungsChance()) == 0)
            {
                spieler.GetSpielerStatistik().HiErfolgreicheErmordungen++;
                SW.Dynamisch.GetKIwithID(zielId).SetStirbt(true);

                return "Die Ermordung von " + SW.Dynamisch.GetKIwithID(zielId).GetKompletterName() +
                       " wird wie geplant durchgeführt!\n\nMöge die Wahrheit nie ans Licht kommen...";
            }

            return "Die Ermordung von " + SW.Dynamisch.GetKIwithID(zielId).GetKompletterName() +
                   " ist fehlgeschlagen.\n\nDie Männer und Euer Geld sind spurlos verschwunden...";
        }

        /// <summary>
        /// Führt einen vom Spieler beauftragten vergifteten Wein aus (mit Erfolgschance). Bei Erfolg
        /// stirbt das (KI-)Ziel, sonst leidet nur dessen Gesundheit; menschliche Ziele leiden immer nur.
        /// </summary>
        /// <returns>Die beiden Meldungen (Andeutung, dann Ergebnis) oder null, wenn kein Auftrag vorlag.</returns>
        [PublicAPI]
        public List<string> FuehreVergiftetenWeinDurch()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            int zielId = spieler.GetVergiftetWeinVonKISpielerID();

            if (zielId == 0)
                return null;

            spieler.SetVergiftetWeinVonKISpielerID(0);

            var ziel = SW.Dynamisch.GetKIwithID(zielId);

            string andeutung = "Bei einem Fest mischt Ihr " + ziel.GetKompletterName() + " einen Tropfen Gift in " +
                               ziel.GetSeinenIhren() + " Trank. Einige Tage später erfahrt Ihr von Euren Informanten, dass " +
                               ziel.GetName() + " ein seltsames Leiden hat...";

            int rand = SW.Statisch.Rnd.Next(0, SW.Statisch.GetVergifteterWeinChance());

            // Menschliche Ziele leiden nur an ihrer Gesundheit.
            if (zielId < SW.Statisch.GetMinKIID())
                rand = 0;

            string ergebnis;

            if (rand == 1)
            {
                ziel.SetStirbt(true);
                ergebnis = "Bald wird " + ziel.GetName() + " " + ziel.GetSeinerIhrer() +
                           " Krankheit erliegen\nMöge die Wahrheit nie ans Licht kommen...";
            }
            else
            {
                ziel.ErhoeheGesundheit(-10);
                ergebnis = "Nach kurzer Zeit erholt sich " + ziel.GetName() + " wieder. " + ziel.GetSeinerIhrer() +
                           " Gesundheit hat gelitten.";
            }

            return new List<string> { andeutung, ergebnis };
        }

        /// <summary>Übersetzt eine Beweismächtigkeit (1..4) in die Beschreibung des Originals.</summary>
        private static string BeweisStaerkeText(int beweismaechtigkeit)
        {
            if (beweismaechtigkeit > 3) return "stark belastende";
            if (beweismaechtigkeit > 2) return "belastende";
            if (beweismaechtigkeit > 1) return "einige";
            return "schwache";
        }

        /// <summary>Übersetzt eine Sabotagemächtigkeit (1..8) in die Beschreibung des Originals.</summary>
        private static string SabotageStaerkeText(int sabMaechtigkeit)
        {
            if (sabMaechtigkeit > 8) return "sehr starke";
            if (sabMaechtigkeit > 6) return "starke";
            if (sabMaechtigkeit > 4) return "einige";
            if (sabMaechtigkeit > 2) return "geringe";
            return "sehr geringe";
        }
    }

    /// <summary>
    /// Das Ergebnis eines Schuldenprozesses mit den Urteilen der einzelnen Geschworenen.
    /// </summary>
    public class SchuldenProzessErgebnis
    {
        public List<string> GeschworenenNamen { get; } = new List<string>();

        public List<bool> Urteile { get; } = new List<bool>();

        public bool Schuldig { get; internal set; }
    }
}
