using System.Collections.Generic;
using System.Threading.Tasks;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.Weltkarte
{
    /// <summary>Ein möglicher Kontrahent (Ziel) für eine Weltkarte-Aktion.</summary>
    public class KontrahentInfo
    {
        public int Id { get; }
        public string Name { get; }
        public bool IstMensch { get; }

        public KontrahentInfo(int id, string name, bool istMensch)
        {
            Id = id;
            Name = name;
            IstMensch = istMensch;
        }
    }

    /// <summary>
    /// Kapselt die Logik von KontrahentenForm/UI.PersonWasMachen für die Privilegien-Modi der Weltkarte:
    /// liefert die wählbaren Kontrahenten (menschliche Mitspieler zuerst, dann die KI) und führt die
    /// zielgerichtete Aktion aus – Modus 8 = Prozess initiieren, Modus 13 = Hand des Henkers.
    /// </summary>
    public class KontrahentenManager
    {
        /// <summary>
        /// Die wählbaren Kontrahenten: zuerst die menschlichen Mitspieler (ohne den aktiven Spieler),
        /// danach alle KI-Spieler.
        /// </summary>
        public List<KontrahentInfo> GetKontrahenten()
        {
            var list = new List<KontrahentInfo>();

            for (int i = 1; i <= SW.Dynamisch.GetAktivSpielerAnzahl(); i++)
            {
                if (i != SW.Dynamisch.GetAktiverSpieler())
                    list.Add(new KontrahentInfo(i, SW.Dynamisch.GetSpWithID(i).GetCompleteNameOhneTitel(), true));
            }

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                list.Add(new KontrahentInfo(i, SW.Dynamisch.GetSpWithID(i).GetCompleteNameOhneTitel(), false));

            return list;
        }

        /// <summary>
        /// Führt die dem Modus entsprechende Aktion auf den gewählten Kontrahenten aus
        /// (Migration von UI.PersonWasMachen, beschränkt auf die Privilegien-Modi 8 und 13).
        /// </summary>
        public async Task PersonWasMachen(int id, int modus)
        {
            if (id == 0)
                return;

            if (id == SW.Dynamisch.GetAktiverSpieler())
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr könnt diese Aktion nicht auf Euch selbst anwenden");
                return;
            }

            switch (modus)
            {
                case 0: // Beziehungen pflegen (Hinterzimmer)
                    SW.UI.BeziehungPflegen.ShowDialog(id);
                    break;
                case 1: // Sabotage (Hinterzimmer)
                    await SW.Dynamisch.Sabotage(id);
                    break;
                case 3: // Spionage (Hinterzimmer)
                    await SW.Dynamisch.Spionage(id);
                    break;
                case 4: // Ermordung (Hinterzimmer)
                    await SW.Dynamisch.Ermordung(id);
                    break;
                case 8: // Prozess initiieren
                    await SW.Dynamisch.ProzessInitiieren(id);
                    break;
                case 13: // Hand des Henkers
                    SW.Dynamisch.HenkersHand(id);
                    break;
                default:
                    // Beziehungen (0), Sabotage (1), Anschwärzen (2), Spionage (3), Erpressung (5)
                    // werden in eigenen Migrationsschritten ergänzt.
                    SW.Dynamisch.BelTextAnzeigen("Diese Aktion ist noch nicht verfügbar.");
                    break;
            }
        }
    }
}
