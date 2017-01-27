using System;
using System.ComponentModel.DataAnnotations;

namespace QSOsm.DTO
{

	/// <summary>
	/// Тип населенного пункта. Маленькими буквами для совпадения с соответствующими типами в OSM.
	/// </summary>
	public enum LocalityType
	{
		[Display (Name = "Город")]
		city,
		[Display (Name = "Город")]
		town,
		[Display (Name = "Населенный пункт")]
		village,
		[Display (Name = "Дачный поселок")]
		allotments,
		[Display (Name = "Деревня")]
		hamlet,
		[Display (Name = "Ферма")]
		farm,
		[Display (Name = "Хутор")]
		isolated_dwelling
	}

	public enum RoomType
	{
		[Display (Name = "Квартира")]
		Apartment,
		[Display (Name = "Офис")]
		Office,
		[Display (Name = "Помещение")]
		Room
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
