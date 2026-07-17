using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Speichert und lädt Spielstände als offene, von Hand editierbare JSON-Dateien (*.json).
    /// Alte Spielstände des WinForms-Clients (*.dat, BinaryFormatter) werden beim Laden
    /// konvertiert, sofern die Runtime den BinaryFormatter noch unterstützt (unter .NET 9+
    /// wurde er entfernt — dort alte Spielstände zuerst mit dem WinForms-Client laden und neu speichern).
    /// </summary>
    public class SpeicherManager
    {
        private readonly string _savegamePath;

        public SpeicherManager(string savegamePath)
        {
            _savegamePath = savegamePath;
        }

        /// <summary>
        /// Speichert den aktuellen Spielstand als JSON-Datei unter dem angegebenen Namen.
        /// </summary>
        [PublicAPI]
        public bool Speichern(string name, out string fehler)
        {
            fehler = "";

            try
            {
                Directory.CreateDirectory(_savegamePath);

                string json = JsonConvert.SerializeObject(SW.Dynamisch.Spielstand, typeof(Spielstand), ErstelleJsonEinstellungen());
                File.WriteAllText(GetJsonDateiname(name), json);

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
                File.Delete(GetJsonDateiname(SW.Dynamisch.SpielName + "_" + (SW.Dynamisch.GetAktuellesJahr() - 2)));
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

            if (File.Exists(GetJsonDateiname(name)))
                return LadeJson(name, out fehler);

            if (File.Exists(GetDatDateiname(name)))
                return LadeAltesFormat(name, out fehler);

            fehler = $"Es ist kein gültiger Spielstand mit diesem Namen vorhanden: {name}";
            return false;
        }

        private bool LadeJson(string name, out string fehler)
        {
            fehler = "";

            try
            {
                string json = File.ReadAllText(GetJsonDateiname(name));
                SW.Dynamisch.Spielstand = JsonConvert.DeserializeObject<Spielstand>(json, ErstelleJsonEinstellungen());

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
        /// Lädt einen alten BinaryFormatter-Spielstand des WinForms-Clients und speichert ihn
        /// direkt im neuen JSON-Format (funktioniert nur auf Runtimes mit BinaryFormatter).
        /// </summary>
        private bool LadeAltesFormat(string name, out string fehler)
        {
            fehler = "";

            try
            {
#pragma warning disable SYSLIB0011
                IFormatter formatter = new BinaryFormatter
                {
                    Binder = new SpielstandDeserializationBinder()
                };

                using (Stream stream = new FileStream(GetDatDateiname(name), FileMode.Open, FileAccess.Read, FileShare.Read))
                    SW.Dynamisch.Spielstand = (Spielstand)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011

                AlteObjekteNachLadenAnreichern();

                // In das neue Format konvertieren und die alte Datei entfernen
                if (Speichern(name, out fehler))
                {
                    try
                    {
                        File.Delete(GetDatDateiname(name));
                    }
                    catch
                    {
                        // Ein fehlgeschlagenes Aufräumen ist unkritisch
                    }
                }

                return true;
            }
            catch (NotSupportedException)
            {
                fehler = "Dieser Spielstand liegt im alten Format des WinForms-Clients vor und kann hier nicht geladen werden.\n" +
                         "Ladet und speichert ihn einmal im WinForms-Client, um ihn in das neue Format zu übernehmen.";
                return false;
            }
            catch (Exception ex)
            {
                fehler = $"Der Spielstand konnte nicht geladen werden, er scheint beschädigt zu sein. Fehler: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Liefert alle vorhandenen Spielstände (neues und altes Format), der zuletzt gespeicherte zuerst.
        /// </summary>
        [PublicAPI]
        public List<SpielstandInfo> GetSpielstaende()
        {
            var spielstaende = new List<SpielstandInfo>();

            if (!Directory.Exists(_savegamePath))
                return spielstaende;

            var verzeichnis = new DirectoryInfo(_savegamePath);

            foreach (var datei in verzeichnis.GetFiles("*.json").Concat(verzeichnis.GetFiles("*.dat")).OrderByDescending(f => f.LastWriteTime))
                spielstaende.Add(new SpielstandInfo(Path.GetFileNameWithoutExtension(datei.Name), datei.LastWriteTime));

            return spielstaende;
        }

        [PublicAPI]
        public bool Loeschen(string name)
        {
            try
            {
                File.Delete(GetJsonDateiname(name));
                File.Delete(GetDatDateiname(name));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetJsonDateiname(string name)
        {
            return Path.Combine(_savegamePath, name + ".json");
        }

        private string GetDatDateiname(string name)
        {
            return Path.Combine(_savegamePath, name + ".dat");
        }

        private static JsonSerializerSettings ErstelleJsonEinstellungen()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new SpielstandContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new SpielstandJsonTypBinder(),
                Formatting = Formatting.None,  // Kompakt; zum Anschauen/Editieren einfach in einem Editor mit JSON-Formatierung öffnen
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
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
