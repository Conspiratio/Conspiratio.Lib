using System;
using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Stellt eine Aktion eines Stützpunkts dar, z.B. Überwachen
    /// </summary>
    [Serializable]
    public abstract class StuetzpunktAktion
    {
        #region Variablen und Properties

        private int _zielLandID;
        private int _zielStuetzpunktID;

        /// <summary>
        /// ID des Stützpunktes, zu dem diese Aktion gehört.
        /// </summary>
        public int StuetzpunktID { get; }

        /// <summary>
        /// Index (Nummer) der Aktion im Array des Stützpunktes (normalerweise 0 oder 1)
        /// </summary>
        public int AktionIndexStuetzpunkt { get; }

        /// <summary>
        /// Liste aller Einheiten von dieser Aktion
        /// </summary>
        public List<Einheit> Einheiten { get; set; }

        /// <summary>
        /// LandID des Ziellandes, in dem die Aktion stattfinden soll (z.B. Überwachung von [Zielland], Plündern von [Zielland] usw.)
        /// </summary>
        public int ZielLandID
        {
            get { return _zielLandID; }
            set
            {
                if (SW.Dynamisch.GetLandWithID(value) == null)   // gültiges Ziel-Land?
                    _zielLandID = 0;
                else
                    _zielLandID = value;
            }
        }

        /// <summary>
        /// StuetzpunktID des Zielstuetzpunktes, bei dem die Aktion stattfinden soll (z.B. Schicken von Truppen nach [Zielstuetzpunkt], Überfallen von [Zielstuetzpunkt] usw.)
        /// </summary>
        public int ZielStuetzpunktID
        {
            get { return _zielStuetzpunktID; }
            set
            {
                if ((SW.Dynamisch.GetStuetzpunkte().Length <= (value - 1)) && (SW.Dynamisch.GetStuetzpunkte()[value - 1] != null))  // gültiger Ziel-Stützpunkt?
                    _zielStuetzpunktID = value;
                else
                    _zielStuetzpunktID = 0;
            }
        }

        #endregion

        #region Konstruktor
        /// <summary>
        /// Initialisiert das aktuelle Objekt
        /// </summary>
        /// <param name="zielLandID">Gewünschte LandID des Ziellandes, in dem die Aktion stattfinden soll (z.B. Überwachung von [Zielland], Plündern von [Zielland] usw.)</param>
        /// <param name="zielStuetzpunktID">Gewünschte StuetzpunktID des Zielstuetzpunktes, bei dem die Aktion stattfinden soll (z.B. Schicken von Truppen nach [Zielstuetzpunkt], Überfallen von [Zielstuetzpunkt] usw.)</param>
        /// <param name="stuetzpunktID">ID des Stützpunktes, zu dem diese Aktion gehört.</param>
        /// <param name="aktionIndexStuetzpunkt">Index (Nummer) der Aktion im Array des Stützpunktes (normalerweise 0 oder 1)</param>
        /// <param name="einheiten">OPTIONAL: Gewünschte Einheiten der Aktion</param>
        public StuetzpunktAktion(int zielLandID, int zielStuetzpunktID, int stuetzpunktID, int aktionIndexStuetzpunkt, List<Einheit> einheiten = null)
        {
            ZielLandID = zielLandID;
            ZielStuetzpunktID = zielStuetzpunktID;
            StuetzpunktID = stuetzpunktID;
            AktionIndexStuetzpunkt = aktionIndexStuetzpunkt;
            Einheiten = einheiten;
        }
        #endregion

        #region Public Funktionen

        #region ErhoeheTruppen
        /// <summary>
        /// Erhöht die Truppen der StützpunktAktion um die angegebene Anzahl.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl der Einheiten, die hinzugefügt werden sollen</param>
        /// <param name="TypeEinheit">Gewünschte Einheit, die hinzugefügt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        public void ErhoeheTruppen(int Anzahl, Type TypeEinheit)
        {
            if (!TypeEinheit.IsSubclassOf(typeof(Einheit)))
                return;  // Ungültiger Typ

            if (Einheiten == null)
                Einheiten = new List<Einheit>();

            for (int i = 0; i < Anzahl; i++)
            {
                // Eine neue Instanz der übergebenen Truppeneinheit erstellen und der Liste der Einheiten des Stützpunktes hinzufügen
                Einheiten.Add((Einheit)Activator.CreateInstance(TypeEinheit));
            }
        }
        #endregion

        #region VerringereTruppen
        /// <summary>
        /// Verringert die Truppen der StützpunktAktion um die angegebene Anzahl.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl der Einheiten, die verringert werden sollen</param>
        /// <param name="TypeEinheit">Gewünschte Einheit, die entfernt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        /// <returns>Gibt im Falle von Erfolg null zurück, ansonsten einen String mit der Meldung, warum das Verringern gescheitert ist</returns>
        public string VerringereTruppen(int Anzahl, Type TypeEinheit)
        {
            if (!TypeEinheit.IsSubclassOf(typeof(Einheit)))
                return $"Systemfehler: Ungültiger Typ '{TypeEinheit.ToString()}' für Einheit!";

            if (Einheiten == null)
                Einheiten = new List<Einheit>();

            if (GetAnzahlTruppen(TypeEinheit) < Anzahl)
                return $"Es sind nicht so viele Truppen vorhanden.";

            for (int i = 0; i < Anzahl; i++)
            {
                for (int j = 0; j < Einheiten?.Count; j++)
                {
                    if (Einheiten[j].GetType() == TypeEinheit)
                    {
                        Einheiten.RemoveAt(j);
                        break;
                    }
                }
            }

            return null;
        }
        #endregion

        #region GetAnzahlTruppen
        /// <summary>
        /// Gibt die Anzahl der Truppen einer bestimmten Einheit des Stützpunktes zurück.
        /// </summary>
        /// <param name="TypeEinheit">Gewünschte Einheit, die gezählt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        /// <returns>Gibt gibt die Anzahl der Truppen zurück</returns>
        public int GetAnzahlTruppen(Type TypeEinheit)
        {
            return Einheiten.Count(x => x.GetType() == TypeEinheit);
        }
        #endregion

        public abstract string AktionAusfuehren(int StuetzpunktID);

        #endregion
    }
}
