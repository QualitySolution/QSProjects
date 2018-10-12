using System;
using System.ComponentModel.DataAnnotations;

namespace QSOsm.DTO
{

	/// <summary>
	/// Тип населенного пункта. Маленькими буквами для совпадения с соответствующими типами в OSM.
	/// </summary>
	public enum LocalityType
	{
		[Display (Name = "Город", ShortName = "г.")]
		city,
		[Display (Name = "Город", ShortName = "г.")]
		town,
		[Display (Name = "Населенный пункт", ShortName = "н.п.")]
		village,
		[Display (Name = "Дачный поселок", ShortName = "д.п.")]
		allotments,
		[Display (Name = "Деревня", ShortName = "дер.")]
		hamlet,
		[Display (Name = "Ферма", ShortName = "фер.")]
		farm,
		[Display (Name = "Хутор", ShortName = "х.")]
		isolated_dwelling
	}

	public enum RoomType
	{
		[Display (Name = "Квартира", ShortName = "кв.")]
		Apartment,
		[Display (Name = "Офис", ShortName = "оф.")]
		Office,
		[Display(Name = "Склад", ShortName = "склад")]
		Store,
		[Display(Name = "Помещение", ShortName = "пом.")]
		Room,
		[Display(Name = "Комната", ShortName = "ком.")]
		Chamber,
		[Display(Name = "Секция", ShortName = "сек.")]
		Section
	}

	public class RoomTypeStringType : NHibernate.Type.EnumStringType
	{
		public RoomTypeStringType () : base (typeof(RoomType))
		{
		}
	}

	public class LocalityTypeStringType : NHibernate.Type.EnumStringType
	{
		public LocalityTypeStringType () : base (typeof(LocalityType))
		{
		}
	}

}
