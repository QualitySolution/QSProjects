using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QSOsm.DTO;

namespace QSOsm.Loaders
{
	public abstract class OsmDataLoader
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

		protected IOsmService Osm { get; }

		protected OsmDataLoader(IOsmService osm)
		{
			Osm = osm ?? throw new ArgumentNullException(nameof(osm));
		}

		protected abstract void CancelLoading();
	}
}
