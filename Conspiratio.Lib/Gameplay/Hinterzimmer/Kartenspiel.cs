using System;
using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Hinterzimmer
{
    /// <summary>
    /// Kapselt das Kartenspiel "17 und 4" gegen einen KI-Gegner (Migration von KartenSpielen aus dem
    /// WinForms-Client). Verwaltung des Ablaufs als Zustandsmaschine: Gegner ermitteln, Einsatz festlegen,
    /// austeilen, der Spieler kauft Karten, der Gegner zieht bis 17 und die Runde wird ausgewertet
    /// (Taler und Beziehung zum Gegner). Die eigentliche Anzeige/Interaktion übernimmt der Client.
    /// </summary>
    public class Kartenspiel
    {
        private static readonly string[] Kartennamen =
        {
            "eine Zwei", "eine Drei", "eine Vier", "eine Fünf", "eine Sechs", "eine Sieben", "eine Acht",
            "eine Neun", "eine Zehn", "einen Buben", "eine Dame", "einen König", "ein Ass"
        };

        private static readonly int[] Kartenwerte = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 11 };

        private int _gegnerId;
        private int _einsatz;
        private int _eigenePunkte;
        private int _gegnerPunkte;

        public string GegnerName { get; private set; }
        public string GegnerErSie { get; private set; }
        public string GegnerSeinenIhren { get; private set; }

        public bool FindetKartenspielStatt => SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpieltKartenGegenSpielerID() != 0;

        public bool HatSpielerGenugTaler() => SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetTaler() > SW.Statisch.GetKartenSpielenMinTaler();

        /// <summary>Der Mindesteinsatz (5 % des Vermögens des Spielers).</summary>
        public int MinEinsatz => Convert.ToInt32(AktSpieler.GetTaler() * 0.05);

        /// <summary>Der Höchsteinsatz (das gesamte Bargeld des Spielers).</summary>
        public int MaxEinsatz => AktSpieler.GetTaler();

        public void InitiiereKartenspielUndErmittleGegner()
        {
            if (SW.Dynamisch.GetGesetzX(4) > 0)
                SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).ErhoeheGesetzXUmEins(4);

            _gegnerId = AktSpieler.GetSpieltKartenGegenSpielerID();
            var kiGegner = SW.Dynamisch.GetKIwithID(_gegnerId);

            GegnerName = kiGegner.GetKompletterName();

            if (kiGegner.GetMaennlich())
            {
                GegnerErSie = "er";
                GegnerSeinenIhren = "seinen";
            }
            else
            {
                GegnerErSie = "sie";
                GegnerSeinenIhren = "ihren";
            }
        }

        /// <summary>Legt den Einsatz des Spielers für diese Runde fest.</summary>
        public void SetzeEinsatz(int einsatz) => _einsatz = einsatz;

        /// <summary>Teilt aus: der Gegner erhält eine Karte, der Spieler zwei. Liefert Karten und Punktestände.</summary>
        public KartenspielStart Austeilen()
        {
            int gegnerKarte = ZieheKarte();
            int eigeneKarte1 = ZieheKarte();
            int eigeneKarte2 = ZieheKarte();

            _gegnerPunkte = Kartenwerte[gegnerKarte];
            _eigenePunkte = Kartenwerte[eigeneKarte1] + Kartenwerte[eigeneKarte2];

            return new KartenspielStart
            {
                GegnerKarteName = Kartennamen[gegnerKarte],
                GegnerPunkte = _gegnerPunkte,
                EigeneKarte1Name = Kartennamen[eigeneKarte1],
                EigeneKarte2Name = Kartennamen[eigeneKarte2],
                EigenePunkte = _eigenePunkte
            };
        }

        /// <summary>Der Spieler zieht eine weitere Karte. Liefert die Karte, den neuen Punktestand und den Status.</summary>
        public KartenZug SpielerZiehtKarte()
        {
            int karte = ZieheKarte();
            _eigenePunkte += Kartenwerte[karte];

            KartenZugStatus status;

            if (_eigenePunkte > 21)
                status = KartenZugStatus.Ueberkauft;
            else if (_eigenePunkte == 21)
                status = KartenZugStatus.Genau21;
            else
                status = KartenZugStatus.Weiter;

            return new KartenZug { KarteName = Kartennamen[karte], EigenePunkte = _eigenePunkte, Status = status };
        }

        /// <summary>Wertet aus, dass sich der Spieler überkauft hat (Einsatz verloren), und beendet die Runde.</summary>
        public string SpielerUeberkauftAuswerten()
        {
            AktSpieler.ErhoeheTaler(-_einsatz);
            Gegner.ErhoeheBeziehungZuX(SW.Dynamisch.GetAktiverSpieler(), 50);

            string meldung = "Ihr habt Euch leider überkauft und damit " + _einsatz.ToStringGeld() + " an " + GegnerName + " verloren.";

            RundeBeenden();
            return meldung;
        }

        /// <summary>
        /// Der Gegner zieht Karten bis mindestens 17 Punkte, danach wird die Runde ausgewertet (Taler und
        /// Beziehung werden verbucht) und die Runde beendet.
        /// </summary>
        public KartenspielAuswertung GegnerZiehtUndWertetAus()
        {
            var auswertung = new KartenspielAuswertung();
            int aktiverSpieler = SW.Dynamisch.GetAktiverSpieler();

            while (_gegnerPunkte < 17)
            {
                int karte = ZieheKarte();
                _gegnerPunkte += Kartenwerte[karte];
                auswertung.GegnerZuege.Add(GegnerName + " kauft noch " + Kartennamen[karte] + " und besitzt damit " + _gegnerPunkte + " Punkte.");
            }

            if (_gegnerPunkte > 21)
            {
                AktSpieler.ErhoeheTaler(_einsatz);
                Gegner.ErhoeheBeziehungZuX(aktiverSpieler, 20);
                auswertung.TalerDelta = _einsatz;
                auswertung.Ergebnis = GegnerName + " hat sich mit " + _gegnerPunkte + " überkauft und damit " + _einsatz.ToStringGeld() + " an Euch verloren. Triumphierend streicht Ihr den Gewinn ein.";
            }
            else if (_gegnerPunkte > _eigenePunkte)
            {
                AktSpieler.ErhoeheTaler(-_einsatz);
                Gegner.ErhoeheBeziehungZuX(aktiverSpieler, 50);
                auswertung.TalerDelta = -_einsatz;
                auswertung.Ergebnis = "Leider konnte " + GegnerName + " Euch mit " + GegnerSeinenIhren + " " + _gegnerPunkte + " Punkten Eure " + _eigenePunkte + " schlagen. Ihr verliert Euren Einsatz in Höhe von " + _einsatz.ToStringGeld() + ".";
            }
            else if (_gegnerPunkte < _eigenePunkte)
            {
                AktSpieler.ErhoeheTaler(_einsatz);
                Gegner.ErhoeheBeziehungZuX(aktiverSpieler, 20);
                auswertung.TalerDelta = _einsatz;
                auswertung.Ergebnis = "Mit Euren " + _eigenePunkte + " Punkten konntet Ihr die " + _gegnerPunkte + " Punkte von " + GegnerName + " übertreffen. Jubelnd streicht Ihr Euren Gewinn in Höhe von " + _einsatz.ToStringGeld() + " ein.";
            }
            else
            {
                Gegner.ErhoeheBeziehungZuX(aktiverSpieler, 30);
                auswertung.TalerDelta = 0;
                auswertung.Ergebnis = "Ihr besitzt mit " + _eigenePunkte + " Punkten gleich viele wie " + GegnerName + " und habt daher ein Unentschieden erlangt. Beide gehen ohne Gewinn nach Hause...";
            }

            RundeBeenden();
            return auswertung;
        }

        /// <summary>
        /// Der Spieler hat zu wenig Taler für das Spiel: der Gegner verlässt verärgert den Tisch
        /// (Beziehung sinkt). Liefert die Meldung und beendet die Runde.
        /// </summary>
        public string LehneMangelsTalerAb()
        {
            string meldung = Gegner.GetKompletterName() + ": \"Als Ihr mich zum Kartenspielen eingeladen habt,\nsah es so aus als hättet Ihr auch das nötige Geld dafür.\"\n\n " +
                             Gegner.GetKompletterName() + " verlässt wütend den Tisch.";

            Gegner.ErhoeheBeziehungZuX(SW.Dynamisch.GetAktiverSpieler(), -20);
            RundeBeenden();

            return meldung;
        }

        private static int ZieheKarte() => SW.Statisch.Rnd.Next(0, Kartennamen.Length);

        private void RundeBeenden() => AktSpieler.SetSpieltKartenGegenSpielerID(0);

        private static HumSpieler AktSpieler => SW.Dynamisch.GetAktHum();

        private KISpieler Gegner => SW.Dynamisch.GetKIwithID(_gegnerId);
    }

    /// <summary>Das Ergebnis des Austeilens: die erste Gegnerkarte und die beiden Startkarten des Spielers.</summary>
    public class KartenspielStart
    {
        public string GegnerKarteName { get; set; }
        public int GegnerPunkte { get; set; }
        public string EigeneKarte1Name { get; set; }
        public string EigeneKarte2Name { get; set; }
        public int EigenePunkte { get; set; }
    }

    /// <summary>Der Status nach dem Ziehen einer Karte durch den Spieler.</summary>
    public enum KartenZugStatus
    {
        Weiter,
        Genau21,
        Ueberkauft
    }

    /// <summary>Das Ergebnis eines Kartenzugs des Spielers.</summary>
    public class KartenZug
    {
        public string KarteName { get; set; }
        public int EigenePunkte { get; set; }
        public KartenZugStatus Status { get; set; }
    }

    /// <summary>Die Auswertung der Runde: die Karten-Meldungen des Gegners, das Ergebnis und der Taler-Saldo.</summary>
    public class KartenspielAuswertung
    {
        public List<string> GegnerZuege { get; } = new List<string>();
        public string Ergebnis { get; set; }
        public int TalerDelta { get; set; }
    }
}
