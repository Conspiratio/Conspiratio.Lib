using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.Weltkarte
{
    /// <summary>Ein Rohstoff samt seinem aktuellen Preis in der betrachteten Stadt.</summary>
    public class RohstoffpreisInfo
    {
        public int RohId { get; }
        public string Name { get; }
        public int Preis { get; }

        public RohstoffpreisInfo(int rohId, string name, int preis)
        {
            RohId = rohId;
            Name = name;
            Preis = preis;
        }
    }

    /// <summary>
    /// Kapselt die Logik von RohstoffpreiseForm (Weltkarte-Modi Händler/Kaufmann/Großkaufmann):
    /// die Rohstoffpreise einer Stadt sowie die level-abhängige Aktion beim Anklicken eines Rohstoffs.
    /// Level 0 = Händler (nur Einsicht), 1 = Kaufmann, 2 = Großkaufmann.
    ///
    /// Hinweis zur Originaltreue: Kaufmann/Großkaufmann erzählen zwar von einer Preisänderung, im
    /// Original wird der berechnete Wert jedoch nie auf den Grundpreis angewendet – dieses Verhalten
    /// wird hier bewusst 1:1 übernommen (die einzige echte Wirkung ist die Einmal-pro-Jahr-Sperre).
    /// </summary>
    public class RohstoffpreiseManager
    {
        private readonly int _stadtID;
        private readonly int _level;

        public RohstoffpreiseManager(int stadtID, int level)
        {
            _stadtID = stadtID;
            _level = level;
        }

        public int Level => _level;

        public string GetStadtName() => SW.Dynamisch.GetStadtwithID(_stadtID).GetGebietsName();

        public string GetUeberschrift() => "Rohstoffpreise in " + GetStadtName();

        /// <summary>Alle Rohstoffe mit ihrem aktuellen Preis in der Stadt.</summary>
        public List<RohstoffpreisInfo> GetPreise()
        {
            var list = new List<RohstoffpreisInfo>();
            var stadt = SW.Dynamisch.GetStadtwithID(_stadtID);

            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                list.Add(new RohstoffpreisInfo(
                    i, SW.Dynamisch.GetRohstoffwithID(i).GetRohName(), stadt.GetRohstoffPreisVonIDX(i)));
            }

            return list;
        }

        public string GetRohstoffName(int rohId) => SW.Dynamisch.GetRohstoffwithID(rohId).GetRohName();

        /// <summary>Aktueller Preis eines Rohstoffs in der Stadt (zum Aktualisieren der Anzeige).</summary>
        public int GetPreis(int rohId) => SW.Dynamisch.GetStadtwithID(_stadtID).GetRohstoffPreisVonIDX(rohId);

        #region Aktion (level-abhängig)

        /// <summary>Ob das Kaufmanns-/Großkaufmanns-Privileg dieses Jahr bereits benutzt wurde (geteilte Sperre).</summary>
        public bool IstBereitsBenutzt() =>
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetPrivilegKaufmannBenutzt();

        public string GetBereitsBenutztMeldung() =>
            "Ihr solltet dieses Jahr keine weiteren Aktionen dieser Art durchführen, da man Euch sonst auf die Schliche kommen würde";

        /// <summary>Level 0 (Händler): keine Einflussmöglichkeit.</summary>
        public string GetZuWenigEinflussMeldung() =>
            "Ihr besitzt zu wenig Einfluss um an\n den Preisen zu rütteln";

        /// <summary>Level 1 (Kaufmann): Bestätigungsfrage.</summary>
        public string GetKaufmannFrage(int rohId) =>
            "Wollt Ihr falsche Informationen verbreiten um\n den Grundpreis von " + GetRohstoffName(rohId) +
            " ins schwanken zu bringen?";

        /// <summary>Level 1 (Kaufmann): verbraucht die Jahressperre und liefert die (zufällige) Ergebnismeldung.</summary>
        public string FuehreKaufmannAus(int rohId)
        {
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).SetPrivilegKaufmannBenutzt(true);

            int value = SW.Statisch.Rnd.Next(-2, 3); // wie im Original berechnet, aber nicht auf den Grundpreis angewendet

            if (value == 0)
                return "Eure Versuche den Grundpreis von " + GetRohstoffName(rohId) + "\n zu ändern, sind gescheitert";
            if (value < 0)
                return "Es ist Euch gelungen den Grundpreis\nvon " + GetRohstoffName(rohId) + " zu senken";
            return "Es ist Euch gelungen den Grundpreis\nvon " + GetRohstoffName(rohId) + " zu steigern";
        }

        /// <summary>Level 2 (Großkaufmann): Frage (Ja = steigern, Nein = senken).</summary>
        public string GetGroßkaufmannFrage(int rohId) =>
            "Wollt Ihr Euren Einfluss auf die Großhändler dazu\n nutzen um den Grundpreis von " +
            GetRohstoffName(rohId) + " zu";

        /// <summary>Level 2 (Großkaufmann): verbraucht die Jahressperre und liefert die Ergebnismeldung.</summary>
        public string FuehreGroßkaufmannAus(int rohId, bool steigern)
        {
            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).SetPrivilegKaufmannBenutzt(true);

            if (steigern)
            {
                SW.Statisch.Rnd.Next(1, 3); // wie im Original berechnet, aber nicht angewendet
                return "Ihr habt den Grundpreis\nvon " + GetRohstoffName(rohId) + " gesteigert";
            }

            SW.Statisch.Rnd.Next(-2, 0); // wie im Original berechnet, aber nicht angewendet
            return "Ihr habt den Grundpreis\nvon " + GetRohstoffName(rohId) + " gesenkt";
        }

        #endregion
    }
}
