using Conspiratio.Lib.Allgemein;

namespace Conspiratio.Lib.Gameplay.Spielwelt
{
    /// <summary>
    /// Spielwelt, die alle untergeordneten Objekte von statischen und dynamischen Daten enthält.
    /// </summary>
    public static class SW
    {
        private static StatischeSpieldaten _statisch = null;
        private static DynamischeSpieldaten _dynamisch = null;
        private static UIHelper _uiHelper = null;

        public static StatischeSpieldaten Statisch
        {
            get
            {
                if (_statisch == null)
                    _statisch = new StatischeSpieldaten();

                return _statisch;
            }
        }

        public static DynamischeSpieldaten Dynamisch
        {
            get
            {
                if (_dynamisch == null)
                    _dynamisch = new DynamischeSpieldaten();

                return _dynamisch;
            }
        }

        public static UIHelper UI
        {
            get
            {
                if (_uiHelper == null)
                    _uiHelper = new UIHelper();

                return _uiHelper;
            }
        }
    }
}
