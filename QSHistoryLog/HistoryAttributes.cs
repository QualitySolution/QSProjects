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

	/// <summary>
	/// Установка этого атрибута на поле, говорит трекеру следить за изменениями внутри объекта, а не только изменению значения самого поля.
	/// ВНИМАНИЕ! В текущей реализации устанавливается только на поле(обычно приватное), не на свойство.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class HistoryTraceGoDeepAttribute : Attribute 
	{
	}

	/// <summary>
	/// Установка этот атрибут на поле с коллекцией, говорит трекеру следить за изменениями внутри элементов этой коллекции, а не только изменению значения самой коллекции.
	/// ВНИМАНИЕ! В текущей реализации устанавливается только на поле(обычно приватное), не на свойство.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class HistoryDeepCloneItemsAttribute : Attribute 
	{
	}

	/// <summary>
	/// Для поле типа DateTime в журнал изменений пишем только дату, без времени.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
					AllowMultiple = false, Inherited = true)]
	//TODO Пока не реализовано в трекере.
	public class HistoryDateOnlyAttribute : Attribute
	{
	}

}