using System;
using QS.Dialog;

namespace QS.Navigation
{
	public class AbortCreatingPageException : Exception
	{
		public string Title;
		public ImportanceLevel ImportanceLevel { get; }

		public AbortCreatingPageException(string userMessage, string title, ImportanceLevel importanceLevel = ImportanceLevel.Error) : base(userMessage) {
			Title = title;
			ImportanceLevel = importanceLevel;
		}
	}
}
