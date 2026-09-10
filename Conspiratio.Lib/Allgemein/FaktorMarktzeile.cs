using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Was der Faktor über eine Ware in einer Stadt zu berichten weiß – eine Zeile seiner Übersicht.
    ///
    /// Bewusst reine Daten ohne Formulierung: Der Faktor liefert die Zahlen, die Sprache gehört in den
    /// Client. Sonst ließe sich die Übersicht nicht als Tabelle setzen, und genau die ist ihr Zweck.
    /// </summary>
    public class FaktorMarktzeile
    {
        /// <summary>Wert von <see cref="JahreBisHoechstabschlag"/>, wenn der Deckel nie erreicht wird.</summary>
        public const int NieErreicht = -1;

        public int StadtId { get; internal set; }

        public string StadtName { get; internal set; }

        /// <summary>Der Preis ohne Mengenabschlag.</summary>
        public int Grundpreis { get; internal set; }

        /// <summary>Was die Stadt tatsächlich zahlt – Grundpreis abzüglich Abschlag.</summary>
        public int Marktpreis { get; internal set; }

        /// <summary>Der wirksame, auf den Deckel gekappte Abschlag in Prozentpunkten.</summary>
        public int AbschlagProzent { get; internal set; }

        /// <summary>Was dort an Ware liegt.</summary>
        public int Vorrat { get; internal set; }

        /// <summary>Was die Stadt im Jahr davon verbraucht.</summary>
        public int Jahresbedarf { get; internal set; }

        /// <summary>Was der Spieler selbst im Jahr dorthin liefert (aus seinen Verkaufsaufträgen).</summary>
        public int EigeneJahreslieferung { get; internal set; }

        /// <summary>Ob der Spieler in dieser Stadt eine Werkstätte für diese Ware besitzt.</summary>
        public bool EigeneWerkstatt { get; internal set; }

        /// <summary>Ob der Abschlag hier bereits am Deckel steht.</summary>
        public bool AmHoechstabschlag { get; internal set; }

        /// <summary>
        /// Wie viele Jahre es bei der derzeitigen eigenen Lieferung dauert, bis der Höchstabschlag
        /// erreicht ist. 0 heißt: bereits erreicht. <see cref="NieErreicht"/> heißt, dass die Lieferung
        /// den Jahresbedarf nicht übersteigt – der Überhang wächst dann nicht.
        ///
        /// <b>Eine Schätzung, kein Versprechen:</b> Sie rechnet nur mit der eigenen Lieferung und
        /// ignoriert, was andere in dieselbe Stadt bringen, ebenso die jährliche Preisschwankung.
        /// </summary>
        public int JahreBisHoechstabschlag { get; internal set; }
    }
}
