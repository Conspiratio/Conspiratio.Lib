using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt den Kauf von zusätzlichem Lagerraum für eine Werkstätte des Spielers in einer Stadt
    /// (Migration von LagerraumKaufen aus dem WinForms-Client). Beim Erstellen werden drei Angebote
    /// (Größe in m² und Preis) ermittelt; der Preis richtet sich nach dem Reichtum der Stadt.
    /// </summary>
    public class LagerraumManager
    {
        public const int AnzahlAngebote = 3;

        private readonly int _stadtId;
        private readonly int _werkstattNr;
        private readonly int[] _groessen = new int[AnzahlAngebote];
        private readonly int[] _preise = new int[AnzahlAngebote];

        public LagerraumManager(int stadtId, int werkstattNr)
        {
            _stadtId = stadtId;
            _werkstattNr = werkstattNr;

            AktuellerLagerraum = Werkstatt().GetSKillX(1);

            int basispreis = SW.Statisch.GetLagerraumBasisPreis();
            double preiszuschlag = (double)SW.Dynamisch.GetStadtwithID(_stadtId).GetReichtum() / SW.Statisch.GetMaxReichtum();

            _groessen[0] = AktuellerLagerraum * SW.Statisch.Rnd.Next(10, 30) / 100;
            _groessen[1] = AktuellerLagerraum * SW.Statisch.Rnd.Next(20, 40) / 100;
            _groessen[2] = AktuellerLagerraum * SW.Statisch.Rnd.Next(40, 60) / 100;

            for (int i = 0; i < AnzahlAngebote; i++)
                _preise[i] = (int)(_groessen[i] * (basispreis + basispreis * preiszuschlag));
        }

        /// <summary>Der aktuelle Lagerraum der Werkstätte in m² (steigt nach einem Kauf).</summary>
        public int AktuellerLagerraum { get; private set; }

        /// <summary>Die zusätzliche Lagerfläche (m²) des Angebots (0..2).</summary>
        public int GetGroesse(int angebot) => _groessen[angebot];

        /// <summary>Der Preis des Angebots (0..2).</summary>
        public int GetPreis(int angebot) => _preise[angebot];

        /// <summary>
        /// Kauft das Angebot (0..2): Bei ausreichendem Guthaben wird der Lagerraum erweitert und der
        /// Preis abgezogen. Liefert true bei Erfolg.
        /// </summary>
        public bool Kaufe(int angebot)
        {
            if (!SW.Dynamisch.CheckIfenoughGold(_preise[angebot]))
                return false;

            Werkstatt().SetSkillX(1, AktuellerLagerraum + _groessen[angebot]);
            SW.Dynamisch.GetAktHum().ErhoeheTaler(-_preise[angebot]);
            AktuellerLagerraum = Werkstatt().GetSKillX(1);

            return true;
        }

        private Gameplay.Niederlassung.SpHatWerkstaetten Werkstatt()
        {
            return SW.Dynamisch.GetAktHum().GetSpielerHatInStadtXWerkstaettenY(_werkstattNr, _stadtId);
        }
    }
}
