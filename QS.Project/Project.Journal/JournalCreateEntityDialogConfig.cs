using System;
using QS.Tdi;
namespace QS.Project.Journal
{
	public sealed class JournalCreateEntityDialogConfig
	{
		public string Title { get; }

		public Func<ITdiTab> OpenEntityDialogFunction { get; }

		public JournalCreateEntityDialogConfig(string title, Func<ITdiTab> openEntityDialogFunction)
		{
			Title = title;
			OpenEntityDialogFunction = openEntityDialogFunction ?? throw new ArgumentNullException(nameof(openEntityDialogFunction));
		}
	}
}
