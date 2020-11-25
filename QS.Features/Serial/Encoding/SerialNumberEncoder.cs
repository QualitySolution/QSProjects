using System;
using QS.Project.Versioning;

namespace QS.Serial.Encoding
{
	public class SerialNumberEncoder
	{
		public byte CodeVersion { get; private set;}

		public string DecodedProduct { get; private set;}

		public bool IsValid { get; private set;}

		public bool IsNotSupport { get; private set;}

		public bool IsAnotherProduct { get; private set; }

		private string number;
		private readonly string forProduct;

		public SerialNumberEncoder(IApplicationInfo info)
		{
			this.forProduct = info.ProductName;
		}

		public virtual string Number {
			get {return number;}
			set { 
				if (number == value)
					return;

				number = value;
				Clear();

				string digits = value.Replace("-", "");

				if (digits.Length < 10)
					return;

				byte[] summaryArray;

				try
				{
					summaryArray = Base58Encoding.DecodeWithCheckSum(digits);
				}
				catch (FormatException ex)
				{
					return;
				}

				if (summaryArray.Length < 9)
				{
					return;
				}

				CodeVersion = summaryArray[0];

				if (CodeVersion != 1)
				{
					IsNotSupport = true;
					return;
				}

				DecodedProduct = SerialCommon.GetProductFromBinary(summaryArray, 8);
				IsAnotherProduct = DecodedProduct != forProduct;
				IsValid = !IsAnotherProduct && !IsNotSupport;
			}
		}

		private void Clear()
		{
			DecodedProduct = String.Empty;
			CodeVersion = default(byte);
			IsValid = IsNotSupport = IsAnotherProduct = false;
		}

		public string ComponentsText{
			get{
				if(IsNotSupport)
					return "Версия формата не поддерживается.";
				else if (IsValid)
					return String.Format("Версия кодирования: {0}\n" +
						"Продукт: {1}",
						CodeVersion,
						DecodedProduct
					);
				else
					return "Не корректный Сер. номер.";
			}
		}
	}
}

