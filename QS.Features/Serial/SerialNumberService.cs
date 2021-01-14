using System;
namespace QS.Serial
{
	public class SerialNumberService : ISerialNumberService
	{
		public SerialNumberService(BaseParameters.ParametersService parametersService)
		{
			SerialNumber = parametersService.Dynamic.serial_number;
		}

		public SerialNumberService(string serialNumber)
		{
			SerialNumber = serialNumber;
		}

		public string SerialNumber { get; private set; }
	}
}
