using System;
using QS.Tdi;

namespace QS.ViewModels.Control.EEVM
{
	public interface ILegacyEEVMBuilderParameters : IEEVMBuilderParameters
	{
		ITdiTab DialogTab { get; }
	}
}
