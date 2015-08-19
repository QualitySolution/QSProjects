using System;
using System.Linq;

namespace GammaBinding.ColumnConfig
{
	public class RowMapping<TNode>
	{
		FluentColumnsConfig<TNode> myConfig;

		public RowMapping (FluentColumnsConfig<TNode> parentConfig)
		{
			this.myConfig = parentConfig;
		}

		public RowMapping<TNode> AddSetter<TCellRenderer>(Action<TCellRenderer, TNode> setter) where TCellRenderer : class
		{
			foreach(var cell in myConfig.ConfiguredColumns.OfType<ColumnMapping<TNode>> ()
				.SelectMany (c => c.ConfiguredRenderersGeneric))
			{
				cell.SetCommonSetter<TCellRenderer> (setter);
			}
			return this;
		}

		public IColumnsConfig Finish()
		{
			return myConfig.Finish ();
		}
	}
}

