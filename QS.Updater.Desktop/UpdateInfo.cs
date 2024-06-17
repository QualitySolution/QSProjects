using QS.Dialog;

namespace QS.Updater
{
	public readonly struct UpdateInfo 
	{
		public string Title { get; }
		
		public string Message { get; }
		
		public UpdateStatus Status { get; }
		
		public ImportanceLevel ImportanceLevel { get; }
		
		public UpdateInfo(string title, string message, UpdateStatus status, ImportanceLevel importanceLevel) 
		{
			Title = title;
			Message = message;
			Status = status;
			ImportanceLevel = importanceLevel;
		}
	}
	
	public enum UpdateStatus 
	{
		Ok,
		Skip,
		Shelve,
		UpToDate,
		ExternalError,
		Error
	}
}
