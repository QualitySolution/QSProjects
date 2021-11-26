using System;
using QS.Tdi;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public sealed class JournalEntityDocumentsConfig
	{
		private List<JournalCreateEntityDialogConfig> createEntityDialogConfigs;
		private Func<JournalEntityNodeBase, ITdiTab> openEntityDialogFunction;
		private Func<JournalEntityNodeBase, bool> nodeIdentificationFunction;
		public JournalParametersForDocument JournalParameters { get; }
		private bool withoutCreation;

		public JournalEntityDocumentsConfig(
			string createActionTitle,
			Func<ITdiTab> createDialogFunc,
			Func<JournalEntityNodeBase, ITdiTab> openDialogFunc,
			Func<JournalEntityNodeBase, bool> nodeIdentificationFunc,
			JournalParametersForDocument journalParameters = null)
		{
			createEntityDialogConfigs = new List<JournalCreateEntityDialogConfig>();
			openEntityDialogFunction = openDialogFunc;
			nodeIdentificationFunction = nodeIdentificationFunc;
			JournalParameters = journalParameters ?? JournalParametersForDocument.DefaultShow;
			createEntityDialogConfigs.Add(new JournalCreateEntityDialogConfig(createActionTitle, createDialogFunc));
		}

		public JournalEntityDocumentsConfig(
			Func<JournalEntityNodeBase, ITdiTab> openDialogFunc,
			Func<JournalEntityNodeBase, bool> nodeIdentificationFunc,
			JournalParametersForDocument journalParameters = null)
		{
			withoutCreation = true;
			createEntityDialogConfigs = new List<JournalCreateEntityDialogConfig>();
			openEntityDialogFunction = openDialogFunc;
			nodeIdentificationFunction = nodeIdentificationFunc;
			JournalParameters = journalParameters ?? JournalParametersForDocument.DefaultShow;
		}

		public void AddCreateDialogConfig(string title, Func<ITdiTab> createDialogFunc)
		{
			if(withoutCreation) {
				return;
			}
			createEntityDialogConfigs.Add(new JournalCreateEntityDialogConfig(title, createDialogFunc));
		}

		public bool IsIdentified(JournalEntityNodeBase node)
		{
			return nodeIdentificationFunction.Invoke(node);
		}

		public Func<JournalEntityNodeBase, ITdiTab> GetOpenEntityDlgFunction()
		{
			return openEntityDialogFunction;
		}

		public IEnumerable<JournalCreateEntityDialogConfig> GetCreateEntityDlgConfigs()
		{
			return createEntityDialogConfigs;
		}
	}
}
