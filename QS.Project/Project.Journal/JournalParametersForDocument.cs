using System;
namespace QS.Project.Journal
{
	public class JournalParametersForDocument
	{
		public static JournalParametersForDocument DefaultShow => new JournalParametersForDocument() {
			HideJournalForCreateDialog = true,
			HideJournalForOpenDialog = true
		};

		public bool HideJournalForCreateDialog { get; set; }
		public bool HideJournalForOpenDialog { get; set; }
	}
}
