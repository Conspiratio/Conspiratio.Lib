using System;

namespace Conspiratio.Lib.Gameplay.Kampf.Einheiten
{
    /// <summary>
    /// Basisklasse zur Verwaltung von Einheiten (Kämpfer) eines Stützpunktes. Eine Instanz dieser Klasse stellt einen Kämpfer dar.
    /// </summary>
    [Serializable]
    public class Einheit
    {
        private double _maximaleLebenspunkte;
        private double _lebenspunkte;

        /// <summary>
        /// Interne, eindeutige Nummer der Einheit, wird automatisch generiert (GUID)
        /// </summary>
        public Guid GUID { get; }

        /// <summary>
        /// Name der Einheit (z.B. Offizier)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Name im Plural (Mehrzahl) der Einheit (z.B. Offiziere)
        /// </summary>
        public string NamePlural { get; }

        /// <summary>
        /// Art des Stützpunktes, auf dem diese Einheit stationiert werden kann (z.B. Zollburg)
        /// </summary>
        public EnumStuetzpunktArt StuetzpunktArt { get; }

        /// <summary>
        /// Angriffswert, bestimmt den Schaden beim Angriff
        /// </summary>
        public double Angriffswert { get; }

        /// <summary>
        /// Verteidungswert, bestimmt den Schaden bei der Verteidung
        /// </summary>
        public double Verteidigungswert { get; }

        /// <summary>
        /// Basispreis für die Rekrutierung
        /// </summary>
        public int Basispreis { get; }

        /// <summary>
        /// Agilität im Kampf, bestimmt die Reihenfolge, in der die Einheiten angreifen
        /// </summary>
        public double Agilitaet { get; }

        /// <summary>
        /// Stark gegen diese bestimmte Einheit (kann auch null sein)
        /// </summary>
        public Type StarkGegen { get; }

        /// <summary>
        /// Schwach gegen diese bestimmte Einheit (kann auch null sein)
        /// </summary>
        public Type SchwachGegen { get; }

        /// <summary>
        /// Maximale Lebenspunkte der Einheit. Wenn die Lebenspunkte kleiner gleich 0 sind, stirbt die Einheit.
        /// </summary>
        public double MaximaleLebenspunkte
        {
            get { return _maximaleLebenspunkte; }
            set
            {
                if (value < 0)
                    _maximaleLebenspunkte = 0;
                else
                    _maximaleLebenspunkte = value;
            }
        }

        /// <summary>
        /// Aktuelle Lebenspunkte der Einheit. Wenn die Lebenspunkte kleiner gleich 0 sind, stirbt die Einheit.
        /// Kann die maximalen Lebenspunkte nicht überschreiten.
        /// </summary>
        public double Lebenspunkte
        {
            get { return _lebenspunkte; }
            set
            {
                if (value < 0)
                    _lebenspunkte = 0;
                else if (value > MaximaleLebenspunkte)
                    _lebenspunkte = MaximaleLebenspunkte;
                else
                    _lebenspunkte = value;
            }
        }

        /// <summary>
        /// Initialisiert das aktuelle Objekt
        /// </summary>
        /// <param name="name">Gewünschter Name der Einheit (z.B. Offizier)</param>
        /// <param name="namePlural">Gewünschter Name im Plural (Mehrzahl) der Einheit (z.B. Offiziere)</param>
        /// <param name="stuetzpunktArt">Gewünschte Stützpunktart der Einheit (gibt an, auf welchem Stützpunkt die Einheit stationiert werden kann)</param>
        /// <param name="angriffswert">Gewünschter Angriffswert</param>
        /// <param name="verteidigungswert">Gewünschter Verteidigungswert</param>
        /// <param name="basispreis">Gewünschter Basispreis für die Rekrutierung</param>
        /// <param name="agilitaet">Gewünschte Agilität im Kampf, bestimmt die Reihenfolge, in der die Einheiten angreifen</param>
        /// <param name="maximaleLebenspunkte">Gewünschte maximale Lebenspunkte</param>
        /// <param name="lebenspunkte">Gewünschte Lebenspunkte</param>
        /// <param name="starkGegen">OPTIONAL: Eine Einheit, gegen die diese Einheit erhebliche Stärken besitzt (Standard: null)</param>
        /// <param name="schwachGegen">OPTIONAL: Eine Einheit, gegen die diese Einheit erhebliche Schwächen besitzt (Standard: null)</param>
        public Einheit(string name, string namePlural, EnumStuetzpunktArt stuetzpunktArt, double angriffswert, double verteidigungswert, int basispreis, double agilitaet,
                       double maximaleLebenspunkte, double lebenspunkte, Type starkGegen = null, Type schwachGegen = null) 
        {
            GUID = Guid.NewGuid();
            Name = name;
            NamePlural = namePlural;
            StuetzpunktArt = stuetzpunktArt;
            Angriffswert = angriffswert;
            Verteidigungswert = verteidigungswert;
            Basispreis = basispreis;
            Agilitaet = agilitaet;
            MaximaleLebenspunkte = maximaleLebenspunkte;
            Lebenspunkte = lebenspunkte;

            if (starkGegen.IsSubclassOf(typeof(Einheit)))
                StarkGegen = starkGegen;
            else
                StarkGegen = null;

            if (schwachGegen.IsSubclassOf(typeof(Einheit)))
                SchwachGegen = schwachGegen;
            else
                SchwachGegen = null;
        }

        /// <summary>
        /// Berechnet die Angriffsstärke der Einheit nach folgender Formel:
        /// Angriffsstärke = Angriffswert + Moralfaktor + StarkGegenFaktor
        /// </summary>
        /// <param name="Moral">Aktuelle Moral der Einheit</param>
        /// <param name="Gegner">Aktueller Gegner-Typ der Einheit</param>
        /// <returns>Aktuelle Angriffsstärke der Einheit</returns>
        public double AngriffsstaerkeBerechnen(int Moral, Type Gegner)
        {
            double Angriffsstaerke = Angriffswert;

            Angriffsstaerke += (Convert.ToDouble(Moral) / 50d) - 1d;

            if (Gegner == StarkGegen)
                Angriffsstaerke += (Angriffsstaerke * 0.1d);

            return Angriffsstaerke;
        }

        /// <summary>
        /// Berechnet die Verteidigungsstärke der Einheit nach folgender Formel:
        /// Verteidigungsstärke = Verteidungswert + Moralfaktor - SchwachGegenFaktor
        /// </summary>
        /// <param name="Moral">Aktuelle Moral der Einheit</param>
        /// <param name="Gegner">Aktueller Gegner-Typ der Einheit</param>
        /// <returns>Aktuelle Verteidigungsstärke der Einheit</returns>
        public double VerteidigungsstaerkeBerechnen(int Moral, Type Gegner)
        {
            double Verteidigungsstaerke = Verteidigungswert;

            Verteidigungsstaerke += (Convert.ToDouble(Moral) / 50d) - 1d;

            if (Gegner == SchwachGegen)
                Verteidigungsstaerke -= (Verteidigungsstaerke * 0.1d);

            return Verteidigungsstaerke;
        }
    }
}
