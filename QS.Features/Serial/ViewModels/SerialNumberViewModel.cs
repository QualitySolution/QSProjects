using System;
using QS.BaseParameters;
using QS.Navigation;
using QS.Serial.Encoding;
using QS.ViewModels.Dialog;

namespace QS.Serial.ViewModels
{
	public class SerialNumberViewModel : WindowDialogViewModelBase
	{
		private readonly SerialNumberEncoder SerialNumberEncoder;
		private readonly dynamic parametersService;

		public SerialNumberViewModel(INavigationManager navigation, SerialNumberEncoder encoder, ParametersService parametersService) : base(navigation)
		{
			SerialNumberEncoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
			serialNumber = this.parametersService.serial_number;

			Title = "Замена серийного номера";
		}

		#region Свойства

		private string serialNumber;
		public virtual string SerialNumber {
			get => serialNumber;
			set {
				if (SetField(ref serialNumber, value)) {
					SerialNumberEncoder.Number = SerialNumber;
					OnPropertyChanged(nameof(SensetiveOk));
					OnPropertyChanged(nameof(ResultText));
				}
			}
		}

		public bool SensetiveOk => String.IsNullOrWhiteSpace(SerialNumber) || SerialNumberEncoder.IsValid;

		public string ResultText { 
			get{
				if (SerialNumberEncoder.IsNotSupport)
					return "Формат серийного номера не поддерживается этой верией программы.\n" +
					"Если вы уверены что серийный номер правильный, попробуйте обновить программу.";
				if (SerialNumberEncoder.IsAnotherProduct) 
					return "Серийный номер от другого продукта.";
				return null;
			} 
		}

		#endregion

		public void Save()
		{
			parametersService.serial_number = SerialNumber;
			Close(false, CloseSource.Save);
		}
	}
}
