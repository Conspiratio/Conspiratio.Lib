using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

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

        #region Wertung (Delta-Fold beim Speichern)

        /// <summary>Die additiven Int-Kennzahlen der <see cref="SpielerStatistik"/> – alle außer dem Max-Feld <c>SoHoechstesAmt</c>.</summary>
        private static readonly PropertyInfo[] AdditiveStatFelder =
            typeof(SpielerStatistik).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(int) && p.CanRead && p.CanWrite && p.Name != nameof(SpielerStatistik.SoHoechstesAmt))
                .ToArray();

        /// <summary>
        /// Faltet den Statistik-Zuwachs aller menschlichen Spieler mit Profil-Zuordnung in ihr jeweiliges
        /// Profil (Delta seit der letzten Wertung) und speichert. Bei jedem Speichern des Spiels aufzurufen.
        /// Mehrfachaufrufe zählen dank der gespeicherten Snapshots nicht doppelt.
        /// </summary>
        [PublicAPI]
        public void WerteLaufendesSpiel()
        {
            bool geaendert = false;

            for (int id = 1; id < SW.Statisch.GetMinKIID(); id++)
            {
                var mensch = SW.Dynamisch.GetHumWithID(id);

                if (mensch == null || string.IsNullOrEmpty(mensch.GetName()) || string.IsNullOrEmpty(mensch.ProfilId))
                    continue;

                var profil = FindeProfil(mensch.ProfilId);

                if (profil == null)
                    continue;

                FalteSpielerInProfil(mensch, id, profil);
                geaendert = true;
            }

            if (geaendert)
                Speichere();
        }

        private static void FalteSpielerInProfil(HumSpieler mensch, int spielerId, Profil profil)
        {
            var aktuell = mensch.GetSpielerStatistik();
            var basis = mensch.GewerteteStatistik;

            // Additive Zähler: nur den Zuwachs seit der letzten Wertung aufaddieren.
            foreach (var feld in AdditiveStatFelder)
            {
                int neu = (int)feld.GetValue(aktuell);
                int alt = basis != null ? (int)feld.GetValue(basis) : 0;
                int delta = neu - alt;

                if (delta != 0)
                    feld.SetValue(profil.Gesamt, (int)feld.GetValue(profil.Gesamt) + delta);
            }

            // Max-Werte (nicht additiv).
            if (aktuell.SoHoechstesAmt > profil.Meta.HoechstesAmt)
                profil.Meta.HoechstesAmt = aktuell.SoHoechstesAmt;

            int vermoegen = mensch.GetGesamtVermoegen(spielerId);

            if (vermoegen > profil.Meta.HoechstesVermoegen)
                profil.Meta.HoechstesVermoegen = vermoegen;

            // Gespielte Jahre (Delta gegenüber dem festen Startjahr).
            int jahre = SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr;

            if (jahre > mensch.GewerteteJahre)
            {
                profil.Meta.GespielteJahre += jahre - mensch.GewerteteJahre;
                mensch.GewerteteJahre = jahre;
            }

            // Das Spiel einmalig als gespielt zählen.
            if (!mensch.WurdeGezaehlt)
            {
                profil.Meta.SpieleGesamt++;
                mensch.WurdeGezaehlt = true;
            }

            mensch.GewerteteStatistik = KopiereStatistik(aktuell);
            profil.ZuletztGespielt = DateTime.Now;
        }

        private static SpielerStatistik KopiereStatistik(SpielerStatistik quelle)
        {
            var kopie = new SpielerStatistik();

            foreach (var feld in typeof(SpielerStatistik).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (feld.PropertyType == typeof(int) && feld.CanRead && feld.CanWrite)
                    feld.SetValue(kopie, feld.GetValue(quelle));
            }

            return kopie;
        }

        #endregion

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
