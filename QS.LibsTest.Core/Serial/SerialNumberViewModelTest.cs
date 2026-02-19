using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Serial.Encoding;
using QS.Serial.ViewModels;

namespace QS.Test.Serial
{
	[TestFixture(TestOf = typeof(SerialNumberViewModel))]
	public class SerialNumberViewModelTest
	{
		private INavigationManager navigation;
		private ParametersService parametersService;
		private IApplicationQuitService quitService;
		private IInteractiveQuestion interactive;
		private IApplicationInfo appInfo;
		private IProductService productService;

		[SetUp]
		public void SetUp()
		{
			navigation = Substitute.For<INavigationManager>();
			quitService = Substitute.For<IApplicationQuitService>();
			interactive = Substitute.For<IInteractiveQuestion>();
			
			// Создаем реальный ParametersService для тестов с пустым словарем
			// ParametersService наследуется от DynamicObject и не может быть замокан
			var parameters = new Dictionary<string, string>();
			parameters["serial_number"] = "";
			parametersService = new ParametersService(parameters);
			
			// Создаем mock для IApplicationInfo
			appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductCode.Returns((byte)2); // workwear product code
			appInfo.ProductName.Returns("workwear");
			
			// Создаем mock для IProductService
			productService = Substitute.For<IProductService>();
			productService.GetEditionName(1).Returns("Однопользовательская");
			productService.GetEditionName(2).Returns("Профессиональная");
			productService.GetEditionName(3).Returns("Предприятие");
			productService.GetEditionName(4).Returns("Спецаутсорсинг");
		}

		[Test(Description = "Тест проверки правильных и неправильных серийных номеров через диалог")]
		public void SerialNumberDialog_ValidateMultipleSerialNumbers_Test()
		{
			var encoder = new SerialNumberEncoder(appInfo);
			var viewModel = new SerialNumberViewModel(navigation, encoder, parametersService, quitService, interactive, productService);

			// Тест 1: Правильный серийный номер версии 2
			viewModel.SerialNumber = "XgAc-j7gz-2pJ3-J7"; // Профессиональная 2 пользователя
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для правильного серийного номера");
			Assert.That(viewModel.ResultText, Is.EqualTo("Редакция: Профессиональная"), "Должна отображаться редакция Профессиональная");

			// Тест 2: Правильный серийный номер версии 3
			viewModel.SerialNumber = "2i2z-gcqW-BK1V-Cm4r-eFjJ-8G2"; // Профессиональная 2 пользователя
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для правильного серийного номера V3");
			Assert.That(viewModel.ResultText, Is.EqualTo("Редакция: Профессиональная"), "Должна отображаться редакция Профессиональная");

			// Тест 3: Неправильный серийный номер
			viewModel.SerialNumber = "WPDi-j6j5-88VB-HK"; // Ошибочный
			Assert.That(viewModel.SensitiveOk, Is.False, "Кнопка Save должна быть неактивна для неправильного серийного номера");

			// Тест 4: Истекший серийный номер
			viewModel.SerialNumber = "BJbw-eP4K-gvRn-aXtD-5"; // Спецаутсорсинг с датой окончания 2024-10-03
			Assert.That(viewModel.SensitiveOk, Is.False, "Кнопка Save должна быть неактивна для истекшего серийного номера");
			Assert.That(viewModel.ResultText, Does.Contain("Срок действия серийного номера истек"), "Должно отображаться сообщение об истечении срока");

			// Тест 5: Пустой серийный номер (должен быть разрешен)
			viewModel.SerialNumber = "";
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для пустого серийного номера");

			// Тест 6: Неправильный формат
			viewModel.SerialNumber = "Vw9k-kiEi-5LP7-Андр-ей"; // Ошибочный (кириллица)
			Assert.That(viewModel.SensitiveOk, Is.False, "Кнопка Save должна быть неактивна для серийного номера с неправильным форматом");

			// Тест 7: Однопользовательская редакция
			viewModel.SerialNumber = "WPDi-ZMra-QnAe-jS"; // Однопользовательская
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для правильного серийного номера");
			Assert.That(viewModel.ResultText, Is.EqualTo("Редакция: Однопользовательская"), "Должна отображаться редакция Однопользовательская");

			// Тест 8: Предприятие редакция
			viewModel.SerialNumber = "XgAc-tpMz-qdne-fM"; // Предприятие 1 пользователь
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для правильного серийного номера");
			Assert.That(viewModel.ResultText, Is.EqualTo("Редакция: Предприятие"), "Должна отображаться редакция Предприятие");
		}

		[Test(Description = "Тест проверки отображения редакции Профессиональная для серийного номера NQZ1-BNZq-wGFA-xA5B-uJ")]
		public void SerialNumberDialog_ProfessionalEdition_SpecificSerialNumber_Test()
		{
			var encoder = new SerialNumberEncoder(appInfo);
			var viewModel = new SerialNumberViewModel(navigation, encoder, parametersService, quitService, interactive, productService);

			// Устанавливаем специальный серийный номер
			viewModel.SerialNumber = "NQZ1-BNZq-wGFA-xA5B-uJ";

			// Проверяем что кнопка Save активна (серийный номер валидный)
			Assert.That(viewModel.SensitiveOk, Is.True, "Кнопка Save должна быть активна для серийного номера NQZ1-BNZq-wGFA-xA5B-uJ");

			// Проверяем что отображается редакция Профессиональная
			Assert.That(viewModel.ResultText, Is.EqualTo("Редакция: Профессиональная"), "Для серийного номера NQZ1-BNZq-wGFA-xA5B-uJ должна отображаться редакция Профессиональная");
		}
	}
}
