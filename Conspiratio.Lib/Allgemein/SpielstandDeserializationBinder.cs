using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Klasse um bei der Deserialization über den BinaryFormatter das Binding der Types zur Assembly zu übersteuern.<br/>
    /// Wird benötigt, da einiges Types (also Klassen, Enums usw.) aus der Conspiratio Assembly in die Conspiratio.Lib Assembly ausgelagert wurden, in den alten Spielständen
    /// steht aber bei diesen Types noch die Conspiratio Assembly drin (der BinaryFormatter schreibt auch diese Info bei der Serialisierung).<br/><br/>
    /// Für weiter Infos siehe https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.serializationbinder?redirectedfrom=MSDN&view=netframework-4.6.2
    /// </summary>
    public sealed class SpielstandDeserializationBinder : SerializationBinder
    {
        // Key = alter Typename (vor der Auslagerung in die Conspiratio.Lib-Assembly), Value = aktueller Typename.
        // Wird sowohl vom BinaryFormatter-Binder (diese Klasse) als auch vom JSON-Binder (SpielstandJsonTypBinder)
        // genutzt, damit ältere Spielstände in beiden Formaten geladen werden können.
        internal static readonly Dictionary<string, string> TypeMappings = new Dictionary<string, string>
        {
            { "Conspiratio.Rohstoff", "Conspiratio.Lib.Gameplay.Rohstoffe.Rohstoff" },
            { "Conspiratio.Enumeratoren+StuetzpunktArt", "Conspiratio.Lib.Gameplay.Kampf.EnumStuetzpunktArt" },
            { "Conspiratio.Enumeratoren+KampfArt", "Conspiratio.Lib.Gameplay.Kampf.EnumKampfArt" },
            { "Conspiratio.Enumeratoren+AktionsartZollburg", "Conspiratio.Lib.Gameplay.Kampf.EnumAktionsartZollburg" },
            { "Conspiratio.Enumeratoren+AktionsartRaeuberlager", "Conspiratio.Lib.Gameplay.Kampf.EnumAktionsartRaeuberlager" },
            { "Conspiratio.Classes.Privilegien.FestGeben.Fest", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.Fest" },
            { "Conspiratio.Classes.Privilegien.FestGeben.EnumFestMusiker", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.EnumFestMusiker" },
            { "Conspiratio.Classes.Privilegien.FestGeben.EnumFestGroesse", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.EnumFestGroesse" },
            { "Conspiratio.AktiveSpionagen", "Conspiratio.Lib.Gameplay.Hinterzimmer.AktiveSpionagen" },
            { "Conspiratio.AktiveSabotagen", "Conspiratio.Lib.Gameplay.Hinterzimmer.AktiveSabotagen" },
            { "Conspiratio.Gebiet", "Conspiratio.Lib.Gameplay.Gebiete.Gebiet" },
            { "Conspiratio.Kampf.Einheit", "Conspiratio.Lib.Gameplay.Kampf.Einheiten.Einheit" },
            { "Conspiratio.Kampf.Kampf", "Conspiratio.Lib.Gameplay.Kampf.Kampf" },
            { "Conspiratio.Kampf.KampfKarawane", "Conspiratio.Lib.Gameplay.Kampf.KampfKarawane" },
            { "Conspiratio.Kampf.KampfErgebnis", "Conspiratio.Lib.Gameplay.Kampf.KampfErgebnis" },
            { "Conspiratio.Classes.Ereignisse.Ereigniszeitpunkt", "Conspiratio.Lib.Gameplay.Ereignisse.Ereigniszeitpunkt" },
            { "Conspiratio.Land", "Conspiratio.Lib.Gameplay.Gebiete.Land" },
            { "Conspiratio.Reich", "Conspiratio.Lib.Gameplay.Gebiete.Reich" },
            { "Conspiratio.Kind", "Conspiratio.Lib.Gameplay.Personen.Kind" },
            { "Conspiratio.SpielerStatistik", "Conspiratio.Lib.Gameplay.Personen.SpielerStatistik" },
            { "Conspiratio.Amtsenthebung", "Conspiratio.Lib.Gameplay.Privilegien.Amtsenthebung" },
            { "Conspiratio.Gerichtsverhandlung", "Conspiratio.Lib.Gameplay.Justiz.Gerichtsverhandlung" },
            { "Conspiratio.AmtsInfo", "Conspiratio.Lib.Gameplay.Schreibstube.AmtsInfo" },
            { "Conspiratio.Kredite", "Conspiratio.Lib.Gameplay.Schreibstube.Kredit" },
            { "Conspiratio.WahlAbhalten", "Conspiratio.Lib.Gameplay.Schreibstube.WahlAbhalten" },
            { "Conspiratio.SpHatHaus", "Conspiratio.Lib.Gameplay.Wohnsitz.SpHatHaus" },
            { "Conspiratio.SpHatWerkstaetten", "Conspiratio.Lib.Gameplay.Niederlassung.SpHatWerkstaetten" },
            { "Conspiratio.Classes.Spielstand", "Conspiratio.Lib.Gameplay.Spielwelt.Spielstand" },
            { "Conspiratio.Spieler", "Conspiratio.Lib.Gameplay.Personen.Spieler" },
            { "Conspiratio.KISpieler", "Conspiratio.Lib.Gameplay.Personen.KISpieler" },
            { "Conspiratio.HumSpieler", "Conspiratio.Lib.Gameplay.Personen.HumSpieler" },
            { "Conspiratio.Produktionsslot", "Conspiratio.Lib.Gameplay.Niederlassung.Produktionsslot" },
            { "Conspiratio.Stadt", "Conspiratio.Lib.Gameplay.Gebiete.Stadt" },
            { "Conspiratio.Kampf.Stuetzpunkt", "Conspiratio.Lib.Gameplay.Kampf.Stuetzpunkt" },
            { "Conspiratio.Kampf.Landsicherheit", "Conspiratio.Lib.Gameplay.Kampf.Landsicherheit" },
            { "Conspiratio.Kampf.StuetzpunktAktion", "Conspiratio.Lib.Gameplay.Kampf.StuetzpunktAktion" },
            { "Conspiratio.Kampf.Raeuberlager", "Conspiratio.Lib.Gameplay.Kampf.Raeuberlager" },
            { "Conspiratio.Kampf.RaeuberlagerAktion", "Conspiratio.Lib.Gameplay.Kampf.RaeuberlagerAktion" },
            { "Conspiratio.Kampf.RaubBombenleger", "Conspiratio.Lib.Gameplay.Kampf.RaubBombenleger" },
            { "Conspiratio.Kampf.RaubKanonier", "Conspiratio.Lib.Gameplay.Kampf.RaubKanonier" },
            { "Conspiratio.Kampf.RaubRaeuber", "Conspiratio.Lib.Gameplay.Kampf.RaubRaeuber" },
            { "Conspiratio.Kampf.RaubSchuetze", "Conspiratio.Lib.Gameplay.Kampf.RaubSchuetze" },
            { "Conspiratio.Kampf.Zollburg", "Conspiratio.Lib.Gameplay.Kampf.Zollburg" },
            { "Conspiratio.Kampf.ZollburgAktion", "Conspiratio.Lib.Gameplay.Kampf.ZollburgAktion" },
            { "Conspiratio.Kampf.ZollKanonier", "Conspiratio.Lib.Gameplay.Kampf.ZollKanonier" },
            { "Conspiratio.Kampf.ZollMusketier", "Conspiratio.Lib.Gameplay.Kampf.ZollMusketier" },
            { "Conspiratio.Kampf.ZollOffizier", "Conspiratio.Lib.Gameplay.Kampf.ZollOffizier" },
            { "Conspiratio.Kampf.ZollSoeldner", "Conspiratio.Lib.Gameplay.Kampf.ZollSoeldner" },
        };

        private readonly Dictionary<string, string> typeMappings = TypeMappings;

        public override Type BindToType(string assemblyName, string typeName)
        {
            // Der Zweck dieser Methode ist die Verhinderung einer SerializationException, weil Types, die ausgelagert wurden, nicht mehr an der alten Stelle gefunden werden.
            // Es kommt dann bei der Deserialisierung zu einer Meldung wie: Auf das Objekt mit der ID 15 wurde verwiesen, aber es ist nicht vorhanden.

            var typeToDeserialize = Type.GetType(typeName, false);  // Type anhand des Namens in der aktuellen Assembly suchen

            if (typeToDeserialize == null)
                typeToDeserialize = Type.GetType(Assembly.CreateQualifiedName("Conspiratio.Lib", typeName));  // Wenn der Type nicht gefunden wurde, dann suche in der Assembly "Conspiratio.Lib"

            if (typeToDeserialize == null)
            {
                // Es hat sich der Namespace oder der Name des Types verändert, ermittle den neuen Namen
                if (typeMappings.TryGetValue(typeName, out string newTypeName))
                    typeName = newTypeName;

                typeToDeserialize = Type.GetType(typeName, false);  // Type anhand des neuen Namens in der aktuellen Assembly suchen (wurde evtl. umbenannt oder in anderen Namespace verschoben)
            }

            if (typeToDeserialize == null)
                typeToDeserialize = Type.GetType(Assembly.CreateQualifiedName("Conspiratio.Lib", typeName));  // Wenn der Type nicht gefunden wurde, dann suche in der Assembly "Conspiratio.Lib"

            // Sonderbehandlung für Listen
            if (typeToDeserialize == null && assemblyName.Contains("mscorlib") && typeName.Contains("System.Collections.Generic.List`1[["))  
            {
                string oldTypeName = typeName.Replace("System.Collections.Generic.List`1[[", "");
                oldTypeName = oldTypeName.Substring(0, oldTypeName.IndexOf(","));

                if (typeMappings.TryGetValue(oldTypeName, out string newTypeName))
                {
                    string typeNamePattern = "System.Collections.Generic.List`1[[{0}{1}]]";

                    typeName = string.Format(typeNamePattern, newTypeName, "");
                    typeToDeserialize = Type.GetType(typeName, false);  // Type anhand des neuen Namens in der aktuellen Assembly suchen (wurde evtl. umbenannt oder in anderen Namespace verschoben)

                    if (typeToDeserialize == null)
                    {
                        typeName = string.Format(typeNamePattern, newTypeName, ", Conspiratio.Lib");
                        typeToDeserialize = Type.GetType(typeName);  // Wenn der Type nicht gefunden wurde, dann suche in der Assembly "Conspiratio.Lib"
                    }
                }
            }

            if (typeToDeserialize == null)
                throw new SerializationException($"Type '{typeName}'. Bitte melde dich mit diesem Spielstand im Forum oder unter mail@conspiratio.net");

            return typeToDeserialize;
        }
    }
}
