using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Mapping;

namespace QSOrmProject.Deletion
{

	public class DeleteDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;

		/// <summary>
		/// Используется только для проверки зависимостей в NHibernate, не нужно для удаления
		/// </summary>
		public string PropertyName;
        public string CollectionName;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public DeleteDependenceInfo(Type objectClass, string sqlwhere, string property)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
			PropertyName = property;
		}

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

		public IDeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.OfType<DeleteInfo> ().First (i => i.TableName == TableName);
		}

        public DeleteDependenceInfo AddCheckProperty(string property)
        {
            PropertyName = property;
            return this;
        }

        public DeleteDependenceInfo AddCheckProperty<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
			if (ObjectClass != typeof(TObject))
				throw new InvalidOperationException(String.Format("Тип {0} должен соответствовать типу правила({1})", typeof(TObject).Name, ObjectClass.Name));
            string propName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
            PropertyName = propName;
            return this;
        }

        public DeleteDependenceInfo AddCheckCollection(string property)
        {
            CollectionName = property;
            return this;
        }

		/// <summary>
		/// Создает класс описания удаления на основе свойства объекта беря информацию из NHibernate.
		/// Удалятся все объекты указанного типа, указанное свойство которых равно удаляемому объекту.
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo Create<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string propName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			string fieldName = OrmMain.OrmConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).ColumnIterator.First ().Text;
			return new DeleteDependenceInfo(typeof(TObject),
				String.Format ("WHERE {0} = @id", fieldName),
				propName
			);
		}

		/// <summary>
		/// Создает класс описания удаления на основе свойства bag в родителе. 
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo CreateFromBag<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string propName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			var collectionMap = OrmMain.OrmConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).Value as Bag;
			Type itemType = (collectionMap.Element as OneToMany).AssociatedClass.MappedClass;
			string fieldName = collectionMap.Key.ColumnIterator.First ().Text;
			return new DeleteDependenceInfo(itemType,
				String.Format ("WHERE {0} = @id", fieldName)
            ).AddCheckCollection(propName);
		}

	}

}
