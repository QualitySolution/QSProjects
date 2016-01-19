using System;
using System.Linq;
using System.Linq.Expressions;

namespace QSOrmProject.Deletion
{

	public class ClearDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;
		public string[] ClearFields;

		/// <summary>
		/// Используется только для проверки зависимостей в NHibernate, не нужно для удаления
		/// </summary>
		public string PropertyName;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public ClearDependenceInfo(Type objectClass, string sqlwhere, params string[] clearField)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
			ClearFields = clearField;
		}

		public ClearDependenceInfo(string tableName, string sqlwhere, params string[] clearField)
		{
			TableName = tableName;
			WhereStatment = sqlwhere;
			ClearFields = clearField;
		}

		public IDeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.OfType<DeleteInfo>().First (i => i.TableName == TableName);
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
		public static ClearDependenceInfo Create<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string propName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			string fieldName = OrmMain.ormConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).ColumnIterator.First ().Text;
			return new ClearDependenceInfo(typeof(TObject),
				String.Format ("WHERE {0} = @id", fieldName),
				new string[] {fieldName}
			).AddCheckProperty(propName);
		}
	}

}
