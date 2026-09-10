using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Der Faktor: ein angestellter Handelsvertreter, der Übersicht und Vorausschau verkauft – nicht
    /// Ertrag.
    ///
    /// <b>Was er kann und was er ausdrücklich nicht tut.</b> Er stellt den Vergleich aller Städte für
    /// eine Ware in einer Tabelle zusammen und schätzt, wie lange die eigene Lieferung einen Markt noch
    /// verträgt. Er erklärt den Mengenabschlag <b>nicht</b>: Der wirkt ab der ersten Runde, und wer ihn
    /// nicht sieht, würde für eine unsichtbare Regel bestraft – deshalb steht die Ursache seit jeher im
    /// Tooltip der Preiszeile und ist gratis. Verkauft wird nur die Bequemlichkeit, alles nebeneinander
    /// zu sehen, statt vierzehn Städte einzeln durchzuschalten.
    ///
    /// <b>Warum er die bessere Geldsenke ist als die Produktionsfertigkeit.</b> Sein Nutzen kumuliert
    /// nicht: Er kostet jedes Jahr aufs Neue und macht den Spieler nicht reicher, sondern klüger. Die
    /// Fertigkeit dagegen ist Fortschritt – einmal bezahlt, für immer wirksam – und darf deshalb nur
    /// klein sein. Sein Lohn wächst mit dem Betrieb, weil auch seine Arbeit damit wächst.
    ///
    /// Siehe docs/produktionsfertigkeit-konzept.md im Godot-Repo.
    /// </summary>
    public class FaktorManager
    {
        /// <summary>Was der Faktor im Jahr kostet, bevor der Betrieb dazukommt.</summary>
        public const int Grundlohn = 750;

        /// <summary>
        /// Aufschlag je Stadt, in der der Spieler eine Werkstätte besitzt. Damit liegt der Lohn bei sechs
        /// Städten schon bei rund 2 850 im Jahr und damit weit über dem
        /// <see cref="AbrechnungsManager.GrundunterhaltProWerkstatt"/> von 100 – er soll spürbar sein,
        /// sonst wäre er keine Senke.
        /// </summary>
        public const int LohnJeStadt = 350;

        #region Anstellung

        [PublicAPI]
        public bool IstAngestellt()
        {
            return SW.Dynamisch.GetAktHum().FaktorAngestellt;
        }

        /// <summary>
        /// Was der Faktor im laufenden Jahr kostet. Fällig wird er in der Jahresabrechnung, nicht beim
        /// Anstellen: Er wird für ein Jahr Arbeit bezahlt, nicht für den Handschlag.
        /// </summary>
        [PublicAPI]
        public int GetJahreslohn()
        {
            return Grundlohn + LohnJeStadt * ZaehleStaedteMitWerkstatt();
        }

        /// <summary>
        /// Stellt den Faktor an. Er kostet nichts im Voraus – geprüft wird nur, dass nicht schon einer
        /// im Dienst steht.
        /// </summary>
        [PublicAPI]
        public bool StelleAn(out string meldung)
        {
            if (IstAngestellt())
            {
                meldung = "Ihr habt bereits einen Faktor in Diensten.";
                return false;
            }

            SW.Dynamisch.GetAktHum().FaktorAngestellt = true;

            meldung = "Der Faktor tritt in Euren Dienst. Sein Lohn von " + GetJahreslohn() +
                      " Talern wird mit der Jahresabrechnung fällig und wächst mit Eurem Betrieb.";
            return true;
        }

        [PublicAPI]
        public void Entlasse()
        {
            SW.Dynamisch.GetAktHum().FaktorAngestellt = false;
        }

        /// <summary>
        /// In wie vielen Städten der Spieler mindestens eine Werkstätte hält. Nicht die Zahl der
        /// Werkstätten: Was den Faktor beschäftigt, sind die Märkte, die er im Blick behalten muss.
        /// </summary>
        private static int ZaehleStaedteMitWerkstatt()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int staedte = 0;

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
                {
                    if (spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId).GetEnabled())
                    {
                        staedte++;
                        break;
                    }
                }
            }

            return staedte;
        }

        #endregion

        #region Marktübersicht

        /// <summary>
        /// Der Vergleich aller Städte für eine Ware. Den gibt es sonst nirgends: Die Zielstadt eines
        /// Exports wird einzeln durchgeschaltet, und wer wissen will, wo eine Ware am meisten einbringt,
        /// muss vierzehn Städte nacheinander aufsuchen.
        ///
        /// Sortiert nach dem Marktpreis, absteigend – die Frage, die man an eine solche Tabelle stellt,
        /// lautet fast immer „wohin damit".
        /// </summary>
        [PublicAPI]
        public List<FaktorMarktzeile> ErstelleMarktuebersicht(int rohstoffId)
        {
            var zeilen = new List<FaktorMarktzeile>();

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                zeilen.Add(ErstelleZeile(stadtId, rohstoffId));

            zeilen.Sort((a, b) => b.Marktpreis.CompareTo(a.Marktpreis));

            return zeilen;
        }

        /// <summary>Eine einzelne Zeile – auch außerhalb der Tabelle brauchbar, etwa für die Warnungen.</summary>
        [PublicAPI]
        public FaktorMarktzeile ErstelleZeile(int stadtId, int rohstoffId)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);

            int vorrat = stadt.GetRohstoffIDXVorrat(rohstoffId);
            int jahresbedarf = stadt.GetJahresbedarf();
            int lieferung = ErmittleEigeneJahreslieferung(stadtId, rohstoffId);
            int abschlag = stadt.GetAbschlagProzentVonIDX(rohstoffId);

            return new FaktorMarktzeile
            {
                StadtId = stadtId,
                StadtName = stadt.GetGebietsName(),
                Grundpreis = stadt.GetRohstoffBasispreisVonIDX(rohstoffId),
                Marktpreis = stadt.GetRohstoffPreisVonIDX(rohstoffId),
                AbschlagProzent = abschlag,
                Vorrat = vorrat,
                Jahresbedarf = jahresbedarf,
                EigeneJahreslieferung = lieferung,
                EigeneWerkstatt = HatWerkstattFuer(stadtId, rohstoffId),
                AmHoechstabschlag = abschlag >= Gameplay.Gebiete.Stadt.MaxAbschlagProzent,
                JahreBisHoechstabschlag = SchaetzeJahreBisHoechstabschlag(stadtId, rohstoffId, lieferung)
            };
        }

        /// <summary>
        /// Die Prognose: Bei welcher eigenen Jahreslieferung ist der Höchstabschlag wann erreicht?
        ///
        /// Der Überhang wächst um Lieferung minus Jahresbedarf – die Stadt verbraucht ja, was sie
        /// aufnimmt. Liefert man nicht mehr als den Bedarf, wächst er gar nicht, und der Deckel wird nie
        /// erreicht. Sonst ist die Antwort <c>(Deckelvorrat − Vorrat) / (Lieferung − Bedarf)</c>,
        /// aufgerundet, denn ein angebrochenes Jahr ist eines.
        ///
        /// <b>Eine Schätzung, kein Versprechen.</b> Sie kennt nur die eigenen Aufträge; was andere in
        /// dieselbe Stadt liefern, sieht sie ebenso wenig wie die jährliche Preisschwankung. Der Client
        /// muss das sagen, sonst liest sich die Zahl als Zusage.
        /// </summary>
        [PublicAPI]
        public int SchaetzeJahreBisHoechstabschlag(int stadtId, int rohstoffId, int jahreslieferung)
        {
            var stadt = SW.Dynamisch.GetStadtwithID(stadtId);

            if (stadt.GetAbschlagProzentVonIDX(rohstoffId) >= Gameplay.Gebiete.Stadt.MaxAbschlagProzent)
                return 0;

            int zuwachs = jahreslieferung - stadt.GetJahresbedarf();

            if (zuwachs <= 0)
                return FaktorMarktzeile.NieErreicht;

            int fehlend = stadt.GetVorratAmHoechstabschlag() - stadt.GetRohstoffIDXVorrat(rohstoffId);

            if (fehlend <= 0)
                return 0;

            // Aufrunden ohne Fliesskomma: Ein angebrochenes Jahr ist eines.
            return (fehlend + zuwachs - 1) / zuwachs;
        }

        /// <summary>
        /// Was der Spieler laut seinen Verkaufsaufträgen im Jahr in diese Stadt bringt.
        ///
        /// Gezählt werden die Exportaufträge, denn nur sie buchen in den Vorrat einer <b>anderen</b>
        /// Stadt und sättigen deren Markt planmäßig. Der Verkauf auf der Stelle (Klick auf das
        /// Warensymbol) räumt das Lager sofort und ist keine wiederkehrende Lieferung, die sich
        /// fortschreiben ließe.
        /// </summary>
        [PublicAPI]
        public int ErmittleEigeneJahreslieferung(int stadtId, int rohstoffId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int menge = 0;

            for (int herkunft = 1; herkunft < SW.Statisch.GetMaxStadtID(); herkunft++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(herkunft, slot);
                    int taetigkeit = produktionsslot.GetTaetigkeit();

                    if (taetigkeit != (int)EnumProduktionsslotAktionsart.Verkaufen &&
                        taetigkeit != (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                        continue;

                    if (produktionsslot.GetVerkaufStadt() != stadtId ||
                        produktionsslot.GetVerkaufRohstoff() != rohstoffId)
                        continue;

                    menge += produktionsslot.GetVerkaufAnzahl();
                }
            }

            return menge;
        }

        #endregion

        #region Warnungen

        /// <summary>
        /// Wovor der Faktor zu Jahresbeginn warnt: belieferte Märkte, die den Höchstabschlag erreicht
        /// haben oder ihn im kommenden Jahr erreichen. Das ist die Auskunft, für die man ihn hält – sie
        /// kommt von selbst, statt auf die Frage zu warten, die der Spieler nicht stellt, weil er den
        /// Grund nicht kennt.
        ///
        /// Ohne Faktor keine Warnung: Eine leere Liste, damit der Aufrufer nicht zu prüfen braucht.
        /// </summary>
        [PublicAPI]
        public List<string> ErmittleWarnungen()
        {
            var meldungen = new List<string>();

            if (!IstAngestellt())
                return meldungen;

            foreach (var ziel in ErmittleBelieferteMaerkte())
            {
                var zeile = ErstelleZeile(ziel.Key, ziel.Value);
                string ware = SW.Dynamisch.GetRohstoffwithID(ziel.Value).GetRohName();

                if (zeile.AmHoechstabschlag)
                {
                    meldungen.Add("Der Markt für " + ware + " in " + zeile.StadtName +
                                  " ist gesättigt: Der Abschlag steht am Höchstwert von " +
                                  zeile.AbschlagProzent + " %, und weitere Lieferungen drücken ihn nicht " +
                                  "tiefer – bringen Euch aber auch nicht mehr ein.");
                }
                else if (zeile.JahreBisHoechstabschlag > 0 && zeile.JahreBisHoechstabschlag <= 1)
                {
                    meldungen.Add("Der Markt für " + ware + " in " + zeile.StadtName +
                                  " ist bald gesättigt: Bei Eurer Liefermenge von " +
                                  zeile.EigeneJahreslieferung + " im Jahr gegen einen Verbrauch von " +
                                  zeile.Jahresbedarf + " ist der Höchstabschlag im nächsten Jahr erreicht.");
                }
            }

            return meldungen;
        }

        /// <summary>Die Ware-Stadt-Paare, in die der Spieler gerade liefert – je Paar nur einmal.</summary>
        private static IEnumerable<KeyValuePair<int, int>> ErmittleBelieferteMaerkte()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var gesehen = new List<KeyValuePair<int, int>>();

            for (int herkunft = 1; herkunft < SW.Statisch.GetMaxStadtID(); herkunft++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(herkunft, slot);
                    int taetigkeit = produktionsslot.GetTaetigkeit();

                    if (taetigkeit != (int)EnumProduktionsslotAktionsart.Verkaufen &&
                        taetigkeit != (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                        continue;

                    if (produktionsslot.GetVerkaufAnzahl() <= 0)
                        continue;

                    var paar = new KeyValuePair<int, int>(produktionsslot.GetVerkaufStadt(),
                                                          produktionsslot.GetVerkaufRohstoff());

                    if (paar.Key <= 0 || paar.Value <= 0 || gesehen.Contains(paar))
                        continue;

                    gesehen.Add(paar);
                }
            }

            return gesehen;
        }

        #endregion

        #region Hilfsfunktionen

        private static bool HatWerkstattFuer(int stadtId, int rohstoffId)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            for (int nr = 1; nr <= SW.Statisch.GetMaxWerkstaettenProStadt(); nr++)
            {
                var werkstatt = spieler.GetSpielerHatInStadtXWerkstaettenY(nr, stadtId);

                if (werkstatt.GetEnabled() &&
                    SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(nr) == rohstoffId)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
