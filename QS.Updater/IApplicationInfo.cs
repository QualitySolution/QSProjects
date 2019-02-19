using System;
namespace QS.Updater
{
	public interface IApplicationInfo
	{
		string ProductName { get; }
		string Edition { get; }
		Version Version { get; }
		string SerialNumber { get; }
	}
}
