using System;


namespace QSHistoryLog
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
	                AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event,
	                AllowMultiple = false, Inherited = true)]
	public class IgnoreHistoryTraceAttribute : Attribute 
	{
	}


}