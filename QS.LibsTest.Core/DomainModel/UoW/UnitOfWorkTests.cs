using NUnit.Framework;
using System;
using QS.DomainModel.UoW;
using QS.Testing.DB;

namespace QS.Test.DomainModel.UoW
{
	[TestFixture()]
	public class UnitOfWorkTests : InMemoryDBTestFixtureBase
	{
		[Test(Description = "Проверка что при попытке открытия транзакции в закрытой сессии выводится эксепшн о закрытой сессии, а не внутренний эксепш хибернейта")]
		public void OpenTransactionMustThrowObjectDisposedExceptionWhenSessionIsClose()
		{
			//arrange
			InitialiseNHibernate();

			var uow = UnitOfWorkFactory.CreateWithoutRoot();
			UnitOfWorkBase uowBaseClass = (UnitOfWorkBase)uow;
			uowBaseClass.OpenTransaction();
			uow.Session.Close();

			//act, assert
			Assert.Throws(typeof(ObjectDisposedException), uowBaseClass.OpenTransaction, "Session is closed!");
		}
	}
}
