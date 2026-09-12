using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.Untergebene
{
    /// <summary>Ein Untergebener des aktiven Spielers (für die Anzeige in der Liste).</summary>
    public class UntergebenerInfo
    {
        public int Id { get; }
        public string Name { get; }

        public UntergebenerInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Kapselt die Logik von UntergebeneForm/UntergebenenOptionen: die Untergebenen des aktiven
    /// Spielers (durch dessen Amt bestimmt) sowie die einzige echte Aktion – die Einleitung einer
    /// Amtsenthebung gegen einen Untergebenen.
    /// </summary>
    public class UntergebeneManager
    {
        /// <summary>Text, wenn der Spieler keine Untergebenen hat.</summary>
        public string KeineUntergebeneText => "Ihr habt keine Untergebenen";

        /// <summary>Die Untergebenen des aktiven Spielers (leere Slots werden weggelassen).</summary>
        public List<UntergebenerInfo> GetUntergebene()
        {
            var result = new List<UntergebenerInfo>();
            int[] ids = SW.Dynamisch.GetUntergebene(SW.Dynamisch.GetAktiverSpieler());

            foreach (int id in ids)
            {
                if (id == 0)
                    break; // wie im Original: die erste 0 beendet die Liste

                result.Add(new UntergebenerInfo(id, SW.Dynamisch.GetSpWithID(id).GetKompletterName()));
            }

            return result;
        }

        /// <summary>Frage der Optionsauswahl ("Was wollt Ihr &lt;Name&gt; antun?").</summary>
        public string GetOptionenFrage(int id) =>
            "Was wollt Ihr " + SW.Dynamisch.GetSpWithID(id).GetName() + " antun?";

        /// <summary>
        /// Leitet eine Amtsenthebung gegen den Untergebenen ein und liefert die Ergebnismeldung.
        /// (Ist bereits eine Enthebung im Gange, bleibt das intern wirkungslos – wie im Original.)
        ///
        /// Die Antragssperre gilt für den Menschen wie für die KI: Wer eben erst gegen jemanden vorging,
        /// muss warten. Sonst wäre das, was der KI verwehrt ist, dem Spieler als Dauerfeuer erlaubt.
        /// Das Jahr steht in der Meldung, damit die Sperre nicht als kaputter Knopf erscheint.
        /// </summary>
        public string LeiteAmtsenthebungEin(int id)
        {
            int antragsteller = SW.Dynamisch.GetAktiverSpieler();
            int gesperrtBis = SW.Dynamisch.GetAbsetzungGesperrtBis(antragsteller, id);

            if (gesperrtBis > 0)
                return "Gegen " + SW.Dynamisch.GetSpWithID(id).GetKompletterName() +
                       " habt Ihr bereits ein Verfahren angestrengt. Ein neues könnt Ihr erst im Jahre " +
                       gesperrtBis + " betreiben.";

            SW.Dynamisch.SetAmtsenthebungVonID(id, antragsteller);
            return "Eine Amtsenthebung von " + SW.Dynamisch.GetSpWithID(id).GetKompletterName() +
                   " wird in die Wege geleitet...";
        }
    }
}
