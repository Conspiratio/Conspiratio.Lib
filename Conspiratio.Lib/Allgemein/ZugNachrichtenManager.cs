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
