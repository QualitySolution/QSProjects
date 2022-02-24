using System;

namespace QS.Utilities.Spatial
{
	public static class DistanceHelper
	{
		private const double _equatorialEarthRadius = 6378.1370D;
		private const double _d2R = Math.PI / 180D;

		public static double GetDistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
		{
			double dlong = (longitude2 - longitude1) * _d2R;
			double dlat = (latitude2 - latitude1) * _d2R;
			double a = Math.Pow(Math.Sin(dlat / 2D), 2D)
				+ Math.Cos(latitude1 * _d2R) * Math.Cos(latitude2 * _d2R) * Math.Pow(Math.Sin(dlong / 2D), 2D);
			double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
			double d = _equatorialEarthRadius * c;

			return d;
		}
	}
}
