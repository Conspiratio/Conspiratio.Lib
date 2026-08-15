using System;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.BauwerkStiften
{
    /// <summary>
    /// Kapselt die Logik des Privilegs "Bauwerk stiften" (Migration von BauwerkStiftenForm):
    /// die stiftbaren Bauwerke samt Preisen, die Auswahl der Stadt sowie das eigentliche Stiften
    /// (Permaansehen erhöhen, Taler abziehen). Die Bestätigungsabfrage bleibt Sache des Frontends.
    /// </summary>
    public class BauwerkStiftenManager
    {
        private readonly int[] _preise = { 5000, 5000, 5000, 5000 };
        private readonly string[] _bauwerke = { "eine Kirche", "einen Kerker", "eine Feuerwehr", "ein Hospital" };

        /// <summary>Die aktuell für die Stiftung gewählte Stadt.</summary>
        public int AktiveStadtID { get; private set; } = 1;

        /// <summary>Anzahl der stiftbaren Bauwerke.</summary>
        public int AnzahlBauwerke => _bauwerke.Length;

        /// <summary>Name des Bauwerks am angegebenen (0-basierten) Index, z. B. "eine Kirche".</summary>
        public string GetBauwerkName(int index) => _bauwerke[index];

        /// <summary>Preis des Bauwerks am angegebenen (0-basierten) Index.</summary>
        public int GetPreis(int index) => _preise[index];

        /// <summary>Beschriftung "&lt;Bauwerk&gt; für &lt;Preis&gt;" für die Anzeige.</summary>
        public string GetBauwerkBeschriftung(int index) =>
            _bauwerke[index] + " für " + _preise[index].ToStringGeld();

        /// <summary>Name der aktuell gewählten Stadt (Gebietsname).</summary>
        public string GetStadtName() => SW.Dynamisch.GetStadtwithID(AktiveStadtID).GetGebietsName();

        /// <summary>Schaltet auf die nächste Stadt weiter (zyklisch, wie im Original).</summary>
        public void SetNextStadt()
        {
            AktiveStadtID++;
            if (AktiveStadtID >= SW.Statisch.GetMaxStadtID())
                AktiveStadtID = 1;
        }

        /// <summary>Prüft (ohne Nebenwirkung), ob der aktive Spieler das Bauwerk bezahlen kann.</summary>
        public bool KannBezahlen(int index) =>
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetTaler() >= _preise[index];

        /// <summary>Meldungstext, wenn das Geld für das Bauwerk nicht reicht (wie im Original).</summary>
        public string GetNichtGenugGoldMeldung(int index) =>
            "Ihr besitzt die " + _preise[index].ToStringGeld(false) + "\n Taler für dieses\nVorhaben nicht.";

        /// <summary>Bestätigungsfrage für die Stiftung des Bauwerks am angegebenen Index.</summary>
        public string GetBestaetigungsfrage(int index) =>
            "Wollt Ihr wirklich für " + _preise[index].ToStringGeld() +
            "\nder Stadt " + GetStadtName() + " " + _bauwerke[index] + " stiften?";

        /// <summary>
        /// Führt die Stiftung des Bauwerks am angegebenen Index in der aktuell gewählten Stadt aus:
        /// erhöht das Permaansehen des aktiven Spielers und zieht die Taler ab. Wird erst nach
        /// bestätigter Bezahlbarkeit (PruefeGoldMitMeldung) und Bestätigung aufgerufen.
        /// </summary>
        public void FuehreStiftungAus(int index)
        {
            var spieler = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler());
            spieler.ErhoehePermaAnsehen(Convert.ToInt16(_preise[index] / 1000));
            spieler.ErhoeheTaler(-_preise[index]);

            // Gestifteten Gesamtwert mitführen (für den Auftrag „Mäzen").
            spieler.GestifteterBauwert += _preise[index];
        }
    }
}
