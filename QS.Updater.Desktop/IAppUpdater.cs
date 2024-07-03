namespace QS.Updater {
	public interface IAppUpdater {
		UpdateInfo CheckUpdate(bool manualRun);
		void TryAnotherChannel();
	}
}
