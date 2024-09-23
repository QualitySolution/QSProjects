using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using QS.Project.DB;

namespace QS.Deletion.Configuration
{

	public class ClearDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;
		public string ClearField;

		/// <summary>
		/// Необходимо для проверки и удаления через NHibernate
		/// </summary>
		public string PropertyName;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public ClearDependenceInfo(Type objectClass, string sqlwhere, string clearField)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
			ClearField = clearField;
		}

		public ClearDependenceInfo(string tableName, string sqlwhere, string clearField)
		{
			TableName = tableName;
			WhereStatment = sqlwhere;
			ClearField = clearField;
		}

		public ClearDependenceInfo AddCheckProperty(string property)
		{
			PropertyName = property;
			return this;
		}

		/// <summary>
		/// Создает класс описания очистки колонки на основе свойства беря информацию из NHibernate
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static ClearDependenceInfo Create<TObject> (Expression<Func<TObject, object>> propertyRefExpr, NHibernate.Cfg.Configuration cfg){
			string propName = PropertyUtil.GetName (propertyRefExpr);
			string fieldName = cfg.GetClassMapping (typeof(TObject)).GetProperty (propName).ColumnIterator.First ().Text;
			return new ClearDependenceInfo(typeof(TObject),
				String.Format ("WHERE {0} = @id", fieldName),
				fieldName
			).AddCheckProperty(propName);
		}
	}

}
