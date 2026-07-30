using System;
using System.Threading.Tasks;

using Conspiratio.Kampf; // konkrete Einheitentypen (ZollSoeldner, RaubRaeuber, ... – aus Savegame-Kompatibilität in diesem Namespace)
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Kapselt die Truppenverwaltung eines eigenen Stützpunkts (Migration von frmStuetzpunktVerwalten,
    /// Kernbereich): die vier Einheitentypen (je nach Zollburg oder Räuberlager) mit ihrer aktuellen
    /// Anzahl, das Anheuern/Entlassen sowie das Manöver. Sicherheit/Tarnung, Zustand, Kapazität und
    /// Zollsatz laufen über den ProzentwertFestlegen-Dialog; die eigentlichen Aktionen (Überfall,
    /// Überwachen, Verstärkung) folgen in einem weiteren Schritt.
    /// </summary>
    public class StuetzpunktVerwaltenManager
    {
        private readonly Stuetzpunkt _stuetzpunkt;
        private readonly Einheit[] _einheiten;
        private readonly Type[] _typen;

        public StuetzpunktVerwaltenManager(int stuetzpunktId)
        {
            StuetzpunktId = stuetzpunktId;
            _stuetzpunkt = SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1];

            _einheiten = IstZollburg
                ? new Einheit[] { new ZollSoeldner(), new ZollMusketier(), new ZollKanonier(), new ZollOffizier() }
                : new Einheit[] { new RaubRaeuber(), new RaubBombenleger(), new RaubKanonier(), new RaubSchuetze() };

            _typen = new Type[_einheiten.Length];
            for (int i = 0; i < _einheiten.Length; i++)
                _typen[i] = _einheiten[i].GetType();
        }

        public int StuetzpunktId { get; }

        public string Name => _stuetzpunkt.Name;

        /// <summary>Zollburg (dann gibt es zusätzlich den Zollsatz) oder Räuberlager?</summary>
        public bool IstZollburg => _stuetzpunkt.Art == EnumStuetzpunktArt.Zollburg;

        /// <summary>Beschriftung des Sicherheits-/Tarnungs-Buttons (je nach Art).</summary>
        public string SicherheitLabel => _stuetzpunkt.SicherheitTarnungAlsString();

        /// <summary>Gesamtkapazität des Stützpunkts (Obergrenze für die Rekrutierung).</summary>
        public int Kapazitaet => _stuetzpunkt.Kapazitaet;

        /// <summary>Anzahl der Einheitentypen (immer 4).</summary>
        public int EinheitenAnzahl => _einheiten.Length;

        /// <summary>Plural-Name eines Einheitentyps (z. B. "Söldner").</summary>
        public string GetEinheitName(int index) => _einheiten[index].NamePlural;

        /// <summary>Aktuell stationierte Anzahl eines Einheitentyps zuzüglich der angeworbenen (noch eintreffenden) Truppen.</summary>
        public int GetAnzahl(int index) => _stuetzpunkt.GetAnzahlTruppenInklGeworben(_typen[index]);

        /// <summary>Heuert die angegebene Anzahl eines Einheitentyps an (mit Rückfrage/Kostenprüfung in der Lib).</summary>
        public Task<bool> Anheuern(int index, int anzahl) => _stuetzpunkt.TruppenAnheuern(anzahl, _typen[index]);

        /// <summary>Entlässt die angegebene Anzahl eines Einheitentyps (mit Rückfrage in der Lib).</summary>
        public Task<bool> Entlassen(int index, int anzahl) => _stuetzpunkt.TruppenEntlassen(anzahl, _typen[index]);

        /// <summary>Führt ein Manöver durch (kostet Taler, hebt die Moral) – mit Rückfrage in der Lib.</summary>
        public Task<bool> ManoeverDurchfuehren() => _stuetzpunkt.ManoeverDurchfuehrenSpieler();

        /// <summary>
        /// Ob der Stützpunkt zum Verkauf angeboten wird. Ist dies gesetzt, unterbreiten KI-Spieler
        /// von Zeit zu Zeit zufällige Kaufangebote, die dem Besitzer zu Zugbeginn vorgelegt werden.
        /// </summary>
        public bool ZumVerkaufAngeboten
        {
            get => _stuetzpunkt.ZumVerkaufAngeboten;
            set => _stuetzpunkt.ZumVerkaufAngeboten = value;
        }

        /// <summary>Ob für die Truppen dieses Stützpunkts bereits ein Moral-Bonus vor dem Kampf bezahlt wurde.</summary>
        public bool MoralBonusBezahlt => _stuetzpunkt.MoralBonusBezahlt > 0;

        /// <summary>Kosten für einen einmaligen Moral-Bonus vor dem Kampf.</summary>
        public int KostenMoralBonus => _stuetzpunkt.BerechneKostenMoralBonus();

        /// <summary>Bezahlt einen einmaligen Moral-Bonus für die kommende Schlacht (Rückerstattung bei Sieg).</summary>
        public Task<bool> MoralBonusZahlen() => _stuetzpunkt.MoralBonusZahlen();
    }
}
