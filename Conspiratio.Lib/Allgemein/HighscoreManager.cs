using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Conspiratio.Lib.Gameplay.Einstellungen;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Ein Eintrag der lokalen Auftrags-Bestenliste (ein erfüllter Auftrag).</summary>
    [Serializable]
    public class HighscoreEintrag
    {
        public EnumAuftrag Auftrag { get; set; }
        public string SpielerName { get; set; }

        /// <summary>Das Spieljahr (A.D.), in dem der Auftrag erfüllt wurde.</summary>
        public int Spieljahr { get; set; }

        /// <summary>Vergangene Spieljahre seit Spielbeginn – je weniger, desto besser (= Wertung).</summary>
        public int JahreGespielt { get; set; }

        /// <summary>Anzahl der menschlichen Mitspieler in dieser Partie.</summary>
        public int MitspielerAnzahl { get; set; }

        public DateTime Datum { get; set; }
    }

    /// <summary>
    /// Verwaltet die lokale Auftrags-Bestenliste (highscores.json im Spielstand-Verzeichnis, analog zu
    /// <see cref="ProfilManager"/>). Speichert bei jeder Änderung sofort und liest defensiv (eine
    /// beschädigte Datei blockiert das Spiel nicht). Sortierung: je weniger gespielte Jahre, desto besser.
    /// </summary>
    public class HighscoreManager
    {
        private const string Dateiname = "highscores.json";

        private readonly string _pfad;
        private List<HighscoreEintrag> _eintraege;

        /// <summary>Öffnet die Bestenliste für das angegebene Verzeichnis (i. d. R. das Spielstand-Verzeichnis).</summary>
        public HighscoreManager(string verzeichnis)
        {
            _pfad = Path.Combine(verzeichnis, Dateiname);
            Lade();
        }

        /// <summary>Fügt einen erfüllten Auftrag als Bestenlisten-Eintrag hinzu und speichert.</summary>
        [PublicAPI]
        public void FuegeEintragHinzu(EnumAuftrag auftrag, string spielerName, int spieljahr, int jahreGespielt, int mitspielerAnzahl)
        {
            _eintraege.Add(new HighscoreEintrag
            {
                Auftrag = auftrag,
                SpielerName = (spielerName ?? "").Trim(),
                Spieljahr = spieljahr,
                JahreGespielt = jahreGespielt,
                MitspielerAnzahl = mitspielerAnzahl,
                Datum = DateTime.Now
            });

            Speichere();
        }

        /// <summary>Die Einträge eines Auftrags, aufsteigend nach gespielten Jahren (schnellste zuerst).</summary>
        [PublicAPI]
        public IReadOnlyList<HighscoreEintrag> GetEintraege(EnumAuftrag auftrag) =>
            _eintraege.Where(e => e.Auftrag == auftrag)
                      .OrderBy(e => e.JahreGespielt)
                      .ThenBy(e => e.Datum)
                      .ToList();

        /// <summary>Gibt es überhaupt Einträge für den angegebenen Auftrag?</summary>
        [PublicAPI]
        public bool HatEintraege(EnumAuftrag auftrag) => _eintraege.Any(e => e.Auftrag == auftrag);

        private void Lade()
        {
            try
            {
                if (File.Exists(_pfad))
                    _eintraege = JsonConvert.DeserializeObject<List<HighscoreEintrag>>(File.ReadAllText(_pfad));
            }
            catch
            {
                // Beschädigte/ungültige Datei: mit leerer Liste weiterarbeiten, statt das Spiel zu blockieren.
                _eintraege = null;
            }

            if (_eintraege == null)
                _eintraege = new List<HighscoreEintrag>();
        }

        private void Speichere()
        {
            var verzeichnis = Path.GetDirectoryName(_pfad);

            if (!string.IsNullOrEmpty(verzeichnis))
                Directory.CreateDirectory(verzeichnis);

            File.WriteAllText(_pfad, JsonConvert.SerializeObject(_eintraege, Formatting.Indented));
        }
    }
}
