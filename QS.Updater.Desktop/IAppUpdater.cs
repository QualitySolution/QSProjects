namespace QS.Updater {
	public interface IAppUpdater {
		void CheckUpdate(bool manualRun);
		void TryAnotherChannel();
	}
}
