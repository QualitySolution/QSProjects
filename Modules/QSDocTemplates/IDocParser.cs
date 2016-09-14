using System;
using System.Collections.Generic;

namespace QSDocTemplates
{
	public interface IDocParser
	{
		List<PatternField> FieldsList { get;}
		void UpdateFields();
		bool FieldsHasValues { get;}
	}
}

