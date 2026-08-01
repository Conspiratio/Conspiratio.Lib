using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Serialisiert Spielstände feldbasiert wie früher der BinaryFormatter: Alle Instanzfelder
    /// (auch private) werden geschrieben und gelesen, Objekte werden ohne Konstruktoraufruf
    /// erzeugt. Dadurch muss das Domänenmodell für das JSON-Format nicht verändert werden.
    /// </summary>
    internal sealed class SpielstandContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            // Objekte wie der BinaryFormatter ohne Konstruktoraufruf erzeugen
            contract.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
            contract.DefaultCreatorNonPublic = false;
            contract.OverrideCreator = null;
            contract.CreatorParameters.Clear();

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = new List<JsonProperty>();
            var vergebeneNamen = new HashSet<string>();

            for (var typ = type; typ != null && typ != typeof(object); typ = typ.BaseType)
            {
                foreach (var feld in typ.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (feld.IsNotSerialized)
                        continue;

                    var property = CreateProperty(feld, memberSerialization);

                    // Backing-Fields von Auto-Properties lesbar benennen ("<Name>k__BackingField" -> "Name")
                    string name = feld.Name;

                    if (name.StartsWith("<", StringComparison.Ordinal))
                    {
                        int ende = name.IndexOf('>');

                        if (ende > 1)
                            name = name.Substring(1, ende - 1);
                    }

                    // Namenskollisionen durch verdeckte Felder in Basisklassen eindeutig machen
                    if (!vergebeneNamen.Add(name))
                    {
                        name = typ.Name + "." + name;
                        vergebeneNamen.Add(name);
                    }

                    property.PropertyName = name;
                    property.Readable = true;
                    property.Writable = true;
                    property.ValueProvider = new ReflectionValueProvider(feld);

                    properties.Add(property);
                }
            }

            return properties;
        }
    }

    /// <summary>
    /// Erlaubt beim Laden von Spielständen nur Typen aus der Conspiratio.Lib-Assembly — Spielstände sind
    /// von Hand editierbare JSON-Dateien und dürfen keine fremden Typen instanziieren können.
    /// </summary>
    internal sealed class SpielstandJsonTypBinder : ISerializationBinder
    {
        public Type BindToType(string assemblyName, string typeName)
        {
            var assembly = typeof(SpeicherManager).Assembly;

            // Direkt in der Lib-Assembly auflösen. Deckt sowohl die "Conspiratio.Lib.*"-Typen ab als auch
            // jene, die aus Kompatibilitätsgründen bewusst im alten Namespace "Conspiratio.Kampf" verblieben
            // sind (z. B. die Stützpunkt-Einheiten wie ZollSoeldner) und deren Namen so im Spielstand steht.
            var typ = assembly.GetType(typeName, false);

            // Sonst über die Tabelle den aktuellen Namen ermitteln (für tatsächlich umbenannte/verschobene
            // Typen aus sehr alten Spielständen, z. B. "Conspiratio.Spieler").
            if (typ == null && SpielstandDeserializationBinder.TypeMappings.TryGetValue(typeName, out string neuerTypname))
                typ = assembly.GetType(neuerTypname, false);

            if (typ == null)
                throw new SerializationException($"Unbekannter Typ im Spielstand: {typeName}");

            // Sicherheit: nur die eigenen Spieltypen zulassen (die Auflösung ist ohnehin auf die
            // Conspiratio.Lib-Assembly beschränkt, dieser Prüfschritt schließt fremde Namespaces zusätzlich aus).
            if (typ.FullName == null || !typ.FullName.StartsWith("Conspiratio.", StringComparison.Ordinal))
                throw new SerializationException($"Unerlaubter Typ im Spielstand: {typeName}");

            return typ;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
}
