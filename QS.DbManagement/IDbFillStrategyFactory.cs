namespace QS.DbManagement {
	public interface IDbFillStrategyFactory {
		IDbFillStrategy ForScript();

		IDbFillStrategy ForDump(string dumpFilePath);
	}
}
