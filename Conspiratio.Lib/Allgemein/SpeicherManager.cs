using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Speichert und lädt Spielstände als serialisierte <see cref="Spielstand"/>-Dateien (*.dat),
    /// im selben Format wie der alte WinForms-Client — Spielstände bleiben dadurch zwischen beiden
    /// Clients austauschbar. Der Client muss dafür die BinaryFormatter-Serialisierung erlauben
    /// (unter .NET ab Version 5: EnableUnsafeBinaryFormatterSerialization).
    /// </summary>
    public class SpeicherManager
    {
        private readonly string _savegamePath;

        public SpeicherManager(string savegamePath)
        {
            _savegamePath = savegamePath;
        }

        /// <summary>
        /// Serialisiert den aktuellen Spielstand unter dem angegebenen Namen.
        /// </summary>
        [PublicAPI]
        public bool Speichern(string name, out string fehler)
        {
            fehler = "";

            try
            {
                Directory.CreateDirectory(_savegamePath);

                IFormatter formatter = new BinaryFormatter();

                using (Stream stream = new FileStream(GetDateiname(name), FileMode.Create, FileAccess.Write, FileShare.None))
                    formatter.Serialize(stream, SW.Dynamisch.Spielstand);

                return true;
            }
            catch (Exception ex)
            {
                fehler = $"Fehler beim Speichern des Spielstandes. Meldung: {ex.Message}";
                return false;
            }
        }

        [PublicAPI]
        public string GetAutosaveName()
        {
            return SW.Dynamisch.SpielName + "_" + SW.Dynamisch.GetAktuellesJahr();
        }

        /// <summary>
        /// Speichert automatisch unter "&lt;Spielname&gt;_&lt;Jahr&gt;" und löscht den vorvorletzten Autosave.
        /// </summary>
        [PublicAPI]
        public bool Autosave(out string fehler)
        {
            if (!Speichern(GetAutosaveName(), out fehler))
                return false;

            // Vorvorletzten Autosave löschen
            try
            {
                File.Delete(GetDateiname(SW.Dynamisch.SpielName + "_" + (SW.Dynamisch.GetAktuellesJahr() - 2)));
            }
            catch
            {
                // Ein fehlgeschlagenes Aufräumen ist unkritisch
            }

            return true;
        }

        /// <summary>
        /// Lädt den Spielstand mit dem angegebenen Namen und setzt ihn als aktuellen Spielstand.
        /// </summary>
        [PublicAPI]
        public bool Laden(string name, out string fehler)
        {
            fehler = "";
            string dateiname = GetDateiname(name);

            // TODO: Spielstände im alten Textformat (*.cons) werden nicht unterstützt — dafür den WinForms-Client verwenden, der konvertiert sie beim Laden
            if (!File.Exists(dateiname))
            {
                fehler = $"Es ist kein gültiger Spielstand mit diesem Namen vorhanden: {name}.dat";
                return false;
            }

            try
            {
                IFormatter formatter = new BinaryFormatter
                {
                    Binder = new SpielstandDeserializationBinder()
                };

                using (Stream stream = new FileStream(dateiname, FileMode.Open, FileAccess.Read, FileShare.Read))
                    SW.Dynamisch.Spielstand = (Spielstand)formatter.Deserialize(stream);

                AlteObjekteNachLadenAnreichern();
                return true;
            }
            catch (Exception ex)
            {
                fehler = $"Der Spielstand konnte nicht geladen werden, er scheint beschädigt zu sein. Fehler: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Liefert alle vorhandenen Spielstände, der zuletzt gespeicherte zuerst.
        /// </summary>
        [PublicAPI]
        public List<SpielstandInfo> GetSpielstaende()
        {
            var spielstaende = new List<SpielstandInfo>();

            if (!Directory.Exists(_savegamePath))
                return spielstaende;

            foreach (var datei in new DirectoryInfo(_savegamePath).GetFiles("*.dat").OrderByDescending(f => f.LastWriteTime))
                spielstaende.Add(new SpielstandInfo(Path.GetFileNameWithoutExtension(datei.Name), datei.LastWriteTime));

            return spielstaende;
        }

        [PublicAPI]
        public bool Loeschen(string name)
        {
            try
            {
                File.Delete(GetDateiname(name));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetDateiname(string name)
        {
            return Path.Combine(_savegamePath, name + ".dat");
        }

        /// <summary>
        /// Wenn Objekte um neue Felder erweitert wurden, werden diese hier für alte Spielstände
        /// gesetzt oder initialisiert (übernommen aus dem WinForms-Client).
        /// </summary>
        private static void AlteObjekteNachLadenAnreichern()
        {
            for (int i = 1; i <= SW.Dynamisch.GetAktivSpielerAnzahl(); i++)
            {
                for (int j = 1; j < SW.Statisch.GetMaxKinderAnzahl(); j++)
                {
                    var kind = SW.Dynamisch.GetHumWithID(i).GetKindX(j);

                    if (!string.IsNullOrEmpty(kind.GetKindName()) && kind.Geburtsjahr <= SW.Statisch.StartJahr)
                        kind.Geburtsjahr = SW.Dynamisch.GetAktuellesJahr() - kind.GetAlter();
                }
            }
        }
    }

    /// <summary>
    /// Ein vorhandener Spielstand mit seinem letzten Speicherzeitpunkt.
    /// </summary>
    public class SpielstandInfo
    {
        public SpielstandInfo(string name, DateTime geaendertAm)
        {
            Name = name;
            GeaendertAm = geaendertAm;
        }

        public string Name { get; }

        public DateTime GeaendertAm { get; }
    }
}
