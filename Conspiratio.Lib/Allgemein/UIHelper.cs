using Conspiratio.Lib.Gameplay.Privilegien;

namespace Conspiratio.Lib.Allgemein
{
    public class UIHelper
    {
        public IYesNoQuestion YesNoQuestion { get; private set; }

        public IShowText ShowText { get; private set; }

        public IBeziehungPflegen BeziehungPflegen { get; private set; }

        public IBauwerkStiftenDialog BauwerkStiftenDialog { get; private set; }

        public IFestGebenDialog FestGebenDialog { get; private set; }

        public IPolitischeWeltkarteDialog PolitischeWeltkarteDialog { get; private set; }

        public ITestamentAnzeigenDialog TestamentAnzeigenDialog { get; private set; }

        public IProzentwertFestlegenDialog ProzentwertFestlegenDialog { get; private set; }

        public IUntergebeneDialog UntergebeneDialog { get; private set; }

        /// <summary>Vollbild-Inszenierung eines Duells; bleibt null, wenn der Client sie nicht anbietet.</summary>
        public IDuellDialog DuellDialog { get; private set; }

        /// <summary>Entscheidung eines menschlichen Erpressungsopfers; null, wenn der Client sie nicht anbietet.</summary>
        public IErpressungDialog ErpressungDialog { get; private set; }

        public void Initialisieren(IYesNoQuestion yesNoQuestion, IShowText showText, IBeziehungPflegen beziehungPflegen, IBauwerkStiftenDialog bauwerkStiftenDialog,
                                   IFestGebenDialog festGebenDialog, IPolitischeWeltkarteDialog politischeWeltkarteDialog, ITestamentAnzeigenDialog testamentAnzeigenDialog,
                                   IProzentwertFestlegenDialog prozentwertFestlegenDialog, IUntergebeneDialog untergebeneDialog, IDuellDialog duellDialog = null,
                                   IErpressungDialog erpressungDialog = null)
        {
            YesNoQuestion = yesNoQuestion;
            ShowText = showText;
            BeziehungPflegen = beziehungPflegen;
            BauwerkStiftenDialog = bauwerkStiftenDialog;
            FestGebenDialog = festGebenDialog;
            PolitischeWeltkarteDialog = politischeWeltkarteDialog;
            TestamentAnzeigenDialog = testamentAnzeigenDialog;
            ProzentwertFestlegenDialog = prozentwertFestlegenDialog;
            UntergebeneDialog = untergebeneDialog;
            DuellDialog = duellDialog;
            ErpressungDialog = erpressungDialog;
        }
    }
}
