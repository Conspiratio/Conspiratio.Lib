using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivVergifteterWein : Privileg
    {
        public PrivVergifteterWein() : base("Vergifteter Wein", 17)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Als Kellermeister steht Euch der Zugang zu den Getränkevorräten mancher Konkurrenten offen. Dadurch könnt Ihr vergünstigt eine Ermordung verüben.");

            // Ist noch keine Vergiftung vorbereitet, öffnet sich die Personen-Karte zur Zielauswahl
            // (Modus 12). Dort ausgeführt wird die Vergiftung über KontrahentenManager.PersonWasMachen →
            // SW.Dynamisch.WeinVergiften; die Auflösung folgt zum Zugende (FuehreVergiftetenWeinDurch).
            if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetVergiftetWeinVonKISpielerID() == 0)
            {
                SW.UI.PolitischeWeltkarteDialog.ShowDialogModus(12);
            }
            else
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr habt in diesem Jahr bereits Vorbereitungen für einen Anschlag getroffen.");
            }
        }
    }
}
