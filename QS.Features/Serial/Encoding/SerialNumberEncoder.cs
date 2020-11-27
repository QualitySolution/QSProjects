using System;
using QS.DomainModel.Entity;
using QS.Project.Versioning;

namespace QS.Serial.Encoding
{
	public class SerialNumberEncoder : PropertyChangedBase
	{
		#region Компоненты
		public byte CodeVersion { get; private set;}

		#region Версия 1
		public string DecodedProduct { get; private set;}
		#endregion

		#region Версия 2
		public byte ProductId { get; private set; }
		public byte EditionId { get; private set; }
		#endregion

		#endregion
		public bool IsValid { get; private set;}

		public bool IsNotSupport { get; private set;}

		public bool IsAnotherProduct { get; private set; }

		private string number;
		private readonly string forProduct;
		private readonly IApplicationInfo applicationInfo;

		public SerialNumberEncoder(IApplicationInfo applicationInfo)
		{
			this.forProduct = applicationInfo?.ProductName;
			this.applicationInfo = applicationInfo;
		}

		[PropertyChangedAlso("ComponentsText")]
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

				if (CodeVersion != 1 && CodeVersion != 2)
				{
					IsNotSupport = true;
					return;
				}

				switch(CodeVersion) {
					case 1:
						DecodedProduct = SerialCommon.GetProductFromBinary(summaryArray, 8);
						IsAnotherProduct = DecodedProduct != forProduct;
						break;
					case 2:
						ProductId = summaryArray[8];
						EditionId = summaryArray[9];
						IsAnotherProduct = ProductId != applicationInfo?.ProductCode;
						break;
				}
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
				else 
					return String.Format("Версия кодирования: {0}\n" +
						"Продукт: {1}",
						CodeVersion,
						DecodedProduct
					);
			}
		}
	}
}

