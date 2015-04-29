using System;
using System.Linq;
using System.Linq.Expressions;

namespace QSOrmProject.Deletion
{

	public class DeleteDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public DeleteDependenceInfo(Type objectClass, string sqlwhere)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
		}

		public DeleteDependenceInfo(string tableName, string sqlwhere)
		{
			TableName = tableName;
			WhereStatment = sqlwhere;
		}

		public DeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.Find (i => i.TableName == TableName);
		}

		/// <summary>
		/// Создает класс описания удаления на основе свойства объекта беря информацию из NHibernate.
		/// Удалятся все объекты указанного типа, указанное свойство которых равно удаляемому объекту.
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo Create<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string propName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			string fieldName = OrmMain.ormConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).ColumnIterator.First ().Text;
			return new DeleteDependenceInfo(typeof(TObject),
				String.Format ("WHERE {0} = @id", fieldName)
			);
		}
	}

}
