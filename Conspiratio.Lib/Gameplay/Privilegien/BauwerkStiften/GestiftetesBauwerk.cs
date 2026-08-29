using System;

namespace Conspiratio.Lib.Gameplay.Privilegien.BauwerkStiften
{
    /// <summary>
    /// Ein bereits gestiftetes Bauwerk: Jedes der vier stiftbaren Bauwerke lässt sich je Stadt nur ein
    /// einziges Mal stiften, weshalb Stadt und Bauwerk zusammen den Eintrag ausmachen.
    /// </summary>
    [Serializable]
    public class GestiftetesBauwerk
    {
        /// <summary>Stadt, in der gestiftet wurde (<c>BauwerkStiftenManager.AktiveStadtID</c>).</summary>
        public int StadtId { get; set; }

        /// <summary>0-basierter Index des Bauwerks in <c>BauwerkStiftenManager</c> (Kirche, Kerker, Feuerwehr, Hospital).</summary>
        public int BauwerkIndex { get; set; }
    }
}
