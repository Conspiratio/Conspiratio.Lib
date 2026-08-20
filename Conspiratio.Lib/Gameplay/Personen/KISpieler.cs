using System;

using Conspiratio.Lib.Gameplay.Schreibstube;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public class KISpieler : Spieler
    {
        #region Variablen
        // Amt: erhöht bzw verringert die Basisdaten zusätzlich
        // alle Ämter werden in Klasse Ämter gespeichert

        // Basisdaten von 1 bis 100 in %
        // Beeinflussen die randoms welche das Spielverhalten
        // von der KI bestimmen
        private int _boese;
        private int _verliebt;
        private bool _nimmtAnWahlTeil;
        private bool _stirbt;

        private int[] _beziehungZuKIMitID;
        #endregion

        #region Konstruktor
        public KISpieler(int taler, string name, bool maennlich, int boese, int verheiratetMit, int verbleibendeJahre): base(taler, name, maennlich, verheiratetMit, verbleibendeJahre)
        {
            this.Taler = taler;
            this.Name = name;
            this.Maennlich = maennlich;
            _boese = boese;
            this.VerheiratetMit = verheiratetMit;

            _beziehungZuKIMitID = new int[SW.Statisch.GetMaxKIID()];
           
            Amtsinformationen = new AmtsInfo(0, 0);
        }
        #endregion

        #region Getter und Setter
        
        public void CreateRndBeziehungen(int own_id)
        {
            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                int rand_wert = SW.Statisch.Rnd.Next(20,81);

                _beziehungZuKIMitID[i] = rand_wert;
            }
        }

        public bool GetStirbt()
        {
            return _stirbt;
        }

        public void SetStirbt(bool value)
        {
            _stirbt = value;
        }

        public void SetVerliebt(int ver)
        {
            _verliebt = ver;
        }

        /// <summary>
        /// Die wirksame Bosheit dieser KI: ihr ausgewürfelter Charakterwert (<c>_boese</c>), verschoben
        /// um die eingestellte KI-Aggressivität. Bei 50 % (Standard) ist das exakt der Charakterwert,
        /// bei 100 % um 50 Punkte höher, bei 1 % um 49 Punkte niedriger – jeweils auf 0–100 begrenzt
        /// (ein gespeicherter Wert 0 gilt als „nicht gesetzt" und wird wie 50 % behandelt, siehe
        /// <see cref="DynamischeSpieldaten.GetKiAggressivitaetProzent"/>). Die Streuung zwischen den
        /// KIs bleibt dabei erhalten: Die Einstellung verschiebt alle Charaktere gemeinsam, sie gleicht
        /// sie nicht an.
        ///
        /// Bewusst hier und nicht beim Auswürfeln: Nur so wirkt die Einstellung auch auf bereits
        /// existierende KIs, im laufenden Spiel und in geladenen Spielständen. Der gespeicherte
        /// Charakterwert bleibt davon unberührt – wer ihn braucht, liest <c>_boese</c> direkt.
        /// </summary>
        public int GetBosheit()
        {
            int verschiebung = SW.Dynamisch.GetKiAggressivitaetProzent() - 50;

            return Math.Max(0, Math.Min(100, _boese + verschiebung));
        }

        /// <summary>
        /// Der rohe, ausgewürfelte Charakterwert dieser KI (<c>_boese</c>), unbeeinflusst von der
        /// eingestellten KI-Aggressivität. Gedacht für Mechaniken, die keine Feindseligkeit gegenüber
        /// dem Menschen ausdrücken (z. B. das Werbegeschenk in <c>FamilieManager.GibGeschenk</c>) und
        /// deshalb nicht am Aggressivitäts-Regler hängen dürfen. Für alles, was den Menschen
        /// betrifft, gilt weiterhin <see cref="GetBosheit"/>.
        /// </summary>
        public int GetBosheitRoh()
        {
            return _boese;
        }

        /// <summary>
        /// Setzt den rohen Charakterwert. Achtung: Dieser Wert ist nicht um die KI-Aggressivität
        /// verschoben – <c>SetBosheit(GetBosheit() + n)</c> würde die aktuelle Aggressivitäts-Verschiebung
        /// dauerhaft in den gespeicherten Charakterwert einbacken. Für ein Lesen-Ändern-Schreiben
        /// deshalb <see cref="GetBosheitRoh"/> verwenden, nicht <see cref="GetBosheit"/>.
        /// </summary>
        public void SetBosheit(int best)
        {
            _boese = best;
        }

        public void ErhoeheBeziehungZuX(int x, int wert)
        {
            _beziehungZuKIMitID[x] += wert;

            if (_beziehungZuKIMitID[x] > 100)
                _beziehungZuKIMitID[x] = 100;
            
            if (_beziehungZuKIMitID[x] < 0)
                _beziehungZuKIMitID[x] = 0;
        }

        public void SetBeziehungZuX(int x, int wert)
        {
            _beziehungZuKIMitID[x] = wert;
        }

        public int GetBeziehungZuKIX(int x)
        {
            return _beziehungZuKIMitID[x];
        }

        public int[] GetBeziehungZuAllen()
        {
            return _beziehungZuKIMitID;
        }

        public int GetVerliebt()
        {
            return _verliebt;
        }

        public void ErhoeheVerliebt(int i)
        {
            _verliebt += i;

            if (_verliebt < 0)
                _verliebt = 0;
            
            if (_verliebt > 100)
                _verliebt = 100;
        }

        public bool GetNimmtAnWahlTeil()
        {
            return _nimmtAnWahlTeil;
        }

        public void SetNimmtAnWahlTeil(bool trueOrFalse)
        {
            _nimmtAnWahlTeil = trueOrFalse;
        }

        public override int GetGesamtVermoegen(int spielerID)
        {
            int Gesamtvermoegen = Taler;

            // Stützpunkte
            for (int i = 0; i < SW.Dynamisch.GetStuetzpunkte().Length; i++)
            {
                if (SW.Dynamisch.GetStuetzpunkte()[i].Besitzer == spielerID)
                    Gesamtvermoegen += SW.Dynamisch.GetStuetzpunkte()[i].BerechneWert();
            }

            return Gesamtvermoegen;
        }
        #endregion
    }
}
