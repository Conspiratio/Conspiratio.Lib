using System.Collections.Generic;
using System.Threading.Tasks;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.Weltkarte
{
    /// <summary>Ein Amt einer Ämter-Ebene samt aktuellem Inhaber.</summary>
    public class AemterSlotInfo
    {
        public int AmtId { get; }
        public string AmtName { get; }
        public int HolderId { get; }
        public string HolderName { get; }
        public bool Besetzt { get; }

        public AemterSlotInfo(int amtId, string amtName, int holderId, string holderName, bool besetzt)
        {
            AmtId = amtId;
            AmtName = amtName;
            HolderId = holderId;
            HolderName = holderName;
            Besetzt = besetzt;
        }
    }

    /// <summary>
    /// Kapselt die Ämter-Struktur von AemterEbene: für ein Gebiet (Stadt/Land/Reich = Stufe 0/1/2)
    /// liefert der Manager je politischer, kirchlicher und militärischer Ebene die Ämter mit ihren
    /// Inhabern. Über die gewählte Person wird – wie bei der Kontrahenten-Liste – die dem Modus
    /// entsprechende Aktion ausgeführt (Modus 8 = Prozess initiieren, Modus 13 = Hand des Henkers).
    /// </summary>
    public class AemterEbeneManager
    {
        // AmtIDs je [Stufe (0=Stadt,1=Land,2=Reich)][Ebene (0=politisch,1=kirchlich,2=militärisch)].
        // Die IDs entsprechen den statischen Amt-IDs (siehe Gebiet.GetAmtX) und der Reihenfolge im Original.
        private static readonly int[][][] AmtMatrix =
        {
            // Stufe 0 – Stadt
            new[]
            {
                new[] { 7, 4, 5, 6, 1, 2, 3 },     // Bürgermeister, Baumeister, Richter, Kämmerer, Ratsherr 1-3
                new[] { 10, 8, 9 },                // Domherr, Priester 1-2
                new[] { 16, 14, 15, 11, 12, 13 },  // Stadtkommandant, Wachkommandant, Kerkermeister, Stadtwache, Folterknecht, Henker
            },
            // Stufe 1 – Land
            new[]
            {
                new[] { 22, 20, 21, 17, 18, 19 },  // Vogt, Justizberater, Finanzberater, Geheimrat 1-3
                new[] { 27, 26, 25, 23, 24 },      // Bischof, Abt, Diakon, Kellermeister, Sakristan
                new[] { 33, 31, 32, 28, 29, 30 },  // Hauptmann, Befehlshaber, Zollmeister, Stellv. Befehlshaber, Zöllner 1-2
            },
            // Stufe 2 – Reich
            new[]
            {
                new[] { 39, 37, 38, 34, 35, 36 },  // Regent, Justizminister, Finanzminister, Hofrat 1-3
                new[] { 42, 40, 41 },              // Erzbischof, Inquisitor, Erzdiakon
                new[] { 48, 46, 47, 43, 44, 45 },  // Feldmarschall, Marschall 1-2, Offizier 1-3
            },
        };

        private readonly int _objektId;
        private readonly int _stufe;

        public AemterEbeneManager(int objektId, int stufe)
        {
            _objektId = objektId;
            _stufe = stufe;
        }

        public int Stufe => _stufe;

        /// <summary>Anzahl der Ämter-Ebenen (politisch, kirchlich, militärisch).</summary>
        public int AnzahlEbenen => 3;

        public string GetGebietsName() => SW.Dynamisch.GetGebietwithID(_objektId, _stufe).GetGebietsName();

        public string GetEbenenName(int ebene)
        {
            switch (ebene)
            {
                case 0: return "Politische Ebene";
                case 1: return "Kirchliche Ebene";
                case 2: return "Militärische Ebene";
                default: return string.Empty;
            }
        }

        /// <summary>Titelzeile ("&lt;Aktion&gt; in &lt;Gebiet&gt;") passend zum Weltkarte-Modus.</summary>
        public string GetTitel(int modus)
        {
            string prefix;
            switch (modus)
            {
                case 8: prefix = "Prozess initiieren in "; break;
                case 13: prefix = "Hand des Henkers in "; break;
                default: prefix = string.Empty; break;
            }

            return prefix + GetGebietsName();
        }

        /// <summary>Die Ämter der angegebenen Ebene mit ihren aktuellen Inhabern.</summary>
        public List<AemterSlotInfo> GetAemter(int ebene)
        {
            var result = new List<AemterSlotInfo>();
            var gebiet = SW.Dynamisch.GetGebietwithID(_objektId, _stufe);

            foreach (int amtId in AmtMatrix[_stufe][ebene])
            {
                int holder = gebiet.GetAmtX(amtId);
                bool besetzt = holder != 0;

                string amtName = SW.Statisch.GetAmtwithID(amtId)
                    .GetAmtsname(besetzt ? SW.Dynamisch.GetSpWithID(holder).GetMaennlich() : true);
                string holderName = besetzt ? SW.Dynamisch.GetSpWithID(holder).GetName() : "(unbesetzt)";

                result.Add(new AemterSlotInfo(amtId, amtName, holder, holderName, besetzt));
            }

            return result;
        }

        /// <summary>
        /// Führt die dem Modus entsprechende Aktion auf dem gewählten Amtsinhaber aus
        /// (dieselbe Logik wie bei der Kontrahenten-Liste).
        /// </summary>
        public Task PersonWasMachen(int id, int modus) => new KontrahentenManager().PersonWasMachen(id, modus);
    }
}
