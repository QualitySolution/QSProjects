using System;
namespace QS.Project.VersionControl
{
	public interface IApplicationInfo
	{
		string ProductName { get; }
		string ProductTitle { get; }
		string Modification { get; }
		string ModificationTitle { get; }
		string[] СompatibleModifications { get; }
		Version Version { get; }
		string SerialNumber { get; }

		bool IsBeta { get;  }
		DateTime BuildDate { get; }
	}
}
