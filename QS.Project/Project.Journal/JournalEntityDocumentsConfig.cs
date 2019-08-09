using System;
using QS.Tdi;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public sealed class JournalEntityDocumentsConfig<TNode>
		where TNode : JournalEntityNodeBase
	{
		private List<JournalCreateEntityDialogConfig> createEntityDialogConfigs;
		private Func<TNode, ITdiTab> openEntityDialogFunction;
		private Func<TNode, bool> nodeIdentificationFunction;
		private readonly bool hideJournal;
		private bool withoutCreation;


		public JournalEntityDocumentsConfig(string createActionTitle, Func<ITdiTab> createDialogFunc, Func<TNode, ITdiTab> openDialogFunc, Func<TNode, bool> nodeIdentificationFunc, bool hideJournal = false)
		{
			createEntityDialogConfigs = new List<JournalCreateEntityDialogConfig>();
			openEntityDialogFunction = openDialogFunc;
			nodeIdentificationFunction = nodeIdentificationFunc;
			this.hideJournal = hideJournal;
			createEntityDialogConfigs.Add(new JournalCreateEntityDialogConfig(createActionTitle, createDialogFunc));
		}

		public JournalEntityDocumentsConfig(Func<TNode, ITdiTab> openDialogFunc, Func<TNode, bool> nodeIdentificationFunc, bool hideJournal = false)
		{
			withoutCreation = true;
			createEntityDialogConfigs = new List<JournalCreateEntityDialogConfig>();
			openEntityDialogFunction = openDialogFunc;
			nodeIdentificationFunction = nodeIdentificationFunc;
			this.hideJournal = hideJournal;
		}

		public void AddCreateDialogConfig(string title, Func<ITdiTab> createDialogFunc)
		{
			if(withoutCreation) {
				return;
			}
			createEntityDialogConfigs.Add(new JournalCreateEntityDialogConfig(title, createDialogFunc));
		}

		public bool IsIdentified(TNode node)
		{
			return nodeIdentificationFunction.Invoke(node);
		}

		public Func<TNode, ITdiTab> GetOpenEntityDlgFunction()
		{
			return openEntityDialogFunction;
		}

		public IEnumerable<JournalCreateEntityDialogConfig> GetCreateEntityDlgConfigs()
		{
			return createEntityDialogConfigs;
		}
	}
}
