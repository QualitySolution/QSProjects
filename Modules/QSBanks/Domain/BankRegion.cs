using System.Data.Bindings;
using QSOrmProject;

namespace QSBanks
{
	[OrmSubject ("Регионы банков")]
	public class BankRegion: PropertyChangedBase
	{
		public virtual int Id { get; set; }

		int regionNum;

		public virtual int RegionNum {
			get { return regionNum; }
			set { SetField (ref regionNum, value, () => RegionNum); }
		}

		string region;

		public virtual string Region {
			get { return region; }
			set { SetField (ref region, value, () => Region); }
		}

		string city;

		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		public static bool EqualsWithoutId (BankRegion first, BankRegion second)
		{
			return (first.City == second.City &&
			first.Region == second.Region &&
			first.RegionNum == second.RegionNum);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is BankRegion))
				return false;
			
			return (City == (obj as BankRegion).City &&
			Id == (obj as BankRegion).Id &&
			RegionNum == (obj as BankRegion).RegionNum &&
			Region == (obj as BankRegion).Region);
		}

		public BankRegion ()
		{
			Region = string.Empty;
			City = string.Empty;
		}
	}
}

