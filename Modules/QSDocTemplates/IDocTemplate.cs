using System;

namespace QSDocTemplates
{
	public interface IDocTemplate
	{
		string Name { get;}
		byte[] File { get; set;}
		IDocParser DocParser { get;}
	}
}

