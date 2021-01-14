using System;
namespace QS.Project.Versioning
{
	public interface IApplicationInfo
	{
		byte ProductCode { get; }
		string ProductName { get; }
		string ProductTitle { get; }
		string Modification { get; }
		string ModificationTitle { get; }
		string[] СompatibleModifications { get; }
		Version Version { get; }

		bool IsBeta { get;  }
		DateTime BuildDate { get; }
	}
}
