using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.JournalViewModels {
	public class DocumentJournalNode 
	{
		protected DocumentJournalNode()
		{
		}

		public int Id { get; set; }
		public string Title { get; }
		public DateTime Date { get; set; }
	}
	
	public class DocumentJournalNode<TDocument> : DocumentJournalNode
		where TDocument : class, IDomainObject
	{
		protected DocumentJournalNode()
		{
		}
	}
}
