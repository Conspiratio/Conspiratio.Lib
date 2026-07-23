using System;
using System.Collections.Generic;

using Conspiratio.Kampf; // konkrete Einheitentypen (Savegame-Kompatibilität)
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Kapselt die beiden Aktionen eines eigenen Stützpunkts (Migration des Aktionsbereichs von
    /// frmStuetzpunktVerwalten): je Slot die Aktionsart (Kein Auftrag / Truppen schicken → anderer
    /// Stützpunkt / Überwachen bzw. Plündern → Grafschaft), das Ziel und die zugeteilten Einheiten.
    /// Die Aktionen werden am Rundenende von der Lib ausgeführt.
    /// </summary>
    public class StuetzpunktAktionenManager
    {
        private readonly int _stuetzpunktId;
        private readonly Stuetzpunkt _stuetzpunkt;
        private readonly bool _istZollburg;
        private readonly Einheit[] _einheiten;
        private readonly Type[] _typen;

        public StuetzpunktAktionenManager(int stuetzpunktId)
        {
            _stuetzpunktId = stuetzpunktId;
            _stuetzpunkt = SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1];
            _istZollburg = _stuetzpunkt.Art == EnumStuetzpunktArt.Zollburg;

            _einheiten = _istZollburg
                ? new Einheit[] { new ZollSoeldner(), new ZollMusketier(), new ZollKanonier(), new ZollOffizier() }
                : new Einheit[] { new RaubRaeuber(), new RaubBombenleger(), new RaubKanonier(), new RaubSchuetze() };
            _typen = new Type[_einheiten.Length];
            for (int i = 0; i < _einheiten.Length; i++)
                _typen[i] = _einheiten[i].GetType();

            if (_istZollburg)
                SW.Dynamisch.GetZollburgWithIDx(stuetzpunktId - 1).AktionenInitialisieren();
            else
                SW.Dynamisch.GetRaeuberlagerWithIDx(stuetzpunktId - 1).AktionenInitialisieren();

            for (int slot = 0; slot < AktionenAnzahl; slot++)
                if (_stuetzpunkt.Aktionen[slot] == null)
                    ErstelleAktion(slot, 0);
        }

        public int AktionenAnzahl => 2;
        public int EinheitenAnzahl => _einheiten.Length;
        public int LandMin => SW.Statisch.GetMinLandID();
        public int LandMax => SW.Statisch.GetMaxLandID() - 1;
        public int StuetzpunktMin => 1;
        public int StuetzpunktMax => SW.Dynamisch.GetStuetzpunkte().Length;

        public string GetEinheitName(int unitIndex) => _einheiten[unitIndex].NamePlural;

        /// <summary>Aktionsart: 0 = Kein Auftrag, 1 = Truppen schicken (Ziel: Stützpunkt), 2 = Überwachen/Plündern (Ziel: Grafschaft).</summary>
        public int GetAktionsart(int slot) => _istZollburg
            ? (int)((ZollburgAktion)_stuetzpunkt.Aktionen[slot]).Aktionsart
            : (int)((RaeuberlagerAktion)_stuetzpunkt.Aktionen[slot]).Aktionsart;

        public string GetAktionsartName(int slot)
        {
            switch (GetAktionsart(slot))
            {
                case 1: return "Truppen schicken";
                case 2: return _istZollburg ? "Überwachen" : "Plündern";
                default: return "Kein Auftrag";
            }
        }

        /// <summary>Bei "Truppen schicken" ist das Ziel ein anderer Stützpunkt.</summary>
        public bool ZielIstStuetzpunkt(int slot) => GetAktionsart(slot) == 1;

        /// <summary>Bei "Überwachen"/"Plündern" ist das Ziel eine Grafschaft (Land).</summary>
        public bool ZielIstGrafschaft(int slot) => GetAktionsart(slot) == 2;

        /// <summary>Schaltet die Aktionsart des Slots auf die nächste (dabei werden Ziel und Truppen zurückgesetzt).</summary>
        public void ZyklusAktionsart(int slot) => ErstelleAktion(slot, (GetAktionsart(slot) + 1) % 3);

        private void ErstelleAktion(int slot, int art)
        {
            if (_istZollburg)
                _stuetzpunkt.Aktionen[slot] = new ZollburgAktion((EnumAktionsartZollburg)art, LandMin, 1, _stuetzpunktId, slot, new List<Einheit>());
            else
                _stuetzpunkt.Aktionen[slot] = new RaeuberlagerAktion((EnumAktionsartRaeuberlager)art, LandMin, 1, _stuetzpunktId, slot, new List<Einheit>());
        }

        public int GetZielLand(int slot) => _stuetzpunkt.Aktionen[slot].ZielLandID;
        public string GetZielLandName(int slot) => SW.Dynamisch.GetLandWithID(GetZielLand(slot)).GetGebietsName();
        public void SetZielLand(int slot, int landId) => _stuetzpunkt.Aktionen[slot].ZielLandID = landId;

        public int GetZielStuetzpunkt(int slot) => _stuetzpunkt.Aktionen[slot].ZielStuetzpunktID;
        public string GetZielStuetzpunktName(int slot) => SW.Dynamisch.GetStuetzpunkte()[GetZielStuetzpunkt(slot) - 1].Name;
        public void SetZielStuetzpunkt(int slot, int stuetzpunktId) => _stuetzpunkt.Aktionen[slot].ZielStuetzpunktID = stuetzpunktId;

        public int GetEinheitInAktion(int slot, int unitIndex) => _stuetzpunkt.Aktionen[slot].GetAnzahlTruppen(_typen[unitIndex]);

        /// <summary>Setzt die Anzahl eines Einheitentyps im Slot auf den gewünschten Wert.</summary>
        public void SetEinheitInAktion(int slot, int unitIndex, int anzahl)
        {
            int vorher = GetEinheitInAktion(slot, unitIndex);

            if (anzahl > vorher)
                _stuetzpunkt.Aktionen[slot].ErhoeheTruppen(anzahl - vorher, _typen[unitIndex]);
            else if (anzahl < vorher)
                _stuetzpunkt.Aktionen[slot].VerringereTruppen(vorher - anzahl, _typen[unitIndex]);
        }

        /// <summary>
        /// Maximale Anzahl eines Einheitentyps, die dem Slot zugeteilt werden kann: die im Stützpunkt
        /// stationierten Truppen abzüglich der Zuteilung des anderen Slots.
        /// </summary>
        public int GetMaxEinheitInAktion(int slot, int unitIndex)
        {
            int andererSlot = slot == 0 ? 1 : 0;
            return _stuetzpunkt.GetAnzahlTruppen(_typen[unitIndex]) - _stuetzpunkt.Aktionen[andererSlot].GetAnzahlTruppen(_typen[unitIndex]);
        }
    }
}
