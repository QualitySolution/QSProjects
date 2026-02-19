using System;
using System.Diagnostics;
using System.Reflection;
using QS.BaseParameters;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Versioning.Product;
using QS.Serial.Encoding;
using QS.ViewModels.Dialog;

namespace QS.Serial.ViewModels
{
	public class SerialNumberViewModel : WindowDialogViewModelBase
	{
		private readonly SerialNumberEncoder SerialNumberEncoder;
		private readonly dynamic parametersService;
		private readonly IApplicationQuitService quitService;
		private readonly IInteractiveQuestion interactive;
		private readonly IProductService productService;
		private readonly byte lastEdition;

		public SerialNumberViewModel(
			INavigationManager navigation,
			SerialNumberEncoder encoder,
			ParametersService parametersService,
			IApplicationQuitService quitService, 
			IInteractiveQuestion interactive,
			IProductService productService = null) : base(navigation)
		{
			SerialNumberEncoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			this.quitService = quitService ?? throw new ArgumentNullException(nameof(quitService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.productService = productService;
			serialNumber = this.parametersService.serial_number;
			Title = "Замена серийного номера";
			SerialNumberEncoder.Number = serialNumber;
			lastEdition = SerialNumberEncoder.EditionId;
		}

		#region Свойства

		private string serialNumber;
		public virtual string SerialNumber {
			get => serialNumber;
			set {
				if (SetField(ref serialNumber, value)) {
					SerialNumberEncoder.Number = SerialNumber;
					OnPropertyChanged(nameof(SensitiveOk));
					OnPropertyChanged(nameof(ResultText));
				}
			}
		}

		public bool SensitiveOk => String.IsNullOrWhiteSpace(SerialNumber) || (SerialNumberEncoder.IsValid && !SerialNumberEncoder.IsExpired);

		public string ResultText { 
			get{
				if (SerialNumberEncoder.IsNotSupport)
					return "Формат серийного номера не поддерживается этой версией программы.\n" +
					"Если вы уверены что серийный номер правильный, попробуйте обновить программу.";
				if (SerialNumberEncoder.IsAnotherProduct) 
					return "Серийный номер от другого продукта.";
				if(SerialNumberEncoder.IsValid) {
					string result = String.Empty;
					if(productService != null)
						result = "Редакция: " + productService.GetEditionName(SerialNumberEncoder.EditionId);
					else {
						result = "Корректен";
					}
					
					if(SerialNumberEncoder.ExpiryDate.HasValue) {
						var dateStr = SerialNumberEncoder.ExpiryDate.Value.ToShortDateString();
						result += $"\nДействителен до: {dateStr}";
					}
					
					if(SerialNumberEncoder.IsExpired) {
						result += "\nСрок действия серийного номера истек";
					}
					
					return result;
				}
				return null;
			} 
		}

		#endregion

		public void Save()
		{
			parametersService.serial_number = SerialNumber;
			Close(false, CloseSource.Save);
			if(lastEdition != SerialNumberEncoder.EditionId &&
				interactive.Question("Редакция программы изменилась, перезапустить приложение?")) {
				Process.Start(Assembly.GetEntryAssembly().Location);
				quitService.Quit();
			}
		}
	}
}
