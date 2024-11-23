using Conspiratio.Lib.Gameplay.Privilegien;

namespace Conspiratio.Lib.Allgemein
{
    public class UIHelper
    {
        public IYesNoQuestion YesNoQuestion { get; private set; }

        public ITextAnzeigen TextAnzeigen { get; private set; }

        public IBeziehungPflegen BeziehungPflegen { get; private set; }

        public IBauwerkStiftenDialog BauwerkStiftenDialog { get; private set; }

        public IFestGebenDialog FestGebenDialog { get; private set; }

        public IPolitischeWeltkarteDialog PolitischeWeltkarteDialog { get; private set; }

        public ITestamentAnzeigenDialog TestamentAnzeigenDialog { get; private set; }

        public IProzentwertFestlegenDialog ProzentwertFestlegenDialog { get; private set; }

        public IUntergebeneDialog UntergebeneDialog { get; private set; }       

        public void Initialisieren(IYesNoQuestion yesNoQuestion, ITextAnzeigen textAnzeigen, IBeziehungPflegen beziehungPflegen, IBauwerkStiftenDialog bauwerkStiftenDialog,
                                   IFestGebenDialog festGebenDialog, IPolitischeWeltkarteDialog politischeWeltkarteDialog, ITestamentAnzeigenDialog testamentAnzeigenDialog,
                                   IProzentwertFestlegenDialog prozentwertFestlegenDialog, IUntergebeneDialog untergebeneDialog)
        {
            YesNoQuestion = yesNoQuestion;
            TextAnzeigen = textAnzeigen;
            BeziehungPflegen = beziehungPflegen;
            BauwerkStiftenDialog = bauwerkStiftenDialog;
            FestGebenDialog = festGebenDialog;
            PolitischeWeltkarteDialog = politischeWeltkarteDialog;
            TestamentAnzeigenDialog = testamentAnzeigenDialog;
            ProzentwertFestlegenDialog = prozentwertFestlegenDialog;
            UntergebeneDialog = untergebeneDialog;
        }
    }
}
