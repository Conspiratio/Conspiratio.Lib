using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivGuenstigeKredite : Privileg
    {

        public PrivGuenstigeKredite() : base("Günstige Kredite", 30)
        {
        }

        public override void PrivExecute()
        {
            SW.Dynamisch.BelTextAnzeigen("Eure hervorragenden Beziehungen erlauben es Euch, Kredite mit niedrigerem Zinssatz aufzunehmen");
        }
    }
}
