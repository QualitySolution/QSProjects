using System;


namespace QS.HistoryLog
{
	/// <summary>
	/// При установки этого атрибута на свойство изменение этого свойства не будет фиксироваться в истории изменений объекта
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class IgnoreHistoryTraceAttribute : Attribute 
	{
	}

	/// <summary>
	/// При установке на класс говорит о том что изменения в этом объекте необходимо записывать в историю изменений.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class HistoryTraceAttribute : Attribute
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

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
					AllowMultiple = false, Inherited = true)]
	public class HistoryIdentifierAttribute : Attribute {
		public Type TargetType { get; set; }
	}
}
