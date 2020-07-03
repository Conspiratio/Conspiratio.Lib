using System;
using System.Collections.Generic;
using System.Linq;

using Conspiratio.Kampf;
using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Klasse zur Berechnung von Kämpfen zwischen Söldnern und Räubern
    /// </summary>
    public class Kampfberechnung
    {
        #region BerechneKampfErgebnis
        /// <summary>
        /// Berechnet das Ergebnis eines Kampfes zwischen zweit Truppen zweier Spieler (menschlich oder auch KI).
        /// Das Ergebnis wird nur berechnet und zurückgegeben, es wird keine Aktualisierung der Moral oder der Truppen in den jeweiligen Stützpunkten vorgenommen!
        /// Dokumentation des Kampfablaufes: https://github.com/DerEinzehnte/Conspiratio-Programm/wiki/S%C3%B6ldner-&-R%C3%A4uber-System#die-kampfberechnung
        /// </summary>
        /// <param name="kampf">Stellt den Kampf dar, für den das Ergebnis berechnet werden soll</param>
        /// <returns>Ergebnis des Kampfes</returns>
        public KampfErgebnis BerechneKampfErgebnis(Kampf kampf)
        {
            List<Einheit> truppenSpielerBeginnt;
            List<Einheit> truppenSpielerZweiter;
            List<Einheit> verlusteSpielerBeginnt = new List<Einheit>();
            List<Einheit> verlusteSpielerZweiter = new List<Einheit>();

            string Zusammenfassung = "";
            string BezeichnungTruppenAngreiferDativ = "";  // Dativ: dem Söldner, den Söldnern (Wem?) Siehe: http://wortwuchs.net/grammatik/kasus/
            string BezeichnungTruppenVerteidigerDativ = "";
            string BezeichnungTruppenAngreiferNominativ = "";  // Nominativ: der Söldner, die Söldner (Wer?) Siehe: http://wortwuchs.net/grammatik/kasus/
            string BezeichnungTruppenVerteidigerNominativ = "";
            string MoralAngreiferAdjektiv = "";
            string MoralVerteidigerAdjektiv = "";

            int StuetzpunktIndexAngreifer = kampf.StuetzpunktIDAngreifer - 1;
            int StuetzpunktIndexVerteidiger = kampf.StuetzpunktIDVerteidiger - 1;
            int Wuerfel;
            int MoralSpielerBeginnt;
            int MoralSpielerZweiter;
            int StuetzpunktIndexSpielerBeginnt;
            int StuetzpunktIndexSpielerZweiter;
            int SpielerIDBeginnt;
            int SpielerIDZweiter;
            int SpielerIDGewinner = 0;
            int SpielerIDVerlierer = 0;
            int StartanzahlTruppenSpielerBeginnt;
            int StartanzahlTruppenSpielerZweiter;
            int AnzahlTruppenAngreifer = 0;
            int AnzahlTruppenVerteidiger = 0;

            double Angriffswert = 0d;
            double Verteidigungswert = 0d;
            double Schaden = 0d;

            bool AngreiferBeginnt = false;
            bool KampfBeendet = false;
            bool VerteidigerVorhanden = (kampf.StuetzpunktIDVerteidiger != 0);  // Gibt es Verteidigungstruppen (oder nur Angreifer und Karawane)?

            #region Bezeichnung der Truppen ermitteln

            if (SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexAngreifer].Art == EnumStuetzpunktArt.Raeuberlager)
            {
                if (kampf.TruppenAngreifer.Count == 1)
                {
                    BezeichnungTruppenAngreiferDativ = "Räuber";
                    BezeichnungTruppenAngreiferNominativ = "Räuber";
                }
                else
                {
                    BezeichnungTruppenAngreiferDativ = "Räubern";
                    BezeichnungTruppenAngreiferNominativ = "Räuber";
                }
            }
            else if (SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexAngreifer].Art == EnumStuetzpunktArt.Zollburg)
            {
                // Für zukünftige Angriffsaktionen von Söldnern
                if (kampf.TruppenAngreifer.Count == 1)
                {
                    BezeichnungTruppenAngreiferDativ = "Söldner";
                    BezeichnungTruppenAngreiferNominativ = "Söldner";
                }
                else
                {
                    BezeichnungTruppenAngreiferDativ = "Söldnern";
                    BezeichnungTruppenAngreiferNominativ = "Söldner";
                }
            }

            if (VerteidigerVorhanden)
            {
                if (SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexVerteidiger].Art == EnumStuetzpunktArt.Raeuberlager)
                {
                    // Für zukünftige Verteidigungsaktionen der Räuber
                    if (kampf.TruppenVerteidiger.Count == 1)
                    {
                        BezeichnungTruppenVerteidigerDativ = "Räuber";
                        BezeichnungTruppenVerteidigerNominativ = "Räuber";
                    }
                    else
                    {
                        BezeichnungTruppenVerteidigerDativ = "Räubern";
                        BezeichnungTruppenVerteidigerNominativ = "Räuber";
                    }
                }
                else if (SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexVerteidiger].Art == EnumStuetzpunktArt.Zollburg)
                {
                    if (kampf.TruppenVerteidiger.Count == 1)
                    {
                        BezeichnungTruppenVerteidigerDativ = "Söldner";
                        BezeichnungTruppenVerteidigerNominativ = "Söldner";
                    }
                    else
                    {
                        BezeichnungTruppenVerteidigerDativ = "Söldnern";
                        BezeichnungTruppenVerteidigerNominativ = "Söldner";
                    }
                }
            }
            #endregion

            // Einleitung der Zusammenfassung erstellen
            if (kampf.KampfArt == EnumKampfArt.KarawanenPluenderung)
            {
                if (kampf.Karawane == null)
                    return null;

                Wuerfel = SW.Statisch.Rnd.Next(1, 4);  // 1 - 3 würfeln

                switch (Wuerfel)
                {
                    case 1:
                        Zusammenfassung += $"Die Karren der Karawane von |{SW.Dynamisch.GetSpWithID(kampf.Karawane.SpielerID).GetKompletterName()}| rumpeln friedlich über einen Hohlweg in {SW.Dynamisch.GetLandWithID(kampf.LandID).GetGebietsName()}. ";
                        break;

                    case 2:
                        Zusammenfassung += $"Die Wagen der Karawane von |{SW.Dynamisch.GetSpWithID(kampf.Karawane.SpielerID).GetKompletterName()}| holpern gemütlich durch ein idyllisches Waldstück in {SW.Dynamisch.GetLandWithID(kampf.LandID).GetGebietsName()}. ";
                        break;

                    case 3:
                        Zusammenfassung += $"Die Fuhrwerke der Karawane von |{SW.Dynamisch.GetSpWithID(kampf.Karawane.SpielerID).GetKompletterName()}| passieren nichtsahnend eine schmale Senke in {SW.Dynamisch.GetLandWithID(kampf.LandID).GetGebietsName()}. ";
                        break;

                    default:
                        break;
                }

                MoralAngreiferAdjektiv = ErmittleMoralAdjektiv(kampf.MoralAngreifer, kampf.TruppenAngreifer.Count > 1, false);

                Zusammenfassung += $"Plötzlich knackt es im umliegenden Gebüsch und {kampf.TruppenAngreifer.Count} {MoralAngreiferAdjektiv} {BezeichnungTruppenAngreiferNominativ} " +
                                   $"aus {SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexAngreifer].Name} unter dem Befehl von |{SW.Dynamisch.GetSpWithID(kampf.SpielerIDAngreifer).GetKompletterName()}| springen hervor " +
                                   $"und versperren den Weg. Ein Überfall! ";

                Zusammenfassung += $"\nWarenwert der Karawane: {kampf.Karawane.Warenwert.ToStringGeld()}\n\n";

                if (VerteidigerVorhanden)
                {
                    MoralVerteidigerAdjektiv = ErmittleMoralAdjektiv(kampf.MoralVerteidiger, (kampf.TruppenVerteidiger.Count > 1), true);

                    Zusammenfassung += $"Doch eine in der Nähe befindliche Patrouille von |{SW.Dynamisch.GetSpWithID(kampf.SpielerIDVerteidiger).GetKompletterName()}| mit {kampf.TruppenVerteidiger.Count} " +
                                       $"{MoralVerteidigerAdjektiv} {BezeichnungTruppenVerteidigerDativ} eilt sofort zur Hilfe. Ein erbitterter Kampf entbrennt.\n\n";
                }
                else
                {
                    Zusammenfassung += "Die Karawane ergibt sich, da in unmittelbarer Umgebung keine Beschützer in Sicht sind.\n";
                }
            }
            else
            {
                // Zukünftig: Normales Aufeinandertreffen in einem Land (kein Überfall), Angriff auf einen Stützpunkt oder Belagerung einer Stadt usw.
                MoralAngreiferAdjektiv = ErmittleMoralAdjektiv(kampf.MoralAngreifer, kampf.TruppenAngreifer.Count > 1, true);
                MoralVerteidigerAdjektiv = ErmittleMoralAdjektiv(kampf.MoralVerteidiger, kampf.TruppenVerteidiger.Count > 1, true);

                Zusammenfassung += $"In {SW.Dynamisch.GetLandWithID(kampf.LandID).GetGebietsName()} kommt zu einem Zusammentreffen von {kampf.TruppenAngreifer.Count} {MoralAngreiferAdjektiv} " +
                                   $"{BezeichnungTruppenAngreiferDativ} aus {SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexAngreifer].Name} unter dem Befehl von |{SW.Dynamisch.GetSpWithID(kampf.SpielerIDAngreifer).GetKompletterName()}| " +
                                   $"und {kampf.TruppenVerteidiger.Count} {MoralVerteidigerAdjektiv} {BezeichnungTruppenVerteidigerDativ} aus {SW.Dynamisch.GetStuetzpunkte()[StuetzpunktIndexVerteidiger].Name}, " +
                                   $"angeführt von |{SW.Dynamisch.GetSpWithID(kampf.SpielerIDVerteidiger).GetKompletterName()}|. Ein erbitterter Kampf entbrennt.\n\n";
            }

            if (!VerteidigerVorhanden)  // Wenn es keinen Verteidiger gibt, muss keine Kampfberechnung erfolgen
            {
                Zusammenfassung += $"Die {BezeichnungTruppenAngreiferNominativ} von |{SW.Dynamisch.GetSpWithID(kampf.SpielerIDAngreifer).GetKompletterName()}| genießen als Sieger ihren kampflosen Triumph und " +
                                   $"machen sich mit der gesamten Beute davon.\nKeine Truppenverluste auf beiden Seiten.";

                MoralSpielerBeginnt = kampf.MoralAngreifer;

                if (kampf.MoralAngreifer < 98)
                    MoralSpielerBeginnt = kampf.MoralAngreifer + 3;
                else if (kampf.MoralAngreifer < 100)
                    MoralSpielerBeginnt = 100;

                return new KampfErgebnis(kampf.SpielerIDAngreifer, 0, kampf.SpielerIDAngreifer, MoralSpielerBeginnt, 0, kampf.StuetzpunktIDAngreifer, 0, kampf.AktionIndexAngreifer,
                                         0, new List<Einheit>(), null, Zusammenfassung, kampf.KampfArt, kampf.Karawane);
            }

            // Entscheidung, welche Seite beginnen darf
            if (kampf.SpielerIDAngreifer == kampf.SpielerIDVerteidiger)
            {
                Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                if (Wuerfel == 1)
                    AngreiferBeginnt = true;
            }
            else if (kampf.SpielerIDAngreifer > kampf.SpielerIDVerteidiger)
                AngreiferBeginnt = true;

            if (AngreiferBeginnt)
            {
                truppenSpielerBeginnt = kampf.TruppenAngreifer;
                truppenSpielerZweiter = kampf.TruppenVerteidiger;
                StartanzahlTruppenSpielerBeginnt = kampf.TruppenAngreifer.Count;
                StartanzahlTruppenSpielerZweiter = kampf.TruppenVerteidiger.Count;
                MoralSpielerBeginnt = kampf.MoralAngreifer;
                MoralSpielerZweiter = kampf.MoralVerteidiger;
                StuetzpunktIndexSpielerBeginnt = StuetzpunktIndexAngreifer;
                StuetzpunktIndexSpielerZweiter = StuetzpunktIndexVerteidiger;
                SpielerIDBeginnt = kampf.SpielerIDAngreifer;
                SpielerIDZweiter = kampf.SpielerIDVerteidiger;
            }
            else
            {
                truppenSpielerBeginnt = kampf.TruppenVerteidiger;
                truppenSpielerZweiter = kampf.TruppenAngreifer;
                StartanzahlTruppenSpielerBeginnt = kampf.TruppenVerteidiger.Count;
                StartanzahlTruppenSpielerZweiter = kampf.TruppenAngreifer.Count;
                MoralSpielerBeginnt = kampf.MoralVerteidiger;
                MoralSpielerZweiter = kampf.MoralAngreifer;
                StuetzpunktIndexSpielerBeginnt = StuetzpunktIndexVerteidiger;
                StuetzpunktIndexSpielerZweiter = StuetzpunktIndexAngreifer;
                SpielerIDBeginnt = kampf.SpielerIDVerteidiger;
                SpielerIDZweiter = kampf.SpielerIDAngreifer;
            }

            // Truppen zufällig mischen
            truppenSpielerBeginnt = truppenSpielerBeginnt.OrderBy(x => x.GUID).ToList();
            truppenSpielerZweiter = truppenSpielerZweiter.OrderBy(x => x.GUID).ToList();

            /* 
            Siehe auch: https://github.com/DerEinzehnte/Conspiratio-Programm/wiki/S%C3%B6ldner-&-R%C3%A4uber-System#die-kampfberechnung
            Kampfablauf:
            in Kampf besteht normalerweise aus mehreren Runden, ausser der Kampf endet schon nach der ersten Runde. 
            Pro Runde werden alle lebenen Einheiten beider Spieler einen Angriff ausführen, bis alle Einheiten einen Angriff durchgeführt haben.
            Die erste Einheit des Spielers, der beginnt, fängt an, danach abwechselnd. Die Einheiten werden gemischt, sind also nicht sortiert. 
            Ursprünglich hatte ich vor, Einheiten mit einem höheren Agilitätswert eher angreifen zu lassen als Einheiten mit einem niedrigeren Wert, 
            allerdings würde dies die Kämpfe weniger dynamisch gestalten und die einzelnen Schwächen und Stärken hätten weniger Auswirkung bzw. einseitige 
            Truppen hätten deutlich größere Vor- oder Nachteile. So fühlt es sich jedenfalls fairer und ausgeglichener an aber das ist derzeit nur mein erster Entwurf,
            der sicher noch reifen muss. Die Angriffe der jeweiligen Einheiten konzentrieren sich immer auf die nächste Einheit, bis diese tot ist.
            Stirbt eine Einheit, dann sinkt die Moral der zugehörigen Truppe zu einer Wahrscheinlichkeit von 50 % um 1 %.
            Wird eine Einheit getötet, steigt die Moral der angreifenden Truppe zu einer Wahrscheinlichkeit von 50 % um 1 %.

            Nach einer kompletten Angriffsrunde wird folgendes geprüft:
            - Ist die Moral einer Truppe 0 %? Dann flieht diese und der Kampf ist beendet und für diese Truppe verloren
            - Ist die Moral einer Truppe unter 20 %, besteht eine Chance von 50 %, dass die Truppen aufgeben und fliehen. Der Kampf ist damit verloren und beendet.
            - Hat eine Truppe mehr als die Hälfte ihrer ursprünglichen Einheiten verloren? Dann flieht diese und der Kampf ist beendet und für diese Truppe verloren
            - Wenn eine Truppe in der ersten Runde komplett ausgelöscht wurde, ist der Kampf natürlich ebenfalls vorbei

            Sind diese Prüfungen alle erfolglos, dann geht es in die nächste Kampfrunde.

            Berechnung der Angriffs- und Verteidigungsstärke

            Die Angriffsstärke einer Einheit wird wie folgt berechnet:
            Angriffsstärke = Angriffswert + Moralfaktor + StarkGegenFaktor

            Die Verteidigungsstärke einer Einheit wird wie folgt berechnet:
            Verteidigungsstärke = Verteidigungswert + Moralfaktor - SchwachGegenFaktor

            Den Lebenspunkten wird bei einem Angriff folgendes Ergebnis abgezogen:
            Abzug Lebenspunkte = Angriffswert - Verteidigungswert
            Ist dieser Wert 0 oder kleiner als 0 so wird der Einheit immer 1 abgezogen (damit Kämpfe nicht unendlich lange dauern bzw. in die Länge gezogen werden).
            */

            while (!KampfBeendet)
            {
                #region Kampfrunde für alle Einheiten ausführen

                for (int i = 0; i < truppenSpielerBeginnt.Count; i++)
                {
                    // Angriff erster Spieler
                    if (truppenSpielerZweiter.Count == 0)
                        break;  // keine Gegner mehr am Leben

                    Angriffswert = truppenSpielerBeginnt[i].AngriffsstaerkeBerechnen(MoralSpielerBeginnt, truppenSpielerZweiter[0].GetType());  // Angriffswert berechnen
                    Verteidigungswert = truppenSpielerZweiter[0].VerteidigungsstaerkeBerechnen(MoralSpielerZweiter, truppenSpielerBeginnt[i].GetType());  // Verteidungsstärke des Gegners berechnen
                    Schaden = Angriffswert - Verteidigungswert;

                    if (Schaden <= 0)
                        Schaden = 1d;  // immer mind. 1 Schaden

                    if ((truppenSpielerZweiter[0].Lebenspunkte - Schaden) <= 0)   // Wird der aktuelle Gegner besiegt?
                    {
                        verlusteSpielerZweiter.Add(truppenSpielerZweiter[0]);
                        truppenSpielerZweiter.RemoveAt(0);

                        // Moral für Angreifer aktualisieren
                        Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                        if (Wuerfel == 1)
                            MoralSpielerBeginnt += 1;

                        // Moral für Verteidiger aktualisieren
                        Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                        if (Wuerfel == 1)
                            MoralSpielerZweiter -= 1;
                    }
                    else
                        truppenSpielerZweiter[0].Lebenspunkte -= Schaden;  // Schaden verursachen

                    if (truppenSpielerZweiter.Count == 0)
                        break;  // keine Gegner mehr am Leben

                    if (i < truppenSpielerZweiter.Count)
                    {
                        // Gegenangriff zweiter Spieler
                        Angriffswert = truppenSpielerZweiter[i].AngriffsstaerkeBerechnen(MoralSpielerZweiter, truppenSpielerBeginnt[0].GetType());  // Angriffswert berechnen
                        Verteidigungswert = truppenSpielerBeginnt[0].VerteidigungsstaerkeBerechnen(MoralSpielerBeginnt, truppenSpielerZweiter[i].GetType());  // Verteidungsstärke des Gegners berechnen
                        Schaden = Angriffswert - Verteidigungswert;

                        if (Schaden <= 0)
                            Schaden = 1d;  // immer mind. 1 Schaden

                        if ((truppenSpielerBeginnt[0].Lebenspunkte - Schaden) <= 0)   // Wird der aktuelle Gegner besiegt?
                        {
                            verlusteSpielerBeginnt.Add(truppenSpielerBeginnt[0]);
                            truppenSpielerBeginnt.RemoveAt(0);

                            // Moral für Angreifer aktualisieren
                            Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                            if (Wuerfel == 1)
                                MoralSpielerZweiter += 1;

                            // Moral für Verteidiger aktualisieren
                            Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                            if (Wuerfel == 1)
                                MoralSpielerBeginnt -= 1;

                            i--;  // den Zähler um 1 zurücksetzen, da ein Eintrag der zu durchlaufenden Liste entfernt wurde, es muss jede Einheit drankommen
                        }
                        else
                            truppenSpielerBeginnt[0].Lebenspunkte -= Schaden;  // Schaden verursachen

                        if (truppenSpielerBeginnt.Count == 0)
                            break;  // keine Gegner mehr am Leben
                    }
                }
                #endregion

                #region Kampfrunde auswerten und Prüfung auf Kampfende durchführen

                if ((truppenSpielerBeginnt.Count == 0) || (truppenSpielerZweiter.Count == 0))
                {
                    if (truppenSpielerBeginnt.Count == 0)
                    {
                        // Spieler Beginnt verliert
                        SpielerIDGewinner = SpielerIDZweiter;
                        SpielerIDVerlierer = SpielerIDBeginnt;
                    }
                    else
                    {
                        // Spieler Zweiter verliert
                        SpielerIDGewinner = SpielerIDBeginnt;
                        SpielerIDVerlierer = SpielerIDZweiter;
                    }

                    Zusammenfassung += $"Mit einer verheerenden Angriffswelle werden die Truppen von |{SW.Dynamisch.GetSpWithID(SpielerIDVerlierer).GetKompletterName()}| vernichtend geschlagen und komplett ausgelöscht. ";
                    KampfBeendet = true;
                    break;
                }

                if ((truppenSpielerBeginnt.Count < (StartanzahlTruppenSpielerBeginnt / 2)) || (truppenSpielerZweiter.Count < (StartanzahlTruppenSpielerZweiter / 2)))
                {
                    if ((truppenSpielerBeginnt.Count < (StartanzahlTruppenSpielerBeginnt / 2)) && (truppenSpielerZweiter.Count < (StartanzahlTruppenSpielerZweiter / 2)))
                    {
                        // Haben beide Spieler nur noch weniger als die Hälfte der Truppen?
                        if (truppenSpielerBeginnt.Count > truppenSpielerZweiter.Count)
                        {
                            // Spieler Zweiter flieht
                            SpielerIDGewinner = SpielerIDBeginnt;
                            SpielerIDVerlierer = SpielerIDZweiter;
                        }
                        else if (truppenSpielerBeginnt.Count < truppenSpielerZweiter.Count)
                        {
                            // Spieler Beginnt flieht
                            SpielerIDGewinner = SpielerIDZweiter;
                            SpielerIDVerlierer = SpielerIDBeginnt;
                        }
                        else
                        {
                            if (MoralSpielerBeginnt > MoralSpielerZweiter)
                            {
                                // Spieler Zweiter flieht
                                SpielerIDGewinner = SpielerIDBeginnt;
                                SpielerIDVerlierer = SpielerIDZweiter;
                            }
                            else if (MoralSpielerBeginnt < MoralSpielerZweiter)
                            {
                                // Spieler Beginnt flieht
                                SpielerIDGewinner = SpielerIDZweiter;
                                SpielerIDVerlierer = SpielerIDBeginnt;
                            }
                            else
                            {
                                // Anzahl Truppen und Moral ist bei beiden gleich, Ausgang des Kampfes auswürfeln
                                Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                                if (Wuerfel == 1)
                                {
                                    // Spieler Beginnt flieht
                                    SpielerIDGewinner = SpielerIDZweiter;
                                    SpielerIDVerlierer = SpielerIDBeginnt;
                                }
                            }
                        }
                    }
                    else if (truppenSpielerBeginnt.Count < (StartanzahlTruppenSpielerBeginnt / 2))
                    {
                        // Spieler Beginnt flieht
                        SpielerIDGewinner = SpielerIDZweiter;
                        SpielerIDVerlierer = SpielerIDBeginnt;
                    }
                    else
                    {
                        // Spieler Zweiter flieht
                        SpielerIDGewinner = SpielerIDBeginnt;
                        SpielerIDVerlierer = SpielerIDZweiter;
                    }

                    Zusammenfassung += $"Nach einem sehr verlustreichen Kampf zerstreuen sich die Truppen von |{SW.Dynamisch.GetSpWithID(SpielerIDVerlierer).GetKompletterName()}| und geben ihre Stellung auf. ";
                    KampfBeendet = true;
                    break;
                }

                if ((MoralSpielerBeginnt == 0) || (MoralSpielerZweiter == 0))
                {
                    if (MoralSpielerBeginnt == 0)
                    {
                        SpielerIDGewinner = SpielerIDZweiter;
                        SpielerIDVerlierer = SpielerIDBeginnt;
                    }
                    else if (MoralSpielerZweiter == 0)
                    {
                        SpielerIDGewinner = SpielerIDBeginnt;
                        SpielerIDVerlierer = SpielerIDZweiter;
                    }

                    Zusammenfassung += $"Nach einiger Zeit fehlt den Truppen von |{SW.Dynamisch.GetSpWithID(SpielerIDVerlierer).GetKompletterName()}| schließlich jegliche Moral und sie flüchten kopflos in die umliegenden Wälder. ";
                    KampfBeendet = true;
                    break;
                }

                if ((MoralSpielerBeginnt < 20) || (MoralSpielerZweiter < 20))
                {
                    Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                    if (Wuerfel == 1)
                    {
                        if (MoralSpielerBeginnt > MoralSpielerZweiter)
                        {
                            // Spieler Zweiter flieht
                            SpielerIDGewinner = SpielerIDBeginnt;
                            SpielerIDVerlierer = SpielerIDZweiter;
                        }
                        else if (MoralSpielerBeginnt < MoralSpielerZweiter)
                        {
                            // Spieler Beginnt flieht
                            SpielerIDGewinner = SpielerIDZweiter;
                            SpielerIDVerlierer = SpielerIDBeginnt;
                        }
                        else
                        {
                            // Moral ist bei beiden gleich, Ausgang des Kampfes auswürfeln
                            Wuerfel = SW.Statisch.Rnd.Next(1, 3);  // 1 - 2 würfeln

                            if (Wuerfel == 1)
                            {
                                // Spieler Beginnt flieht
                                SpielerIDGewinner = SpielerIDZweiter;
                                SpielerIDVerlierer = SpielerIDBeginnt;
                            }
                        }

                        Zusammenfassung += $"Nach einem langen Kampf ist die Moral der Truppen von |{SW.Dynamisch.GetSpWithID(SpielerIDVerlierer).GetKompletterName()}| schließlich so niedrig, dass sie den Rückzug antreten. ";
                        KampfBeendet = true;
                        break;
                    }
                }
                #endregion
            }

            if (AngreiferBeginnt)
            {
                AnzahlTruppenAngreifer = kampf.TruppenAngreifer.Count - verlusteSpielerBeginnt.Count;
                AnzahlTruppenVerteidiger = kampf.TruppenVerteidiger.Count - verlusteSpielerZweiter.Count;
            }
            else
            {
                AnzahlTruppenAngreifer = kampf.TruppenAngreifer.Count - verlusteSpielerZweiter.Count;
                AnzahlTruppenVerteidiger = kampf.TruppenVerteidiger.Count - verlusteSpielerBeginnt.Count;
            }

            if (SpielerIDGewinner == kampf.SpielerIDAngreifer)
            {
                if (AnzahlTruppenAngreifer == 1)
                    Zusammenfassung += $"Der {BezeichnungTruppenAngreiferNominativ} von |{SW.Dynamisch.GetSpWithID(SpielerIDGewinner).GetKompletterName()}| genießt als Sieger seinen Triumph.\n";
                else
                    Zusammenfassung += $"Die {BezeichnungTruppenAngreiferNominativ} von |{SW.Dynamisch.GetSpWithID(SpielerIDGewinner).GetKompletterName()}| genießen als Sieger ihren Triumph.\n";
            }
            else
            {
                if (AnzahlTruppenVerteidiger == 1)
                    Zusammenfassung += $"Der {BezeichnungTruppenVerteidigerNominativ} von |{SW.Dynamisch.GetSpWithID(SpielerIDGewinner).GetKompletterName()}| genießt als Sieger seinen Triumph.\n";
                else
                    Zusammenfassung += $"Die {BezeichnungTruppenVerteidigerNominativ} von |{SW.Dynamisch.GetSpWithID(SpielerIDGewinner).GetKompletterName()}| genießen als Sieger ihren Triumph.\n";
            }

            if (AngreiferBeginnt)
            {
                Zusammenfassung += $"Verluste der Angreifer: {verlusteSpielerBeginnt.Count}\n";
                Zusammenfassung += $"Verluste der Verteidiger: {verlusteSpielerZweiter.Count}";
                return new KampfErgebnis(kampf.SpielerIDAngreifer, kampf.SpielerIDVerteidiger, SpielerIDGewinner, MoralSpielerBeginnt, MoralSpielerZweiter, kampf.StuetzpunktIDAngreifer,
                                         kampf.StuetzpunktIDVerteidiger, kampf.AktionIndexAngreifer, kampf.AktionIndexVerteidiger, verlusteSpielerBeginnt, verlusteSpielerZweiter, 
                                         Zusammenfassung, kampf.KampfArt, kampf.Karawane);
            }
            else
            {
                Zusammenfassung += $"Verluste der Angreifer: {verlusteSpielerZweiter.Count}\n";
                Zusammenfassung += $"Verluste der Verteidiger: {verlusteSpielerBeginnt.Count}";
                return new KampfErgebnis(kampf.SpielerIDAngreifer, kampf.SpielerIDVerteidiger, SpielerIDGewinner, MoralSpielerZweiter, MoralSpielerBeginnt, kampf.StuetzpunktIDAngreifer,
                                         kampf.StuetzpunktIDVerteidiger, kampf.AktionIndexAngreifer, kampf.AktionIndexVerteidiger, verlusteSpielerZweiter, verlusteSpielerBeginnt,
                                         Zusammenfassung, kampf.KampfArt, kampf.Karawane);
            }
        }
        #endregion

        #region KampfErgebnisAnwenden
        /// <summary>
        /// Aktualisiert die Moral und die Truppenverluste in den Stützpunkten anhand eines Ergebnisses.
        /// </summary>
        /// <param name="ergebnis">Das Ergebnis, welches angewendet werden soll</param>
        public void KampfErgebnisAnwenden(KampfErgebnis ergebnis)
        {
            // Angreifer aktualisieren
            SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].MoralTruppeInProzent = ergebnis.MoralAngreifer;

            if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Art == EnumStuetzpunktArt.Zollburg)
            {
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollSoeldner)), typeof(ZollSoeldner), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollMusketier)), typeof(ZollMusketier), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollKanonier)), typeof(ZollKanonier), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollOffizier)), typeof(ZollOffizier), false);

                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollSoeldner)), typeof(ZollSoeldner));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollMusketier)), typeof(ZollMusketier));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollKanonier)), typeof(ZollKanonier));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(ZollOffizier)), typeof(ZollOffizier));
            }
            else
            {
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubRaeuber)), typeof(RaubRaeuber), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubBombenleger)), typeof(RaubBombenleger), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubKanonier)), typeof(RaubKanonier), false);
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubSchuetze)), typeof(RaubSchuetze), false);

                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubRaeuber)), typeof(RaubRaeuber));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubBombenleger)), typeof(RaubBombenleger));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubKanonier)), typeof(RaubKanonier));
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteAngreifer, typeof(RaubSchuetze)), typeof(RaubSchuetze));
            }

            // Prüfen, ob Angriffsaktion mangels Truppen gelöscht werden muss
            if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer].Einheiten.Count == 0)
            {
                if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Art == EnumStuetzpunktArt.Raeuberlager)
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer] = new RaeuberlagerAktion(EnumAktionsartRaeuberlager.Kein_Auftrag, 0, 0, ergebnis.StuetzpunktIDAngreifer, ergebnis.AktionIndexAngreifer, new List<Einheit>());
                else if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Art == EnumStuetzpunktArt.Zollburg)
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDAngreifer - 1].Aktionen[ergebnis.AktionIndexAngreifer] = new ZollburgAktion(EnumAktionsartZollburg.Kein_Auftrag, 0, 0, ergebnis.StuetzpunktIDAngreifer, ergebnis.AktionIndexAngreifer, new List<Einheit>());
            }

            if (ergebnis.SpielerIDVerteidiger > 0)  // Gibt es einen Verteidiger?
            {
                // Verteidiger aktualisieren
                SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].MoralTruppeInProzent = ergebnis.MoralVerteidiger;

                if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Art == EnumStuetzpunktArt.Zollburg)
                {
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollSoeldner)), typeof(ZollSoeldner), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollMusketier)), typeof(ZollMusketier), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollKanonier)), typeof(ZollKanonier), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollOffizier)), typeof(ZollOffizier), false);

                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollSoeldner)), typeof(ZollSoeldner));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollMusketier)), typeof(ZollMusketier));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollKanonier)), typeof(ZollKanonier));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(ZollOffizier)), typeof(ZollOffizier));
                }
                else
                {
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubRaeuber)), typeof(RaubRaeuber), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubBombenleger)), typeof(RaubBombenleger), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubKanonier)), typeof(RaubKanonier), false);
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubSchuetze)), typeof(RaubSchuetze), false);

                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubRaeuber)), typeof(RaubRaeuber));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubBombenleger)), typeof(RaubBombenleger));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubKanonier)), typeof(RaubKanonier));
                    SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].VerringereTruppen(GetAnzahlEinheit(ergebnis.VerlusteVerteidiger, typeof(RaubSchuetze)), typeof(RaubSchuetze));
                }

                // Prüfen, ob Verteidigungsaktion magels Truppen gelöscht werden muss
                if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger].Einheiten.Count == 0)
                {
                    if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Art == EnumStuetzpunktArt.Raeuberlager)
                        SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger] = new RaeuberlagerAktion(EnumAktionsartRaeuberlager.Kein_Auftrag, 0, 0, ergebnis.StuetzpunktIDVerteidiger, ergebnis.AktionIndexVerteidiger, new List<Einheit>());
                    else if (SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Art == EnumStuetzpunktArt.Zollburg)
                        SW.Dynamisch.GetStuetzpunkte()[ergebnis.StuetzpunktIDVerteidiger - 1].Aktionen[ergebnis.AktionIndexVerteidiger] = new ZollburgAktion(EnumAktionsartZollburg.Kein_Auftrag, 0, 0, ergebnis.StuetzpunktIDVerteidiger, ergebnis.AktionIndexVerteidiger, new List<Einheit>());
                }
            }

            if ((ergebnis.KampfArt == EnumKampfArt.KarawanenPluenderung) && (ergebnis.SpielerIDAngreifer == ergebnis.SpielerIDGewinner))
            {
                SW.Dynamisch.GetSpWithID(ergebnis.SpielerIDGewinner).ErhoeheTaler(ergebnis.Karawane.Warenwert);  // Sieger erhält den Wert der Ware, die Ware verschwindet aus dem System

                if (ergebnis.Karawane.Menge > 0)   // Handelt es sich beim Opfer um einen menschlichen Spieler mit einer "echten" Warenladung? (bei einem KI-Spieler wird nur ein prozentaler Warenwert berechnet und die Menge ist immer 0)
                {
                    int VerkAnzahl = SW.Dynamisch.GetHumWithID(ergebnis.Karawane.SpielerID).GetProduktionsslot(ergebnis.Karawane.StadtID, ergebnis.Karawane.ProduktionsslotNr).GetVerkaufAnzahl() - ergebnis.Karawane.Menge;  // Gestohlene Menge von geplanter Verkaufsmenge abziehen
                    if (VerkAnzahl < 0)
                        VerkAnzahl = 0;

                    SW.Dynamisch.GetHumWithID(ergebnis.Karawane.SpielerID).GetProduktionsslot(ergebnis.Karawane.StadtID, ergebnis.Karawane.ProduktionsslotNr).SetVerkaufAnzahl(VerkAnzahl);  
                    SW.Dynamisch.GetHumWithID(ergebnis.Karawane.SpielerID).GetProduktionsslot(ergebnis.Karawane.StadtID, ergebnis.Karawane.ProduktionsslotNr).SetGestohlenAnzahl(ergebnis.Karawane.Menge);
                }
            }
        }
        #endregion

        #region GetAnzahlEinheit
        /// <summary>
        /// Gibt die Anzahl der Truppen einer bestimmten Einheit der übergebenen Liste zurück.
        /// </summary>
        /// <param name="einheiten">Liste der Einheiten</param>
        /// <param name="typeEinheit">Gewünschte Einheit, deren Anzahl ermittelt werden soll.</param>
        /// <returns>Gibt gibt die Anzahl der Truppen zurück</returns>
        public int GetAnzahlEinheit(List<Einheit> einheiten, Type typeEinheit)
        {
            return einheiten.Count(x => x.GetType() == typeEinheit);
        }
        #endregion

        #region ErmittleMoralAdjektiv
        /// <summary>
        /// Gibt das passende Adjektiv zum übergebenen Moralwert (in Prozent) zurück.
        /// </summary>
        /// <param name="moral">Moralwert</param>
        /// <param name="plural">Handelt es sich um mehrere EInheiten (true) oder nur um eine (false)?</param>
        /// <param name="dativ">Beispiel: True = mutlosen, False = mutlose (Plural: True) oder mutloser (Plural: False)</param>
        /// <returns></returns>
        private string ErmittleMoralAdjektiv(int moral, bool plural, bool dativ)
        {
            string moralbezeichnung;

            if (moral <= 10)
            {
                if (dativ)
                    moralbezeichnung = "am Boden zerstörten";
                else if (plural)
                    moralbezeichnung = "am Boden zerstörte";
                else
                    moralbezeichnung = "am Boden zerstörter";
            }
            else if (moral <= 20)
            {
                if (dativ)
                    moralbezeichnung = "mutlosen";
                else if (plural)
                    moralbezeichnung = "mutlose";
                else
                    moralbezeichnung = "mutloser";
            }
            else if (moral <= 30)
            {
                if (dativ)
                    moralbezeichnung = "demotivierten";
                else if (plural)
                    moralbezeichnung = "demotivierte";
                else
                    moralbezeichnung = "demotivierter";
            }
            else if (moral <= 40)
            {
                if (dativ)
                    moralbezeichnung = "deprimierten";
                else if (plural)
                    moralbezeichnung = "deprimierte";
                else
                    moralbezeichnung = "deprimierter";
            }
            else if (moral <= 50)
            {
                if (dativ)
                    moralbezeichnung = "motivierten";
                else if (plural)
                    moralbezeichnung = "motivierte";
                else
                    moralbezeichnung = "motivierter";
            }
            else if (moral <= 60)
            {
                if (dativ)
                    moralbezeichnung = "disziplinierten";
                else if (plural)
                    moralbezeichnung = "disziplinierte";
                else
                    moralbezeichnung = "disziplinierter";
            }
            else if (moral <= 70)
            {
                if (dativ)
                    moralbezeichnung = "loyalen";
                else if (plural)
                    moralbezeichnung = "loyale";
                else
                    moralbezeichnung = "loyaler";
            }
            else if (moral <= 80)
            {
                if (dativ)
                    moralbezeichnung = "treuen";
                else if (plural)
                    moralbezeichnung = "treue";
                else
                    moralbezeichnung = "treuer";
            }
            else if (moral <= 90)
            {
                if (dativ)
                    moralbezeichnung = "heldenmutigen";
                else if (plural)
                    moralbezeichnung = "heldenmutige";
                else
                    moralbezeichnung = "heldenmutiger";
            }
            else if (moral <= 100)
            {
                if (dativ)
                    moralbezeichnung = "siegessicheren";
                else if (plural)
                    moralbezeichnung = "siegessichere";
                else
                    moralbezeichnung = "siegessicherer";
            }
            else  // Mehr als 100?
            {
                if (dativ)
                    moralbezeichnung = "angespornten";
                else if (plural)
                    moralbezeichnung = "angespornte";
                else
                    moralbezeichnung = "angespornter";
            }

            return moralbezeichnung;
        }
        #endregion

        #region ErmittleStattfindendeKaempfe
        /// <summary>
        /// Dient zur Ermittlung der stattfindenden Kämpfe bei Rundenende.
        /// </summary>
        /// <returns>Gibt eine Liste aller stattfindenden Kämpfe zurück</returns>
        public List<Kampf> ErmittleStattfindendeKaempfe()
        {
            List<Kampf> kaempfe = new List<Kampf>();
            List<StuetzpunktAktion> angriffe;
            List<StuetzpunktAktion> verteidigungen;
            int wuerfel;

            // Angriffe sind frühestens 7 Jahre nach dem Startjahr möglich
            if ((SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr) < 7)
                return kaempfe;

            foreach (Landsicherheit oSicherheit in SW.Dynamisch.Landsicherheiten)
            {
                if (oSicherheit.Aktionen == null)
                    continue;

                angriffe = new List<StuetzpunktAktion>();
                verteidigungen = new List<StuetzpunktAktion>();

                foreach (StuetzpunktAktion oAktion in oSicherheit.Aktionen)
                {
                    if (SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Art == EnumStuetzpunktArt.Zollburg)
                    {
                        // Hier zukünftig auf Angriffsaktion von Zollburg abfragen, sobald diese existiert
                        if (((ZollburgAktion)oAktion).Aktionsart == EnumAktionsartZollburg.Überwachen)
                            verteidigungen.Add(oAktion);
                    }
                    else if (SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Art == EnumStuetzpunktArt.Raeuberlager)
                    {
                        // Hier zukünftig auf Verteidigungsaktion von Räuberlager abfragen, sobald diese existiert
                        if (((RaeuberlagerAktion)oAktion).Aktionsart == EnumAktionsartRaeuberlager.Plündern)
                            angriffe.Add(oAktion);
                    }
                }

                if (angriffe.Count == 0)
                    continue;  // Keine Angriffsaktion in diesem Land vorhanden

                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 - 100 würfeln

                if (wuerfel > oSicherheit.AngriffsrisikoInProzent)
                    continue;  // Kein Angriff aufgrund der Wahrscheinlichkeit

                // Es gibt mind. einen Kampf
                Kampf oKampf = null;
                int zaehler = 0;

                int index;
                if (angriffe.Count >= verteidigungen.Count)
                {
                    foreach (StuetzpunktAktion oAktion in angriffe)
                    {
                        if (verteidigungen.Count == 0)
                        {
                            oKampf = new Kampf()
                            {
                                SpielerIDAngreifer = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Besitzer,
                                SpielerIDVerteidiger = 0,
                                MoralAngreifer = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].MoralTruppeInProzent,
                                MoralVerteidiger = 0,
                                TruppenAngreifer = oAktion.Einheiten,
                                TruppenVerteidiger = null,
                                StuetzpunktIDAngreifer = oAktion.StuetzpunktID,
                                StuetzpunktIDVerteidiger = 0,
                                AktionIndexAngreifer = oAktion.AktionIndexStuetzpunkt,
                                AktionIndexVerteidiger = 0,
                                LandID = oSicherheit.LandID,
                                KampfArt = EnumKampfArt.KarawanenPluenderung,
                                Karawane = SW.Dynamisch.GetUeberfallOpferInLand(oSicherheit.LandID, SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Besitzer)
                            };
                        }
                        else
                        {
                            if (verteidigungen.Count - 1 <= zaehler)
                                index = verteidigungen.Count - 1;
                            else
                                index = zaehler;

                            oKampf = new Kampf()
                            {
                                SpielerIDAngreifer = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Besitzer,
                                SpielerIDVerteidiger = SW.Dynamisch.GetStuetzpunkte()[verteidigungen[index].StuetzpunktID - 1].Besitzer,
                                MoralAngreifer = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].MoralTruppeInProzent,
                                MoralVerteidiger = SW.Dynamisch.GetStuetzpunkte()[verteidigungen[index].StuetzpunktID - 1].MoralTruppeInProzent,
                                TruppenAngreifer = oAktion.Einheiten,
                                TruppenVerteidiger = verteidigungen[index].Einheiten,
                                StuetzpunktIDAngreifer = oAktion.StuetzpunktID,
                                StuetzpunktIDVerteidiger = verteidigungen[index].StuetzpunktID,
                                AktionIndexAngreifer = oAktion.AktionIndexStuetzpunkt,
                                AktionIndexVerteidiger = verteidigungen[index].AktionIndexStuetzpunkt,
                                LandID = oSicherheit.LandID,
                                KampfArt = EnumKampfArt.KarawanenPluenderung,
                                Karawane = SW.Dynamisch.GetUeberfallOpferInLand(oSicherheit.LandID, SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Besitzer)
                            };
                        }

                        kaempfe.Add(oKampf);
                        zaehler++;
                    }
                }
                else  // Mehr Verteidigungen als Angriffe
                {
                    foreach (StuetzpunktAktion oAktion in verteidigungen)
                    {
                        if (angriffe.Count - 1 <= zaehler)
                            index = angriffe.Count - 1;
                        else
                            index = zaehler;

                        oKampf = new Kampf
                        {
                            SpielerIDAngreifer = SW.Dynamisch.GetStuetzpunkte()[angriffe[index].StuetzpunktID - 1].Besitzer,
                            SpielerIDVerteidiger = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].Besitzer,
                            MoralAngreifer = SW.Dynamisch.GetStuetzpunkte()[angriffe[index].StuetzpunktID - 1].MoralTruppeInProzent,
                            MoralVerteidiger = SW.Dynamisch.GetStuetzpunkte()[oAktion.StuetzpunktID - 1].MoralTruppeInProzent,
                            TruppenAngreifer = angriffe[index].Einheiten,
                            TruppenVerteidiger = oAktion.Einheiten,
                            StuetzpunktIDAngreifer = angriffe[index].StuetzpunktID,
                            StuetzpunktIDVerteidiger = oAktion.StuetzpunktID,
                            AktionIndexAngreifer = angriffe[index].AktionIndexStuetzpunkt,
                            AktionIndexVerteidiger = oAktion.AktionIndexStuetzpunkt,
                            LandID = oSicherheit.LandID,
                            KampfArt = EnumKampfArt.KarawanenPluenderung,
                            Karawane = SW.Dynamisch.GetUeberfallOpferInLand(oSicherheit.LandID, SW.Dynamisch.GetStuetzpunkte()[angriffe[index].StuetzpunktID - 1].Besitzer)
                        };

                        kaempfe.Add(oKampf);
                        zaehler++;
                    }
                }
            }

            return kaempfe;
        }
        #endregion
    }
}
