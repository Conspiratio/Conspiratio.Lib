using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Logik zur Erstellung der menschlichen Spieler zu Beginn eines neuen Spiels.
    /// Im Gegensatz zum alten WinForms-Wizard arbeitet <see cref="ErstelleSpieler"/> transaktional:
    /// Alle Eigenschaften eines Spielers werden in einem Aufruf gesetzt, dadurch entfällt die
    /// Zurück-Navigation mit Taler-Rückerstattung.
    /// </summary>
    public class PlayerSetupManager
    {
        [PublicAPI]
        public const int AnzahlBanner = 14;

        private int _urspruenglichAktiverSpieler;

        [PublicAPI]
        public int AnzahlAngelegteSpieler { get; private set; }

        [PublicAPI]
        public bool AlleSpielerAngelegt => AnzahlAngelegteSpieler >= SW.Dynamisch.GetAktivSpielerAnzahl();

        /// <summary>
        /// Startet die Spielererstellung: merkt sich den aktuell aktiven Spieler und setzt den ersten
        /// menschlichen Spieler (ID 1) als aktiv.
        /// </summary>
        [PublicAPI]
        public void Starte()
        {
            _urspruenglichAktiverSpieler = SW.Dynamisch.GetAktiverSpieler();
            AnzahlAngelegteSpieler = 0;
            SW.Dynamisch.SetAktiverSpieler(1);
        }

        /// <summary>
        /// Beendet die Spielererstellung (nach Abschluss oder Abbruch) und stellt den ursprünglich
        /// aktiven Spieler wieder her.
        /// </summary>
        [PublicAPI]
        public void Beende()
        {
            SW.Dynamisch.SetAktiverSpieler(_urspruenglichAktiverSpieler);
        }

        [PublicAPI]
        public bool ValidateName(string name, out string error)
        {
            if (string.IsNullOrEmpty(name) || name.Length < SW.Statisch.GetMinNameLength())
            {
                error = "Euer Name muss aus mindestens " + SW.Statisch.GetMinNameLength() + " Zeichen bestehen";
                return false;
            }

            if (name.Contains("~"))
            {
                error = "Euer Name darf kein ~ Zeichen enthalten";
                return false;
            }

            for (int i = 1; i < SW.Dynamisch.GetAktiverSpieler(); i++)
            {
                if (SW.Dynamisch.GetHumWithID(i).GetName() == name)
                {
                    error = "Es ist bereits ein Mitspieler mit demselben Namen vorhanden. Bitte wählt einen anderen";
                    return false;
                }
            }

            error = "";
            return true;
        }

        /// <summary>
        /// Prüft, ob das Banner bereits von einem zuvor angelegten Spieler gewählt wurde.
        /// </summary>
        [PublicAPI]
        public bool IstBannerVergeben(int banner)
        {
            for (int i = 1; i < SW.Dynamisch.GetAktiverSpieler(); i++)
            {
                if (SW.Dynamisch.GetHumWithID(i).GetBanner() == banner)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Würfelt eine zufällige Stadt-ID aus, z. B. als kostenlose Vorauswahl der Heimatstadt.
        /// </summary>
        [PublicAPI]
        public int WuerfleZufaelligeStadt()
        {
            return SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
        }

        /// <summary>
        /// Erstellt den aktuell aktiven Spieler vollständig und schaltet danach auf den nächsten
        /// Spieler weiter (sofern noch nicht alle angelegt sind).
        /// </summary>
        /// <param name="name">Der (bereits validierte) Spielername.</param>
        /// <param name="maennlich">Das Geschlecht des Spielers.</param>
        /// <param name="banner">Die Banner-Nummer (1 bis <see cref="AnzahlBanner"/>).</param>
        /// <param name="religionId">Die Religions-ID (katholisch oder evangelisch).</param>
        /// <param name="stadtId">Die Heimatstadt oder 0 für eine zufällige Stadt.</param>
        /// <param name="stadtGewaehlt">True, wenn die Stadt bewusst gewählt wurde (kostet Taler); false bei einer zufällig bestimmten Stadt (kostenlos).</param>
        /// <param name="rohstoffPlatz">Der gewählte Rohstoffplatz 1 oder 2 der Heimatstadt (kostet Taler) oder 0 für einen zufälligen Platz (kostenlos).</param>
        /// <param name="profilId">Optionale Verknüpfung zum spielübergreifenden Profil (GUID) dieses Spielers. Null = ohne Profil.</param>
        [PublicAPI]
        public PlayerSetupErgebnis ErstelleSpieler(string name, bool maennlich, int banner, int religionId, int stadtId, bool stadtGewaehlt, int rohstoffPlatz, string profilId = null)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.SetTaler(SW.Statisch.GetStartgold());
            spieler.SetName(name);
            spieler.ProfilId = profilId;
            spieler.SetVerbleibendeJahre(SW.Statisch.Rnd.Next(SW.Statisch.GetHumminVerblJahre(), SW.Statisch.GetHummaxVerblJahre()));
            spieler.SetMaennlich(maennlich);
            spieler.SetBanner(banner);
            spieler.SetReligion(religionId);

            if (stadtId == 0)
                stadtId = WuerfleZufaelligeStadt();
            else if (stadtGewaehlt)
                spieler.ErhoeheTaler(-SW.Statisch.GetNSPStadtwahlKosten());

            spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId).SetHausID(SW.Statisch.GetStartHausID());
            spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId).SetStadtID(stadtId);

            if (rohstoffPlatz == 0)
                rohstoffPlatz = SW.Statisch.Rnd.Next(1, 3);
            else
                spieler.ErhoeheTaler(-SW.Statisch.GetNSPRohwahlKosten());

            int rohstoffId = SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(rohstoffPlatz);

            var werkstatt = spieler.GetSpielerHatInStadtXWerkstaettenY(rohstoffPlatz, stadtId);
            werkstatt.SetRohstoffID(rohstoffId);
            werkstatt.SetSkillX(1, SW.Statisch.GetStartLagerraum());  // Startlagerraum
            werkstatt.SetEnabled(true);
            spieler.SetRohstoffrechteXZuY(rohstoffId, true);

            AnzahlAngelegteSpieler++;

            if (!AlleSpielerAngelegt)
                SW.Dynamisch.SetAktiverSpieler(SW.Dynamisch.GetAktiverSpieler() + 1);

            return new PlayerSetupErgebnis(stadtId, rohstoffId);
        }
    }

    /// <summary>
    /// Das Ergebnis einer Spielererstellung mit den (ggf. zufällig) aufgelösten IDs.
    /// </summary>
    public class PlayerSetupErgebnis
    {
        public PlayerSetupErgebnis(int stadtId, int rohstoffId)
        {
            StadtId = stadtId;
            RohstoffId = rohstoffId;
        }

        public int StadtId { get; }

        public int RohstoffId { get; }
    }
}
