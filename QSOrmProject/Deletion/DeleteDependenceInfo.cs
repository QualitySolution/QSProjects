using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Mapping;

namespace QSOrmProject.Deletion
{

	public class DeleteDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;

		/// <summary>
		/// Необходимо для проверки и удаления через NHibernate
		/// </summary>
		public string PropertyName;
        public string CollectionName;
		public string ParentPropertyName;

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

		private DeleteDependenceInfo() {}

		public IDeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.OfType<DeleteInfo> ().FirstOrDefault (i => i.TableName == TableName);
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
			string propName = PropertyUtil.GetName (propertyRefExpr);
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
			string propName = PropertyUtil.GetName (propertyRefExpr);
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
			string propName = PropertyUtil.GetName (propertyRefExpr);
			var collectionMap = OrmMain.OrmConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).Value as Bag;
			Type itemType = (collectionMap.Element as OneToMany).AssociatedClass.MappedClass;
			string fieldName = collectionMap.Key.ColumnIterator.First ().Text;
			return new DeleteDependenceInfo(itemType,
				String.Format ("WHERE {0} = @id", fieldName)
            ).AddCheckCollection(propName);
		}

		/// <summary>
		/// Создает класс описания удаления на основе свойства в родителе. 
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo CreateFromParentPropery<TObject> (Expression<Func<TObject, object>> propertyRefExpr){
			string propName = PropertyUtil.GetName (propertyRefExpr);
			var parentMap = OrmMain.OrmConfig.GetClassMapping(typeof(TObject));
			var propertyMap = OrmMain.OrmConfig.GetClassMapping (typeof(TObject)).GetProperty (propName).Value as ManyToOne;
			Type itemType = propertyMap.Type.ReturnedClass;
			var itemMap = OrmMain.OrmConfig.GetClassMapping(itemType);
			var parentTable = parentMap.Table.Name;
			string fieldName = propertyMap.ColumnIterator.First ().Text;
			return new DeleteDependenceInfo{
				ParentPropertyName = propName,
				ObjectClass = itemType,
				WhereStatment = String.Format("WHERE {0}.id = (SELECT {1} FROM {2} WHERE id = @id)", itemMap.Table.Name, fieldName, parentTable)
			};
		}
	}

}
