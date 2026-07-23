using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Titel
{
    /// <summary>
    /// Kapselt die Titelverleihung an den aktiven Spieler (Migration von TitelVerleihen/TitelVerleihForm aus
    /// dem WinForms-Client). Ob ein Titel ansteht, wird zuvor über <see cref="DynamischeSpieldaten.VersuchTitelVerleihen"/>
    /// bestimmt (setzt "BekamTitel"). Steht ein Titel an und gibt es einen Regenten, liefert dieser Manager
    /// den Urkundentext, setzt den neuen Titel des Spielers und quittiert die anstehende Verleihung.
    /// </summary>
    public class TitelVerleihungManager
    {
        /// <summary>
        /// Prüft, ob dem Spieler ein Titel verliehen werden soll. Ohne amtierenden Regenten wird die
        /// Verleihung – wie im Original – auf später verschoben.
        /// </summary>
        public bool StehtTitelverleihungAn()
        {
            return SW.Dynamisch.GetAktHum().GetBekamTitelX() != 0 && SW.Dynamisch.GetReichWithID(1).GetRegent() != 0;
        }

        /// <summary>
        /// Vollzieht die Titelverleihung: erstellt den Urkundentext, setzt den neuen Titel des Spielers und
        /// setzt den "BekamTitel"-Vermerk zurück. Vor dem Aufruf sollte <see cref="StehtTitelverleihungAn"/> gelten.
        /// </summary>
        public TitelverleihungErgebnis Vollziehe()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int titelId = spieler.GetBekamTitelX();

            var regent = SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetReichWithID(1).GetRegent());
            var titel = SW.Statisch.GetTitelX(titelId);
            bool maennlich = spieler.GetMaennlich();
            string titelName = titel.GetName(maennlich);

            string text = "Wir, " + regent.GetKompletterName() + ", verfügen hiermit, dass Ihr, " + spieler.GetName() +
                          ", Euch fortan\n\n\"" + titelName + "\"\n\nnennen dürft.\n\n" + regent.GetKompletterName();

            spieler.SetTitel(titelId);
            spieler.SetBekamTitelX(0);

            return new TitelverleihungErgebnis
            {
                UrkundenText = text,
                TitelName = titelName,
                TitelTyp = titel.GetType().Name,
                Maennlich = maennlich
            };
        }
    }

    /// <summary>Das Ergebnis einer Titelverleihung mit dem anzuzeigenden Urkundentext.</summary>
    public class TitelverleihungErgebnis
    {
        public string UrkundenText { get; set; }
        public string TitelName { get; set; }

        /// <summary>Der Typ-Name des verliehenen Titels (z. B. "Graf") – dient dem Client zur Auswahl der Sprachausgabe.</summary>
        public string TitelTyp { get; set; }

        public bool Maennlich { get; set; }
    }
}
