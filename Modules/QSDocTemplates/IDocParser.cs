using System;
using System.Collections.Generic;

namespace QSDocTemplates
{
	public interface IDocParser
	{
		List<PatternField> FieldsList { get;}
		IEnumerable<IPatternTableField> TablesFields { get;}
		void UpdateFields();
		bool FieldsHasValues { get;}
		void SetDocObject(object doc);
	}
}

