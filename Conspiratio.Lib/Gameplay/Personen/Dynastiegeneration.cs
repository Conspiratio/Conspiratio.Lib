using System;
using System.Collections.Generic;

namespace Conspiratio.Lib.Gameplay.Personen
{
    /// <summary>
    /// Eine Person in der Ahnentafel (Oberhaupt, Ehepartner oder Kind) mit Geburts- und – sofern
    /// verstorben – Todesjahr. <see cref="Todesjahr"/> == 0 bedeutet „lebt noch" (das aktuelle Oberhaupt).
    /// </summary>
    [Serializable]
    public class AhnPerson
    {
        public string Name { get; set; }
        public int Geburtsjahr { get; set; }
        public int Todesjahr { get; set; }
        public bool Maennlich { get; set; }

        public AhnPerson()
        {
        }

        public AhnPerson(string name, int geburtsjahr, int todesjahr, bool maennlich)
        {
            Name = name;
            Geburtsjahr = geburtsjahr;
            Todesjahr = todesjahr;
            Maennlich = maennlich;
        }
    }

    /// <summary>
    /// Eine Generation der Dynastie in der Ahnentafel: das (verstorbene) Oberhaupt, sein Ehepartner und
    /// seine Kinder zum Zeitpunkt des Erbfalls. <see cref="ErbeKindIndex"/> verweist auf das Kind (Index in
    /// <see cref="Kinder"/>), das die Dynastie fortführte; <see cref="EhepartnerErbte"/> ist true, wenn
    /// stattdessen der Ehepartner die Dynastie fortführte.
    /// </summary>
    [Serializable]
    public class Dynastiegeneration
    {
        public AhnPerson Oberhaupt { get; set; }
        public AhnPerson Ehepartner { get; set; }
        public List<AhnPerson> Kinder { get; set; } = new List<AhnPerson>();
        public int ErbeKindIndex { get; set; } = -1;
        public bool EhepartnerErbte { get; set; }

        public Dynastiegeneration()
        {
        }
    }
}
