using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Stellt die Ahnentafel der Dynastie des aktiven Spielers für die Anzeige bereit: die beim jeweiligen
    /// Erbfall festgehaltenen vergangenen Generationen plus die aktuell lebende Generation.
    /// </summary>
    public class AhnentafelManager
    {
        /// <summary>
        /// Alle Generationen der Dynastie von der ältesten (zuerst) bis zur aktuell lebenden (zuletzt).
        /// Die lebende Generation wird aus dem aktuellen Oberhaupt samt Ehepartner und lebenden Kindern
        /// aufgebaut (Todesjahr 0 = lebt noch).
        /// </summary>
        [PublicAPI]
        public List<Dynastiegeneration> GetGenerationen()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var generationen = new List<Dynastiegeneration>(spieler.GetAhnentafelListe());

            int jahr = SW.Dynamisch.GetAktuellesJahr();

            var lebend = new Dynastiegeneration
            {
                Oberhaupt = new AhnPerson(spieler.GetName(), spieler.GetTitelGegendert(), jahr - spieler.GetAlter(), 0, spieler.GetMaennlich())
            };

            if (spieler.GetVerheiratet() != 0)
            {
                var partner = SW.Dynamisch.GetSpWithID(spieler.GetVerheiratet());
                lebend.Ehepartner = new AhnPerson(partner.GetName(), partner.GetTitelGegendert(), jahr - partner.GetAlter(), 0, partner.GetMaennlich());
            }

            for (int slot = SW.Statisch.GetMinKindSlotNr(); slot < SW.Statisch.GetMaxKinderAnzahl(); slot++)
            {
                var kind = spieler.GetKindX(slot);

                if (!string.IsNullOrEmpty(kind.GetKindName()))
                    lebend.Kinder.Add(new AhnPerson(kind.GetKindName(), "", kind.Geburtsjahr, 0, kind.GetMaennlich()));
            }

            generationen.Add(lebend);
            return generationen;
        }
    }
}
