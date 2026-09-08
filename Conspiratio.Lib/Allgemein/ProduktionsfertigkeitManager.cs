using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Wie ein Spieler seine Produktionsfertigkeit erwirbt – und wieder verliert.
    ///
    /// Die Fertigkeit selbst sitzt auf <see cref="HumSpieler"/> und wirkt in
    /// <c>Produktionsslot.GetProduktion</c>; hier stehen ausschließlich die Wege dorthin. Sie sind
    /// bewusst verschieden im Charakter, damit die Fertigkeit nicht bloß eine weitere Sache ist, die
    /// man kauft:
    ///
    /// <list type="bullet">
    /// <item><b>Bücher</b> sind planbar und billig, aber ortsgebunden – es gibt sie dort, wo die Ware
    /// zur Hauptproduktion gehört. Damit lohnt Reisen, ohne dass es dafür eine eigene Mechanik
    /// bräuchte.</item>
    /// <item><b>Meisterschriften</b> sind nicht käuflich, nur zu finden.</item>
    /// <item><b>Vorträge</b> sind eine Entscheidung mit Risiko: Der Eintritt ist fällig, bevor sich
    /// zeigt, ob der Redner etwas zu sagen hatte.</item>
    /// <item><b>Lehrstunden</b> sind der verlässliche, teure Weg. Der Preis steigt mit jeder genommenen
    /// Stunde derselben Ware – dasselbe Muster wie beim Fechtunterricht.</item>
    /// <item>Der <b>Hausgelehrte</b> lehrt von selbst, verlangt aber ein Schloss und Jahreslohn.</item>
    /// <item>Wer eine Ware jahrelang nicht anfasst, <b>verlernt</b> sie wieder.</item>
    /// </list>
    ///
    /// Siehe docs/produktionsfertigkeit-konzept.md im Godot-Repo.
    /// </summary>
    public class ProduktionsfertigkeitManager
    {
        /// <summary>Bit für das gelesene Grundlagenbuch in <c>HumSpieler.GetFertigkeitsschriftenSicher</c>.</summary>
        public const int SchriftBuch = 1;

        /// <summary>Bit für die gelesene Meisterschrift.</summary>
        public const int SchriftMeister = 2;

        public const int BuchGewinn = 10;
        public const int MeisterschriftGewinn = 15;
        public const int VortragGewinn = 4;
        public const int LehrstundeGewinn = 5;

        /// <summary>Grundpreis eines Buches, multipliziert mit der Warenstufe (1 bis 3).</summary>
        public const int BuchGrundpreis = 2000;

        public const int VortragEintritt = 1500;

        /// <summary>Wie wahrscheinlich im Jahr eine Einladung zu einer produzierten Ware eintrifft.</summary>
        public const int VortragChanceProzent = 12;

        /// <summary>
        /// Wie oft der Redner ein Schwätzer ist. Das Geld ist dann fort, die Fertigkeit unverändert –
        /// das ist die Stelle, an der ein Erwerbsweg schiefgehen darf, ohne dem Spieler etwas zu nehmen,
        /// das er schon hatte.
        /// </summary>
        public const int SchwaetzerChanceProzent = 17;

        /// <summary>Wie wahrscheinlich im Jahr eine Meisterschrift gefunden wird.</summary>
        public const int MeisterschriftFundChanceProzent = 4;

        public const int GrundpreisLehrstunde = 1000;
        public const int PreisSteigerungProLehrstunde = 500;

        public const int HausgelehrterGewinn = 2;
        public const int HausgelehrterJahreslohn = 3000;

        /// <summary>Bis hierher lehrt der Hausgelehrte; die letzte Strecke verlangt eigenes Zutun.</summary>
        public const int HausgelehrterObergrenze = 80;

        /// <summary>Ein Schloss (Hausstufe 9) beherbergt einen Gelehrten, weniger nicht.</summary>
        public const int HausgelehrterHausstufe = 9;

        public const int VerlernenNachJahren = 3;
        public const int VerlernenProJahr = 3;

        #region Anzeige

        /// <summary>
        /// Die Fertigkeit in Worten. <b>Bewusst andere Wörter als die Qualitätsangabe des Jahresbuchs</b>
        /// (<c>BuchManager</c>: „schlecht" bis „ausgezeichnet"): Jene beschreibt das einzelne Jahr, diese
        /// das Können. Gleiche Wörter für zweierlei Bedeutungen ließen sich nicht auseinanderhalten.
        /// </summary>
        [PublicAPI]
        public static string FertigkeitAlsText(int fertigkeit)
        {
            if (fertigkeit < 15)
                return "unkundig";

            if (fertigkeit < 30)
                return "laienhaft";

            if (fertigkeit < 45)
                return "mittelmäßig";

            if (fertigkeit < 60)
                return "annehmbar";

            if (fertigkeit < 75)
                return "tüchtig";

            if (fertigkeit < 90)
                return "meisterlich";

            return "vollendet";
        }

        #endregion

        #region Bücher

        /// <summary>
        /// Ob es zu dieser Ware in dieser Stadt ein Buch zu kaufen gibt: nur wo die Ware zur
        /// Hauptproduktion gehört, und nur solange der Spieler es nicht schon gelesen hat.
        ///
        /// <c>GetHauptproduktion</c> liefert die drei effizientesten Waren der Stadt – dieselbe Liste,
        /// die auch die Stadtinformationen zeigen. Das Wissen sitzt also dort, wo es sichtbar
        /// hergestellt wird, und der Spieler kann den Ort ablesen, statt ihn zu suchen.
        /// </summary>
        [PublicAPI]
        public bool GibtEsBuch(int stadtId, int rohstoffId)
        {
            if (HatSchrift(rohstoffId, SchriftBuch))
                return false;

            return Array.IndexOf(SW.Dynamisch.GetStadtwithID(stadtId).GetHauptproduktion(), rohstoffId) >= 0;
        }

        /// <summary>Die Waren, zu denen es in dieser Stadt gerade ein Buch gibt.</summary>
        [PublicAPI]
        public List<int> GetBuchangebote(int stadtId)
        {
            var angebote = new List<int>();

            foreach (int rohstoffId in SW.Dynamisch.GetStadtwithID(stadtId).GetHauptproduktion())
            {
                if (rohstoffId > 0 && !HatSchrift(rohstoffId, SchriftBuch))
                    angebote.Add(rohstoffId);
            }

            return angebote;
        }

        /// <summary>Der Preis richtet sich nach der Warenstufe: einfache Waren lehren sich billiger.</summary>
        [PublicAPI]
        public int GetBuchpreis(int rohstoffId)
        {
            return BuchGrundpreis * SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetRohStufe();
        }

        /// <summary>Kauft und liest das Buch. Liefert false samt Begründung, wenn das nicht geht.</summary>
        [PublicAPI]
        public bool KaufeBuch(int stadtId, int rohstoffId, out string meldung)
        {
            if (!GibtEsBuch(stadtId, rohstoffId))
            {
                meldung = "Hier ist zu dieser Ware nichts zu holen.";
                return false;
            }

            var spieler = SW.Dynamisch.GetAktHum();
            int preis = GetBuchpreis(rohstoffId);

            if (spieler.GetTaler() < preis)
            {
                meldung = "Dafür fehlen Euch die Taler.";
                return false;
            }

            spieler.ErhoeheTaler(-preis);
            spieler.GetFertigkeitsschriftenSicher()[rohstoffId] |= SchriftBuch;
            spieler.ErhoeheProduktionsfertigkeit(rohstoffId, BuchGewinn);

            meldung = "Ihr habt " + preis.ToStringGeld() + " für ein Buch über " + WareName(rohstoffId) +
                      " ausgegeben. Euer Können darin ist jetzt " +
                      FertigkeitAlsText(spieler.GetProduktionsfertigkeit(rohstoffId)) + ".";
            return true;
        }

        #endregion

        #region Lehrstunden

        /// <summary>
        /// Was die nächste Stunde für diese Ware kostet. Der Preis steigt mit jeder bereits genommenen –
        /// eine Ware von 0 bis zur Vollendung zu lernen kostet damit rund 115 000 Taler, und niemand
        /// unterrichtet nebenbei alle Waren des Spiels.
        /// </summary>
        [PublicAPI]
        public int GetLehrstundenPreis(int rohstoffId)
        {
            return GrundpreisLehrstunde +
                   SW.Dynamisch.GetAktHum().GetLehrstundenSicher()[rohstoffId] * PreisSteigerungProLehrstunde;
        }

        /// <summary>Nimmt eine Lehrstunde. Liefert false samt Begründung, wenn das nicht geht.</summary>
        [PublicAPI]
        public bool NimmLehrstunde(int rohstoffId, out string meldung)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.GetProduktionsfertigkeit(rohstoffId) >= HumSpieler.MaxProduktionsfertigkeit)
            {
                meldung = "In " + WareName(rohstoffId) + " kann Euch niemand mehr etwas beibringen.";
                return false;
            }

            int preis = GetLehrstundenPreis(rohstoffId);

            if (spieler.GetTaler() < preis)
            {
                meldung = "Dafür fehlen Euch die Taler.";
                return false;
            }

            spieler.ErhoeheTaler(-preis);
            spieler.GetLehrstundenSicher()[rohstoffId]++;
            spieler.ErhoeheProduktionsfertigkeit(rohstoffId, LehrstundeGewinn);

            meldung = "Die Stunde kostete " + preis.ToStringGeld() + ". Euer Können in " +
                      WareName(rohstoffId) + " ist jetzt " +
                      FertigkeitAlsText(spieler.GetProduktionsfertigkeit(rohstoffId)) + ".";
            return true;
        }

        #endregion

        #region Jahresereignisse

        /// <summary>
        /// Die Ereignisse eines Jahres: Verlernen, Hausgelehrter, Schriftfund und Vortrag. Wird einmal je
        /// Zug aufgerufen und liefert die Meldungen, die der Client anzeigt.
        ///
        /// Asynchron, weil der Vortrag eine Entscheidung verlangt – er fragt über
        /// <c>SW.UI.YesNoQuestion</c>, wie es auch <c>Stuetzpunkt.KaufangebotAbgeben</c> tut.
        /// </summary>
        [PublicAPI]
        public async Task<List<string>> FuehreJahresereignisseDurch()
        {
            var meldungen = new List<string>();
            var produziert = ProduzierteWaren();

            VermerkeProduktion(produziert);
            Verlerne(meldungen);
            LasseHausgelehrtenLehren(meldungen);
            FindeMeisterschrift(produziert, meldungen);

            string vortrag = await BesucheVortrag(produziert);

            if (vortrag != null)
                meldungen.Add(vortrag);

            return meldungen;
        }

        /// <summary>Hält fest, welche Waren der Spieler dieses Jahr betreibt – Grundlage des Verlernens.</summary>
        private static void VermerkeProduktion(List<int> produziert)
        {
            var letzte = SW.Dynamisch.GetAktHum().GetLetzteProduktionSicher();
            int jahr = SW.Dynamisch.GetAktuellesJahr();

            foreach (int rohstoffId in produziert)
                letzte[rohstoffId] = jahr;
        }

        /// <summary>
        /// Wer eine Ware <see cref="VerlernenNachJahren"/> Jahre nicht angefasst hat, kommt aus der
        /// Übung. Das ist der ehrliche Weg nach unten: nachvollziehbar und abwendbar – anders als ein
        /// Zufallsereignis, das die Fertigkeit ohne Zutun des Spielers senkt. Nebenbei erzwingt es die
        /// Spezialisierung, die zur Marktsättigung passt: Wer alles kann, sättigt jeden Markt, den er
        /// beliefert.
        /// </summary>
        private static void Verlerne(List<string> meldungen)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var letzte = spieler.GetLetzteProduktionSicher();
            int jahr = SW.Dynamisch.GetAktuellesJahr();

            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
            {
                if (spieler.GetProduktionsfertigkeit(rohstoffId) <= 0)
                    continue;

                // Noch nie produziert: Buchwissen soll nicht verfallen, bevor es einmal angewandt wurde.
                if (letzte[rohstoffId] == 0)
                    continue;

                if (jahr - letzte[rohstoffId] < VerlernenNachJahren)
                    continue;

                spieler.ErhoeheProduktionsfertigkeit(rohstoffId, -VerlernenProJahr);

                meldungen.Add("Ihr habt " + WareName(rohstoffId) + " lange nicht mehr hergestellt und " +
                              "kommt aus der Übung. Euer Können ist nur noch " +
                              FertigkeitAlsText(spieler.GetProduktionsfertigkeit(rohstoffId)) + ".");
            }
        }

        /// <summary>
        /// Der Hausgelehrte im Schloss: Er hebt jedes Jahr die Ware, in der der Spieler ohnehin am
        /// weitesten ist – Meisterschaft vertieft sich, sie verteilt sich nicht. Über
        /// <see cref="HausgelehrterObergrenze"/> hinaus lehrt er nicht, und ohne jede Vorkenntnis findet
        /// er keinen Anknüpfungspunkt; die letzte Strecke wie der erste Schritt verlangen eigenes Zutun.
        /// </summary>
        private static void LasseHausgelehrtenLehren(List<string> meldungen)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (!spieler.BesitztSpielerFertigesHausAbStufe(HausgelehrterHausstufe))
                return;

            int beste = 0;
            int besteFertigkeit = 0;

            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
            {
                int fertigkeit = spieler.GetProduktionsfertigkeit(rohstoffId);

                if (fertigkeit < HausgelehrterObergrenze && fertigkeit > besteFertigkeit)
                {
                    besteFertigkeit = fertigkeit;
                    beste = rohstoffId;
                }
            }

            if (beste == 0)
                return;

            if (spieler.GetTaler() < HausgelehrterJahreslohn)
            {
                meldungen.Add("Euer Hausgelehrter blieb unbezahlt und hat das Schloss verlassen.");
                return;
            }

            spieler.ErhoeheTaler(-HausgelehrterJahreslohn);
            spieler.ErhoeheProduktionsfertigkeit(beste, HausgelehrterGewinn);

            meldungen.Add("Euer Hausgelehrter kostete " + HausgelehrterJahreslohn.ToStringGeld() +
                          " und hat Euch in " + WareName(beste) + " unterwiesen. Euer Können ist jetzt " +
                          FertigkeitAlsText(spieler.GetProduktionsfertigkeit(beste)) + ".");
        }

        /// <summary>Eine Meisterschrift lässt sich nicht kaufen, nur finden – und je Ware nur einmal.</summary>
        private static void FindeMeisterschrift(List<int> produziert, List<string> meldungen)
        {
            if (SW.Statisch.Rnd.Next(0, 100) >= MeisterschriftFundChanceProzent)
                return;

            var spieler = SW.Dynamisch.GetAktHum();
            var kandidaten = new List<int>();

            foreach (int rohstoffId in produziert)
            {
                if ((spieler.GetFertigkeitsschriftenSicher()[rohstoffId] & SchriftMeister) == 0)
                    kandidaten.Add(rohstoffId);
            }

            if (kandidaten.Count == 0)
                return;

            int gefunden = kandidaten[SW.Statisch.Rnd.Next(0, kandidaten.Count)];

            spieler.GetFertigkeitsschriftenSicher()[gefunden] |= SchriftMeister;
            spieler.ErhoeheProduktionsfertigkeit(gefunden, MeisterschriftGewinn);

            meldungen.Add("In einer Truhe fand sich eine alte Meisterschrift über " + WareName(gefunden) +
                          ". Euer Können ist jetzt " +
                          FertigkeitAlsText(spieler.GetProduktionsfertigkeit(gefunden)) + ".");
        }

        /// <summary>
        /// Die Einladung zu einem Vortrag über eine Ware, die der Spieler betreibt. Der Eintritt ist
        /// fällig, bevor sich zeigt, ob der Redner etwas zu sagen hatte – ablehnen kostet nichts.
        /// </summary>
        private static async Task<string> BesucheVortrag(List<int> produziert)
        {
            if (SW.Statisch.Rnd.Next(0, 100) >= VortragChanceProzent)
                return null;

            var spieler = SW.Dynamisch.GetAktHum();
            var kandidaten = new List<int>();

            foreach (int rohstoffId in produziert)
            {
                if (spieler.GetProduktionsfertigkeit(rohstoffId) < HumSpieler.MaxProduktionsfertigkeit)
                    kandidaten.Add(rohstoffId);
            }

            if (kandidaten.Count == 0)
                return null;

            // Nicht fragen, was der Spieler ohnehin nicht bezahlen kann - das waere eine Frage ohne
            // Antwortmoeglichkeit. (Und deshalb hier GetTaler statt CheckIfenoughGold: Letzteres zeigt
            // von sich aus einen Hinweis an, den es an dieser Stelle nicht braucht.)
            if (spieler.GetTaler() < VortragEintritt)
                return null;

            int ware = kandidaten[SW.Statisch.Rnd.Next(0, kandidaten.Count)];

            var antwort = await SW.UI.YesNoQuestion.ShowDialogText(
                "Ein Gelehrter kündigt einen Vortrag über\n" + WareName(ware) + " an.\n\n" +
                "Der Eintritt kostet " + VortragEintritt.ToStringGeld() + ".\nWollt Ihr ihn hören?",
                "Ja", "Lieber nicht!");

            if (antwort != DialogResultGame.Yes)
                return null;

            spieler.ErhoeheTaler(-VortragEintritt);

            if (SW.Statisch.Rnd.Next(0, 100) < SchwaetzerChanceProzent)
                return "Der Redner erwies sich als Schwätzer. Der Eintritt war vertan.";

            spieler.ErhoeheProduktionsfertigkeit(ware, VortragGewinn);

            return "Der Vortrag über " + WareName(ware) + " war sein Geld wert. Euer Können ist jetzt " +
                   FertigkeitAlsText(spieler.GetProduktionsfertigkeit(ware)) + ".";
        }

        #endregion

        #region Hilfsfunktionen

        /// <summary>Die Waren, die der Spieler gerade herstellt – je Ware nur einmal.</summary>
        private static List<int> ProduzierteWaren()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var waren = new List<int>();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if (produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.Produzieren)
                        continue;

                    if (produktionsslot.GetProduktionStaetten() <= 0)
                        continue;

                    int rohstoffId = produktionsslot.GetProduktionRohstoff();

                    if (rohstoffId > 0 && !waren.Contains(rohstoffId))
                        waren.Add(rohstoffId);
                }
            }

            return waren;
        }

        private static string WareName(int rohstoffId) => SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetRohName();

        private static bool HatSchrift(int rohstoffId, int bit)
            => (SW.Dynamisch.GetAktHum().GetFertigkeitsschriftenSicher()[rohstoffId] & bit) != 0;

        #endregion
    }
}
