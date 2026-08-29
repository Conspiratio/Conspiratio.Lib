using System;

using Conspiratio.Lib.Allgemein;
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

        /// <summary>
        /// Prüft (ohne Nebenwirkung), ob der aktive Spieler das Bauwerk in der aktuell gewählten Stadt
        /// bereits gestiftet hat. Jedes der vier Bauwerke ist je Stadt nur einmal zu haben – damit ist
        /// die Stiftung etwas, das man im ganzen Reich verfolgt (4 mal 14 Städte), statt sie durch
        /// wiederholtes Klicken in derselben Stadt zu häufen. Wie <see cref="KannBezahlen"/> ist dies
        /// eine Vorbedingung, die der Aufrufer prüft, bevor er <see cref="FuehreStiftungAus"/> ruft.
        /// </summary>
        public bool IstBereitsGestiftet(int index) =>
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).HatBauwerkGestiftet(AktiveStadtID, index);

        /// <summary>Meldungstext, wenn das Bauwerk in dieser Stadt bereits gestiftet wurde.</summary>
        public string GetBereitsGestiftetMeldung(int index) =>
            "In " + GetStadtName() + " habt Ihr\nbereits " + _bauwerke[index] + "\ngestiftet.";

        /// <summary>Bestätigungsfrage für die Stiftung des Bauwerks am angegebenen Index.</summary>
        public string GetBestaetigungsfrage(int index) =>
            "Wollt Ihr wirklich für " + _preise[index].ToStringGeld() +
            "\nder Stadt " + GetStadtName() + " " + _bauwerke[index] + " stiften?";

        /// <summary>
        /// Führt die Stiftung des Bauwerks am angegebenen Index in der aktuell gewählten Stadt aus:
        /// erhöht das Permaansehen des aktiven Spielers und zieht die Taler ab. Wird erst nach
        /// bestätigter Bezahlbarkeit (PruefeGoldMitMeldung) und Bestätigung aufgerufen.
        ///
        /// Das Ansehen ist auf <see cref="AbrechnungsManager.AnsehenMaxProJahr"/> je Spieler und Jahr
        /// gedeckelt wie die anderen beiden Ansehensquellen (Hofhaltung, Auslastung) - sonst wäre eine
        /// Stiftung mit 1 000 Talern je Punkt, beliebig oft im selben Zug wiederholbar, die mit Abstand
        /// billigste Geltung im Spiel. Bezahlt und gestiftet wird trotz Deckel immer voll: Nur das
        /// zusätzliche Ansehen entfällt, das Bauwerk steht so oder so.
        ///
        /// Die Stiftung wird beim Spieler vermerkt (<see cref="IstBereitsGestiftet"/>), denn dasselbe
        /// Bauwerk lässt sich in derselben Stadt kein zweites Mal stiften.
        /// </summary>
        public void FuehreStiftungAus(int index)
        {
            var spieler = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler());

            int rohertrag = Convert.ToInt16(_preise[index] / 1000);
            int nochFrei = AbrechnungsManager.AnsehenMaxProJahr - spieler.AnsehenAusStiftungenDiesesJahr;
            int ertrag = rohertrag;

            if (ertrag > nochFrei)
                ertrag = nochFrei;
            if (ertrag < 0)
                ertrag = 0;

            spieler.ErhoehePermaAnsehen(ertrag);
            spieler.AnsehenAusStiftungenDiesesJahr += ertrag;

            spieler.ErhoeheTaler(-_preise[index]);

            // Gestifteten Gesamtwert mitführen (für den Auftrag „Mäzen") - unabhängig vom Ansehensdeckel,
            // der volle Betrag ist gezahlt und das Bauwerk steht.
            spieler.GestifteterBauwert += _preise[index];

            // Bauwerk und Stadt vermerken: Dieses Bauwerk ist hier ab jetzt vergeben.
            spieler.MerkeBauwerkStiftung(AktiveStadtID, index);
        }
    }
}
