using System;
using Gamma.ColumnConfig;

namespace Gamma.GtkWidgets
{
	public static class ColumnsConfigFactory
	{
		public static FluentColumnsConfig<TNode> Create<TNode>()
		{
			return new FluentColumnsConfig<TNode> ();
		}
	}
}

