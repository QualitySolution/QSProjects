using System;
using System.Collections;

namespace QS.Project.Journal.EntitySelector
{
	public interface IEntityAutocompleteSelector : IEntitySelector
	{
		IList Items { get; }
		void SearchValues(params string[] values);
		event EventHandler ListUpdated;
	}
}
