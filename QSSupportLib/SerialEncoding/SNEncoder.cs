using System;

namespace QSSupportLib.Serial
{
	public class SNEncoder
	{
		public byte CodeVersion { get; private set;}

		public string Product { get; private set;}

		public bool IsValid { get; private set;}

		public bool IsNotSupport { get; private set;}

		private string number;

		public virtual string Number {
			get {return number;}
			set { 
				if (number == value)
					return;

				number = value;
				Clear();

				string digits = value.Replace("-", "");
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

				Product = SerialCommon.GetProductFromBinary(summaryArray, 8);

				IsValid = true;
			}
		}

		private void Clear()
		{
			Product = String.Empty;
			CodeVersion = default(byte);
			IsValid = IsNotSupport = false;
		}

		public string ComponentsText{
			get{
				if(IsNotSupport)
					return "Версия формата не поддерживается.";
				else if (IsValid)
					return String.Format("Версия кодирования: {0}\n" +
						"Продукт: {1}",
						CodeVersion,
						Product
					);
				else
					return "Не корректный Сер. номер.";
			}
		}


	}
}

