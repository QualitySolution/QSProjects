using System;
using System.Collections;

namespace Gamma.Binding.Core.LevelTreeConfig
{
	public interface ILevelConfig
	{
		ILevelConfig LevelUp { get;}

		Type NodeType { get;}

		bool IsFirstLevel { get;}
		bool IsLastLevel { get; }

		object GetParent(object node);
		IList GetChilds(object node);
		int IndexOnParent(object node);
		IList MyList(object node);
	}
}

