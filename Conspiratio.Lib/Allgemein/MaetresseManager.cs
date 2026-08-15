using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Das Privileg „Mätresse nehmen" (Issue #8, Vorbild „Die Fugger 2"): ein verheirateter Spieler kann
    /// sich gegen einmalige Kosten eine Mätresse nehmen. Sie mehrt sein Ansehen und seine Lebensfreude
    /// (mehr verbleibende Jahre), senkt aber die Wahrscheinlichkeit auf ehelichen Nachwuchs und kostet
    /// jährlich Unterhalt – dazu droht Jahr für Jahr ein Skandal, der das Ansehen beschädigt.
    /// </summary>
    public class MaetresseManager
    {
        /// <summary>Einmalige Kosten, um sich eine Mätresse zu nehmen.</summary>
        [PublicAPI] public const int Kosten = 5000;

        /// <summary>Jährlicher Unterhalt für die Mätresse.</summary>
        [PublicAPI] public const int JaehrlicherUnterhalt = 1000;

        /// <summary>Ansehensgewinn beim Nehmen der Mätresse (dauerhaft).</summary>
        [PublicAPI] public const int AnsehenBonus = 8;

        /// <summary>Zusätzliche verbleibende Lebensjahre durch die neue Lebensfreude.</summary>
        [PublicAPI] public const int LebenszeitBonus = 3;

        /// <summary>Skandal-Wahrscheinlichkeit pro Jahr: 1 zu diesem Wert.</summary>
        [PublicAPI] public const int SkandalChance = 5;

        /// <summary>Ansehensverlust bei einem Skandal.</summary>
        [PublicAPI] public const int SkandalAnsehenVerlust = 6;

        /// <summary>Faktor, um den sich die Kinder-Wahrscheinlichkeit mit Mätresse verschlechtert (siehe FamilieManager).</summary>
        [PublicAPI] public const int KinderChanceFaktor = 3;

        /// <summary>Ob der aktive Spieler bereits eine Mätresse unterhält.</summary>
        [PublicAPI]
        public bool HatMaetresse()
        {
            return SW.Dynamisch.GetAktHum().HatMaetresse();
        }

        /// <summary>
        /// Prüft, ob sich der aktive Spieler eine Mätresse nehmen darf (verheiratet, noch keine Mätresse,
        /// genügend Taler). Liefert andernfalls in <paramref name="grund"/> den Ablehnungsgrund.
        /// </summary>
        [PublicAPI]
        public bool KannMaetresseNehmen(out string grund)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.HatMaetresse())
            {
                grund = "Ihr unterhaltet bereits eine Mätresse.";
                return false;
            }

            if (spieler.GetVerheiratet() == 0)
            {
                grund = "Nur ein verheirateter Mann von Stand nimmt sich eine Mätresse.";
                return false;
            }

            if (spieler.GetTaler() < Kosten)
            {
                grund = "Eine Mätresse standesgemäß auszuhalten kostet " + Kosten + " Taler – so viel besitzt Ihr nicht.";
                return false;
            }

            grund = null;
            return true;
        }

        /// <summary>Der Angebotstext mit Kosten und Wirkung (für die Rückfrage im Client).</summary>
        [PublicAPI]
        public string GetAngebotstext()
        {
            return "Wollt Ihr Euch eine Mätresse nehmen?\n\n" +
                   "Das kostet einmalig " + Kosten + " Taler und jährlich " + JaehrlicherUnterhalt + " Taler Unterhalt. " +
                   "Sie mehrt Euer Ansehen und Eure Lebensfreude – doch hütet Euch vor dem Skandal, und ehelicher Nachwuchs wird seltener.";
        }

        /// <summary>
        /// Nimmt dem aktiven Spieler die Mätresse: zieht die einmaligen Kosten ab, mehrt Ansehen und
        /// verbleibende Lebensjahre und merkt sich den Zustand. Vorher mit <see cref="KannMaetresseNehmen"/> prüfen.
        /// </summary>
        [PublicAPI]
        public void NimmMaetresse()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.ErhoeheTaler(-Kosten);
            spieler.SetHatMaetresse(true);
            spieler.ErhoehePermaAnsehen(AnsehenBonus);
            spieler.SetVerbleibendeJahre(spieler.GetVerbleibendeJahre() + LebenszeitBonus);
        }

        /// <summary>
        /// Wickelt zu Jahresbeginn den jährlichen Unterhalt ab und würfelt das Skandal-Risiko. Zieht den
        /// Unterhalt still ab; bei einem Skandal sinkt das Ansehen und es wird eine Meldung geliefert
        /// (sonst null). Hat der Spieler keine Mätresse, passiert nichts.
        /// </summary>
        [PublicAPI]
        public string VerarbeiteJahr()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (!spieler.HatMaetresse())
                return null;

            spieler.ErhoeheTaler(-JaehrlicherUnterhalt);

            if (SW.Statisch.Rnd.Next(0, SkandalChance) == 0)
            {
                spieler.ErhoehePermaAnsehen(-SkandalAnsehenVerlust);
                return "Ein Skandal! Euer Verhältnis zur Mätresse ist zum Stadtgespräch geworden – Euer Ansehen leidet.";
            }

            return null;
        }
    }
}
