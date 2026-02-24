using QS.Navigation;
using QS.Tdi;

namespace QS.ViewModels.Control.EEVM {
	public class LegacyDlgOpener<TTdiDialog>
		: IEntityDlgOpener
		where TTdiDialog : ITdiTab {
		private readonly ITdiCompatibilityNavigation _tdiCompatibilityNavigation;
		private readonly IDialogViewModel _masterViewModel;

		public LegacyDlgOpener(ITdiCompatibilityNavigation tdiCompatibilityNavigation, IDialogViewModel masterViewModel) {
			_tdiCompatibilityNavigation = tdiCompatibilityNavigation;
			_masterViewModel = masterViewModel;
		}

		public void OpenEntityDlg(int id) {
			_tdiCompatibilityNavigation.OpenTdiTab<TTdiDialog, int>(_masterViewModel, id, OpenPageOptions.AsSlave);
		}
	}
}
