using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Conspiratio.Lib.Gameplay.Personen;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Verwaltet die lokalen Spielerprofile (profile.json im Spielstand-Verzeichnis): Anlegen, Umbenennen,
    /// Löschen sowie das aktive Profil. Speichert bei jeder Änderung sofort. Die spielübergreifende Wertung
    /// (Delta-Fold beim Speichern) folgt in einer späteren Phase.
    /// </summary>
    public class ProfilManager
    {
        private const string Dateiname = "profile.json";

        /// <summary>Maximale Länge eines Profilnamens (analog zu den Spielnamen).</summary>
        [PublicAPI]
        public int MaxLengthOfProfilName => 30;

        private readonly string _pfad;
        private ProfilAblage _ablage;

        /// <summary>
        /// Öffnet die Profilverwaltung für das angegebene Verzeichnis (i. d. R. das Spielstand-Verzeichnis).
        /// Existiert noch keine profile.json, wird mit einer leeren Ablage begonnen.
        /// </summary>
        public ProfilManager(string verzeichnis)
        {
            _pfad = Path.Combine(verzeichnis, Dateiname);
            Lade();
        }

        /// <summary>Alle vorhandenen Profile in Anlage-Reihenfolge.</summary>
        [PublicAPI]
        public IReadOnlyList<Profil> GetProfile() => _ablage.Profile;

        /// <summary>Das aktuell aktive Profil oder null, wenn keines gesetzt/vorhanden ist.</summary>
        [PublicAPI]
        public Profil GetAktivesProfil() => FindeProfil(_ablage.AktivesProfilId);

        /// <summary>Setzt das aktive Profil (Default-Auswahl für den primären lokalen Spieler).</summary>
        [PublicAPI]
        public void SetzeAktivesProfil(string profilId)
        {
            _ablage.AktivesProfilId = profilId;
            Speichere();
        }

        /// <summary>Legt ein neues Profil an. Ist noch kein aktives Profil gesetzt, wird dieses aktiv.</summary>
        [PublicAPI]
        public Profil ErstelleProfil(string name)
        {
            var profil = new Profil(KuerzeName(name));
            _ablage.Profile.Add(profil);

            if (string.IsNullOrEmpty(_ablage.AktivesProfilId))
                _ablage.AktivesProfilId = profil.Id;

            Speichere();
            return profil;
        }

        /// <summary>Benennt ein Profil um (die Id bleibt stabil).</summary>
        [PublicAPI]
        public void BenenneUm(string profilId, string neuerName)
        {
            var profil = FindeProfil(profilId);

            if (profil == null)
                return;

            profil.Name = KuerzeName(neuerName);
            Speichere();
        }

        /// <summary>Löscht ein Profil. War es das aktive, wird das erste verbleibende Profil aktiv (oder keines).</summary>
        [PublicAPI]
        public void LoescheProfil(string profilId)
        {
            _ablage.Profile.RemoveAll(p => p.Id == profilId);

            if (_ablage.AktivesProfilId == profilId)
                _ablage.AktivesProfilId = _ablage.Profile.FirstOrDefault()?.Id;

            Speichere();
        }

        /// <summary>Findet ein Profil anhand seiner Id oder liefert null.</summary>
        [PublicAPI]
        public Profil FindeProfil(string profilId)
        {
            return string.IsNullOrEmpty(profilId) ? null : _ablage.Profile.FirstOrDefault(p => p.Id == profilId);
        }

        private string KuerzeName(string name)
        {
            name = (name ?? "").Trim();
            return name.Length > MaxLengthOfProfilName ? name.Substring(0, MaxLengthOfProfilName) : name;
        }

        private void Lade()
        {
            try
            {
                if (File.Exists(_pfad))
                    _ablage = JsonConvert.DeserializeObject<ProfilAblage>(File.ReadAllText(_pfad));
            }
            catch
            {
                // Beschädigte/ungültige Datei: mit leerer Ablage weiterarbeiten, statt das Spiel zu blockieren.
                _ablage = null;
            }

            if (_ablage == null)
                _ablage = new ProfilAblage();

            if (_ablage.Profile == null)
                _ablage.Profile = new List<Profil>();
        }

        private void Speichere()
        {
            var verzeichnis = Path.GetDirectoryName(_pfad);

            if (!string.IsNullOrEmpty(verzeichnis))
                Directory.CreateDirectory(verzeichnis);

            File.WriteAllText(_pfad, JsonConvert.SerializeObject(_ablage, Formatting.Indented));
        }

        /// <summary>Serialisierbare Wurzel der profile.json (Profilliste + aktives Profil).</summary>
        [Serializable]
        private sealed class ProfilAblage
        {
            public string AktivesProfilId { get; set; }
            public List<Profil> Profile { get; set; } = new List<Profil>();
        }
    }
}
