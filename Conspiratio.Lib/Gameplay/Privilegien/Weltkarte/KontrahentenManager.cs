using System.Collections.Generic;
using System.Threading.Tasks;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.Weltkarte
{
    /// <summary>Ein möglicher Kontrahent (Ziel) für eine Weltkarte-Aktion.</summary>
    public class KontrahentInfo
    {
        public int Id { get; }
        public string Name { get; }
        public bool IstMensch { get; }

        public KontrahentInfo(int id, string name, bool istMensch)
        {
            Id = id;
            Name = name;
            IstMensch = istMensch;
        }
    }

    /// <summary>
    /// Die Detailangaben eines Kontrahenten für die reine Übersicht (Migration von KontrahentDetails).
    /// Name, Titel, Alter und Amt sind immer bekannt; Vermögen, Gesundheit, Beweislast (aufgedeckte Delikte)
    /// und der Erhebungs­stand liegen nur vor, wenn der aktive Spieler eine laufende Spionage gegen den
    /// Kontrahenten unterhält (<see cref="HatSpionage"/>).
    /// </summary>
    public class KontrahentDetailInfo
    {
        public string Name { get; }
        public string Titel { get; }
        public int Alter { get; }
        public string Amt { get; }

        public bool HatSpionage { get; }
        public int Vermoegen { get; }
        public string Gesundheit { get; }
        public int Delikte { get; }
        public int StandJahr { get; }

        public KontrahentDetailInfo(string name, string titel, int alter, string amt,
            bool hatSpionage, int vermoegen, string gesundheit, int delikte, int standJahr)
        {
            Name = name;
            Titel = titel;
            Alter = alter;
            Amt = amt;
            HatSpionage = hatSpionage;
            Vermoegen = vermoegen;
            Gesundheit = gesundheit;
            Delikte = delikte;
            StandJahr = standJahr;
        }
    }

    /// <summary>
    /// Kapselt die Logik von KontrahentenForm/UI.PersonWasMachen für die Privilegien-Modi der Weltkarte:
    /// liefert die wählbaren Kontrahenten (menschliche Mitspieler zuerst, dann die KI) und führt die
    /// zielgerichtete Aktion aus – Modus 8 = Prozess initiieren, Modus 13 = Hand des Henkers.
    /// </summary>
    public class KontrahentenManager
    {
        /// <summary>
        /// Die wählbaren Kontrahenten: zuerst die menschlichen Mitspieler (ohne den aktiven Spieler),
        /// danach alle KI-Spieler.
        /// </summary>
        public List<KontrahentInfo> GetKontrahenten()
        {
            var list = new List<KontrahentInfo>();

            for (int i = 1; i <= SW.Dynamisch.GetAktivSpielerAnzahl(); i++)
            {
                if (i != SW.Dynamisch.GetAktiverSpieler())
                    list.Add(new KontrahentInfo(i, SW.Dynamisch.GetSpWithID(i).GetCompleteNameOhneTitel(), true));
            }

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
                list.Add(new KontrahentInfo(i, SW.Dynamisch.GetSpWithID(i).GetCompleteNameOhneTitel(), false));

            return list;
        }

        /// <summary>
        /// Liefert die Detailangaben eines Kontrahenten für die Übersicht. Die Beweislast (aufgedeckte
        /// Delikte), Vermögen, Gesundheit und der Erhebungsstand werden nur mitgeliefert, wenn der aktive
        /// Spieler eine laufende Spionage gegen den Kontrahenten unterhält (wie im WinForms-Original).
        /// </summary>
        public KontrahentDetailInfo GetKontrahentDetails(int spielerId)
        {
            var kontrahent = SW.Dynamisch.GetSpWithID(spielerId);
            var spionage = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetAktiveSpionage(spielerId);

            bool hatSpionage = spionage.GetKosten() > 0 && spionage.GetDauer() > 1;

            return new KontrahentDetailInfo(
                kontrahent.GetName(),
                kontrahent.GetTitelGegendert(),
                kontrahent.GetAlter(),
                kontrahent.GetAmtNameUndOrt(),
                hatSpionage,
                hatSpionage ? kontrahent.GetGesamtVermoegen(spielerId) : 0,
                hatSpionage ? kontrahent.BeurteileGesundheitString() : "",
                hatSpionage ? spionage.GetDelikte() : 0,
                hatSpionage ? spionage.GetJahr() : 0);
        }

        /// <summary>
        /// Führt die dem Modus entsprechende Aktion auf den gewählten Kontrahenten aus
        /// (Migration von UI.PersonWasMachen, beschränkt auf die Privilegien-Modi 8 und 13).
        /// </summary>
        public async Task PersonWasMachen(int id, int modus)
        {
            if (id == 0)
                return;

            if (id == SW.Dynamisch.GetAktiverSpieler())
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr könnt diese Aktion nicht auf Euch selbst anwenden");
                return;
            }

            switch (modus)
            {
                case 0: // Beziehungen pflegen (Hinterzimmer)
                    SW.UI.BeziehungPflegen.ShowDialog(id);
                    break;
                case 2: // Anschwärzen (Hinterzimmer, zweistufig)
                    SW.Dynamisch.Anschwaerzen(id);
                    break;
                case 1: // Sabotage (Hinterzimmer)
                    await SW.Dynamisch.Sabotage(id);
                    break;
                case 3: // Spionage (Hinterzimmer)
                    await SW.Dynamisch.Spionage(id);
                    break;
                case 4: // Ermordung (Hinterzimmer)
                    await SW.Dynamisch.Ermordung(id);
                    break;
                case 8: // Prozess initiieren
                    await SW.Dynamisch.ProzessInitiieren(id);
                    break;
                case 13: // Hand des Henkers
                    SW.Dynamisch.HenkersHand(id);
                    break;
                case 12: // Vergifteter Wein (Privileg des Kellermeisters)
                    await SW.Dynamisch.WeinVergiften(id);
                    break;
                case 14: // Beleidigung → Satisfaktion/Duell (Issue #17)
                {
                    var fechtDuell = new FechtDuellManager();

                    if (!fechtDuell.KannBeleidigen(id, out string grundDuell))
                    {
                        SW.Dynamisch.BelTextAnzeigen(grundDuell);
                        break;
                    }

                    if (await SW.UI.YesNoQuestion.ShowDialogText(fechtDuell.GetBeleidigungsFrage(id)) != DialogResultGame.Yes)
                        break;

                    var reaktion = fechtDuell.Beleidige(id);

                    // KI entscheidet aus ihrer Bosheit, ein menschliches Ziel per Dialog.
                    bool satisfaktion = reaktion.ZielIstMensch
                        ? await SW.UI.YesNoQuestion.ShowDialogText(
                              fechtDuell.GetSatisfaktionsFrage(SW.Dynamisch.GetAktiverSpieler()),
                              "Duell im Morgengrauen", "Verzichten") == DialogResultGame.Yes
                        : reaktion.KiVerlangtSatisfaktion;

                    if (satisfaktion)
                    {
                        // Bietet der Client die Vollbild-Inszenierung an, wird das Duell dort ausgetragen
                        // (interaktives Wortgefecht) und erst danach ausgewertet; sonst bleibt es beim
                        // gewürfelten Duell mit reiner Textmeldung.
                        if (SW.UI.DuellDialog != null)
                        {
                            string gegnerName = SW.Dynamisch.GetSpWithID(id).GetKompletterName();
                            var gefecht = new WortgefechtManager(id);

                            bool gewonnen = await SW.UI.DuellDialog.SpieleWortgefecht(gefecht, gegnerName);
                            var ergebnis = fechtDuell.WendeDuellAusgangAn(id, gewonnen);

                            await SW.UI.DuellDialog.ZeigeAusgang(ergebnis.SpielerHatGewonnen, ergebnis.GegnerName,
                                                                 ergebnis.AmtVerloren, ergebnis.AmtName);
                        }
                        else
                        {
                            SW.Dynamisch.BelTextAnzeigen(fechtDuell.FuehreDuellDurch(id).Meldung);
                        }
                    }
                    else
                    {
                        SW.Dynamisch.BelTextAnzeigen(fechtDuell.VerweigereSatisfaktion(id));
                    }

                    break;
                }
                case 5: // Erpressung (Issue #13)
                {
                    var erpressung = new ErpressungManager();

                    if (!erpressung.KannErpressen(id, out string grundErpressung))
                    {
                        SW.Dynamisch.BelTextAnzeigen(grundErpressung);
                        break;
                    }

                    if (await SW.UI.YesNoQuestion.ShowDialogText(erpressung.GetErpressungsFrage(id),
                            "Erpressen", "Davon absehen") != DialogResultGame.Yes)
                        break;

                    // Ein menschliches Opfer entscheidet selbst, ob es sich beugt; bei der KI würfelt
                    // die Erfolgschance. Ohne Dialog (Client ohne Unterstützung) beugt es sich nicht.
                    bool? opferBeugtSich = null;

                    if (id < SW.Statisch.GetMinKIID())
                    {
                        opferBeugtSich = SW.UI.ErpressungDialog != null &&
                                         await SW.UI.ErpressungDialog.FrageOpfer(
                                             erpressung.GetOpferFrage(SW.Dynamisch.GetAktiverSpieler(), id));
                    }

                    SW.Dynamisch.BelTextAnzeigen(erpressung.FuehreErpressungDurch(id, opferBeugtSich).Meldung);
                    break;
                }
                default:
                    SW.Dynamisch.BelTextAnzeigen("Diese Aktion ist noch nicht verfügbar.");
                    break;
            }
        }
    }
}
