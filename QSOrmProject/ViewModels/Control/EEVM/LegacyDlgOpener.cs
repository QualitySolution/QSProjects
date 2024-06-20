using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.ViewModels.Control.EEVM {
	public class LegacyDlgOpener<TTdiDialog>
		: IEntityDlgOpener
		where TTdiDialog : ITdiTab {
		private readonly ITdiCompatibilityNavigation _tdiCompatibilityNavigation;
		private readonly DialogViewModelBase _masterViewModel;

		public LegacyDlgOpener(ITdiCompatibilityNavigation tdiCompatibilityNavigation, DialogViewModelBase masterViewModel) {
			_tdiCompatibilityNavigation = tdiCompatibilityNavigation;
			_masterViewModel = masterViewModel;
		}

		public void OpenEntityDlg(int id) {
			_tdiCompatibilityNavigation.OpenTdiTab<TTdiDialog, int>(_masterViewModel, id, OpenPageOptions.AsSlave);
		}
	}
}
