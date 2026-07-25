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
        Dictionary<string, string> typeMappings = new Dictionary<string, string>();  // Key = alter Typename, Value = neuer Typename

        public SpielstandDeserializationBinder() : base()
        {
            typeMappings.Add("Conspiratio.Rohstoff", "Conspiratio.Lib.Gameplay.Rohstoffe.Rohstoff");
            typeMappings.Add("Conspiratio.Enumeratoren+StuetzpunktArt", "Conspiratio.Lib.Gameplay.Kampf.EnumStuetzpunktArt");
            typeMappings.Add("Conspiratio.Enumeratoren+KampfArt", "Conspiratio.Lib.Gameplay.Kampf.EnumKampfArt");
            typeMappings.Add("Conspiratio.Enumeratoren+AktionsartZollburg", "Conspiratio.Lib.Gameplay.Kampf.EnumAktionsartZollburg");
            typeMappings.Add("Conspiratio.Enumeratoren+AktionsartRaeuberlager", "Conspiratio.Lib.Gameplay.Kampf.EnumAktionsartRaeuberlager");
            typeMappings.Add("Conspiratio.Classes.Privilegien.FestGeben.Fest", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.Fest");
            typeMappings.Add("Conspiratio.Classes.Privilegien.FestGeben.EnumFestMusiker", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.EnumFestMusiker");
            typeMappings.Add("Conspiratio.Classes.Privilegien.FestGeben.EnumFestGroesse", "Conspiratio.Lib.Gameplay.Privilegien.FestGeben.EnumFestGroesse");
            typeMappings.Add("Conspiratio.AktiveSpionagen", "Conspiratio.Lib.Gameplay.Hinterzimmer.AktiveSpionagen");
            typeMappings.Add("Conspiratio.AktiveSabotagen", "Conspiratio.Lib.Gameplay.Hinterzimmer.AktiveSabotagen");
            typeMappings.Add("Conspiratio.Gebiet", "Conspiratio.Lib.Gameplay.Gebiete.Gebiet");
            typeMappings.Add("Conspiratio.Kampf.Einheit", "Conspiratio.Lib.Gameplay.Kampf.Einheiten.Einheit");
            typeMappings.Add("Conspiratio.Kampf.Kampf", "Conspiratio.Lib.Gameplay.Kampf.Kampf");
            typeMappings.Add("Conspiratio.Kampf.KampfKarawane", "Conspiratio.Lib.Gameplay.Kampf.KampfKarawane");
            typeMappings.Add("Conspiratio.Kampf.KampfErgebnis", "Conspiratio.Lib.Gameplay.Kampf.KampfErgebnis");
            typeMappings.Add("Conspiratio.Classes.Ereignisse.Ereigniszeitpunkt", "Conspiratio.Lib.Gameplay.Ereignisse.Ereigniszeitpunkt");
            typeMappings.Add("Conspiratio.Land", "Conspiratio.Lib.Gameplay.Gebiete.Land");
            typeMappings.Add("Conspiratio.Reich", "Conspiratio.Lib.Gameplay.Gebiete.Reich");
            typeMappings.Add("Conspiratio.Kind", "Conspiratio.Lib.Gameplay.Personen.Kind");
            typeMappings.Add("Conspiratio.SpielerStatistik", "Conspiratio.Lib.Gameplay.Personen.SpielerStatistik");
            typeMappings.Add("Conspiratio.Amtsenthebung", "Conspiratio.Lib.Gameplay.Privilegien.Amtsenthebung");
            typeMappings.Add("Conspiratio.Gerichtsverhandlung", "Conspiratio.Lib.Gameplay.Justiz.Gerichtsverhandlung");
            typeMappings.Add("Conspiratio.AmtsInfo", "Conspiratio.Lib.Gameplay.Schreibstube.AmtsInfo");
            typeMappings.Add("Conspiratio.Kredite", "Conspiratio.Lib.Gameplay.Schreibstube.Kredit");
            typeMappings.Add("Conspiratio.WahlAbhalten", "Conspiratio.Lib.Gameplay.Schreibstube.WahlAbhalten");
            typeMappings.Add("Conspiratio.SpHatHaus", "Conspiratio.Lib.Gameplay.Wohnsitz.SpHatHaus");
            typeMappings.Add("Conspiratio.SpHatWerkstaetten", "Conspiratio.Lib.Gameplay.Niederlassung.SpHatWerkstaetten");
            typeMappings.Add("Conspiratio.Classes.Spielstand", "Conspiratio.Lib.Gameplay.Spielwelt.Spielstand");
            typeMappings.Add("Conspiratio.Spieler", "Conspiratio.Lib.Gameplay.Personen.Spieler");
            typeMappings.Add("Conspiratio.KISpieler", "Conspiratio.Lib.Gameplay.Personen.KISpieler");
            typeMappings.Add("Conspiratio.HumSpieler", "Conspiratio.Lib.Gameplay.Personen.HumSpieler");
            typeMappings.Add("Conspiratio.Produktionsslot", "Conspiratio.Lib.Gameplay.Niederlassung.Produktionsslot");
            typeMappings.Add("Conspiratio.Stadt", "Conspiratio.Lib.Gameplay.Gebiete.Stadt");
            typeMappings.Add("Conspiratio.Kampf.Stuetzpunkt", "Conspiratio.Lib.Gameplay.Kampf.Stuetzpunkt");
            typeMappings.Add("Conspiratio.Kampf.Landsicherheit", "Conspiratio.Lib.Gameplay.Kampf.Landsicherheit");
            typeMappings.Add("Conspiratio.Kampf.StuetzpunktAktion", "Conspiratio.Lib.Gameplay.Kampf.StuetzpunktAktion");
            typeMappings.Add("Conspiratio.Kampf.Raeuberlager", "Conspiratio.Lib.Gameplay.Kampf.Raeuberlager");
            typeMappings.Add("Conspiratio.Kampf.RaeuberlagerAktion", "Conspiratio.Lib.Gameplay.Kampf.RaeuberlagerAktion");
            typeMappings.Add("Conspiratio.Kampf.RaubBombenleger", "Conspiratio.Lib.Gameplay.Kampf.RaubBombenleger");
            typeMappings.Add("Conspiratio.Kampf.RaubKanonier", "Conspiratio.Lib.Gameplay.Kampf.RaubKanonier");
            typeMappings.Add("Conspiratio.Kampf.RaubRaeuber", "Conspiratio.Lib.Gameplay.Kampf.RaubRaeuber");
            typeMappings.Add("Conspiratio.Kampf.RaubSchuetze", "Conspiratio.Lib.Gameplay.Kampf.RaubSchuetze");
            typeMappings.Add("Conspiratio.Kampf.Zollburg", "Conspiratio.Lib.Gameplay.Kampf.Zollburg");
            typeMappings.Add("Conspiratio.Kampf.ZollburgAktion", "Conspiratio.Lib.Gameplay.Kampf.ZollburgAktion");
            typeMappings.Add("Conspiratio.Kampf.ZollKanonier", "Conspiratio.Lib.Gameplay.Kampf.ZollKanonier");
            typeMappings.Add("Conspiratio.Kampf.ZollMusketier", "Conspiratio.Lib.Gameplay.Kampf.ZollMusketier");
            typeMappings.Add("Conspiratio.Kampf.ZollOffizier", "Conspiratio.Lib.Gameplay.Kampf.ZollOffizier");
            typeMappings.Add("Conspiratio.Kampf.ZollSoeldner", "Conspiratio.Lib.Gameplay.Kampf.ZollSoeldner");
        }

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
