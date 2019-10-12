using System;
namespace QS.Project.Journal.DataLoader
{
	public class SortRule<TNode>
	{
		public Func<TNode, object> GetOrderByValue { get; private set; }
		public bool Descending { get; private set; }

		public SortRule(Func<TNode, object> orderByFunc, bool descending)
		{
			GetOrderByValue = orderByFunc;
			Descending = descending;
		}
	}
}
