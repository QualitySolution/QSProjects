using System;


namespace QSHistoryLog
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
	                AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event,
	                AllowMultiple = false, Inherited = true)]
	public class IgnoreHistoryTraceAttribute : Attribute 
	{
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
	                    AttributeTargets.Field,
	                    AllowMultiple = false, Inherited = true)]
	public class IgnoreHistoryCloneAttribute : Attribute 
	{
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
	                    AttributeTargets.Field | AttributeTargets.Property,
	                    AllowMultiple = false, Inherited = true)]
	public class HistoryTraceGoDeepAttribute : Attribute 
	{
	}

	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property,
	                    AllowMultiple = false, Inherited = true)]
	public class HistoryDeepCloneItemsAttribute : Attribute 
	{
	}
}