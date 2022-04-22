using System;
namespace QS.Project.Versioning.Product
{
	public class ProductEdition
	{
		public ProductEdition(byte number, string name)
		{
			Number = number;
			Name = name;
		}

		public byte Number { get; }
		public string Name { get; }
	}
}
