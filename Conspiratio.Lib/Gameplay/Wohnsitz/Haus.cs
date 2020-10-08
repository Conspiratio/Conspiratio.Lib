using System.Collections.Generic;

namespace Conspiratio.Lib.Gameplay.Wohnsitz
{
    public class Haus
    {
        public int HausID { get; }

        /// <summary>
        /// Der Name des Hauses ohne Zustand etc.
        /// </summary>
        public string Name { get; }

        public int Ansehensbonus { get; }

        public int Kaufpreis { get; }

        public int Bauzeit { get; }

        public string Bildname { get; }

        public List<HausErweiterung> HausErweiterungen { get; }

        private string _pronomen;  // Pronomen für Meldung: Euer <Name>, Eure <Name>, 
        private HausZustandsbezeichnung[] _hausZustandsbezeichnung;

        public Haus(int hausID, string name, int ansehensbonus, int kaufpreis, int bauzeit, string bildname, string pronomen, 
                    HausZustandsbezeichnung[] hausZustandsbezeichnung, List<HausErweiterung> hausErweiterungen)
        {
            HausID = hausID;
            Name = name;
            Ansehensbonus = ansehensbonus;
            Kaufpreis = kaufpreis;
            Bauzeit = bauzeit;
            Bildname = bildname;
            HausErweiterungen = hausErweiterungen;
            
            _pronomen = pronomen;
            _hausZustandsbezeichnung = hausZustandsbezeichnung;
        }

        /// <summary>
        /// Liefert den Namen des Hauses inkl. Pronomen und optional Zustand z.B. 'Euer schönes Bürgerhaus' für Meldungen
        /// </summary>
        /// <param name="zustandInProzent">Optional: Zustand in Prozent. Wenn angegeben, wird auch die entsprechende Zustandsbezeichnung zurückgegeben</param>
        /// <returns>Name inkl. Pronomen</returns>
        public string GetNameInklPronomen(int zustandInProzent = -1)
        {
            string zustandsbezeichnung = "";

            if (zustandInProzent != -1)
                zustandsbezeichnung = GetZustandsbezeichnung(zustandInProzent) + " ";

            return _pronomen + " " + zustandsbezeichnung + Name;
        }

        /// <summary>
        /// Liefert die Bezeichnung des Zustandes des Hauses zurück, abhängig vom Zustand in Prozent.
        /// </summary>
        /// <param name="zustandInProzent">Der Wert des Zustandes in Prozent</param>
        /// <returns>Bezeichnung des Zustandes</returns>
        public string GetZustandsbezeichnung(int zustandInProzent)
        {
            string zustandsbezeichnung = "";

            if (_hausZustandsbezeichnung != null)
            {
                for (int i = 0; i < _hausZustandsbezeichnung.GetLength(0); i++)
                {
                    if (zustandInProzent >= _hausZustandsbezeichnung[i].VonProzent &&
                        zustandInProzent <= _hausZustandsbezeichnung[i].BisProzent)
                    {
                        zustandsbezeichnung = _hausZustandsbezeichnung[i].Bezeichnung;
                        break;
                    }
                }
            }

            return zustandsbezeichnung;
        }
    }
}