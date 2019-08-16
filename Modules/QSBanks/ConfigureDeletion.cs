using QS.Banks;
using QS.Banks.Domain;
using QS.Deletion;

namespace QSBanks
{
	public static partial class QSBanksMain
	{
		public static void ConfigureDeletion()
		{
			ConfigureDeletionBanks.ConfigureDeletion();
		}
	}
}

