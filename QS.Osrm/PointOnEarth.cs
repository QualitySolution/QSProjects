namespace QS.Osrm
{
	public struct PointOnEarth
	{
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public PointOnEarth(double lat, double lon)
		{
			Latitude = lat;
			Longitude = lon;
		}

		public PointOnEarth (decimal lat, decimal lon)
		{
			Latitude = (double)lat;
			Longitude = (double)lon;
		}
	}
}
