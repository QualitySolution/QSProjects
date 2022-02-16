using NUnit.Framework;
using QS.DomainModel.UoW;
using System;
namespace QS.Test.DomainModel.UoW
{
	[TestFixture(TestOf = typeof(UnitOfWorkFactory))]
	public class UnitOfWorkFactoryTest
	{
		[Test(Description = "Было подозрение что вызов статического метода в результате, по цепочке в реальный класс передадут неправильно информацию о том кто вызвал метод.")]
		public void StaticCallsCorrectGetNameOfCallersTest()
		{
			var uow = UnitOfWorkFactory.CreateWithoutRoot();
			Assert.That(uow.ActionTitle.CallerMemberName, Is.EqualTo(nameof(StaticCallsCorrectGetNameOfCallersTest)));
		}
	}
}
