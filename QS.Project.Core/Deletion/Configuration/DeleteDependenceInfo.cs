using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Mapping;
using QS.Project.DB;

namespace QS.Deletion.Configuration
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

		public bool IsCascade;
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
		public static DeleteDependenceInfo Create<TObject> (Expression<Func<TObject, object>> propertyRefExpr, NHibernate.Cfg.Configuration cfg){
			string propName = PropertyUtil.GetName (propertyRefExpr);
			string fieldName = cfg.GetClassMapping (typeof(TObject)).GetProperty (propName).ColumnIterator.First ().Text;
			return new DeleteDependenceInfo(typeof(TObject),
				String.Format ("WHERE {0} = @id", fieldName),
				propName
			);
		}

		[Obsolete("Используйте CreateFromCollection а лучше вызывайте метод AddDeleteDependenceFromCollection у DeleteInfoHibernate")]
		public static DeleteDependenceInfo CreateFromBag<TObject>(Expression<Func<TObject, object>> propertyRefExpr, NHibernate.Cfg.Configuration cfg)
		{
			string propName = PropertyUtil.GetName(propertyRefExpr);
			return CreateFromCollection<TObject>(propName, cfg);
		}

		/// <summary>
		/// Создает класс описания удаления на основе свойства коллекции в родителе. 
		/// </summary>
		/// <param name="propName">Имя </param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo CreateFromCollection<TObject>(string propName, NHibernate.Cfg.Configuration cfg)
		{
			var collectionMap = cfg.GetClassMapping(typeof(TObject)).GetProperty(propName).Value;
			var bagMap = collectionMap as Bag;
			var listMap = collectionMap as List;

			Type itemType;
			string fieldName;

			if(bagMap != null) {
				itemType = (bagMap.Element as OneToMany).AssociatedClass.MappedClass;
				fieldName = bagMap.Key.ColumnIterator.First().Text;
			} else if(listMap != null) {
				itemType = (listMap.Element as OneToMany).AssociatedClass.MappedClass;
				fieldName = listMap.Key.ColumnIterator.First().Text;
			} else
				throw new NotImplementedException($"Тип коллекции {collectionMap} не реализован.");

			return new DeleteDependenceInfo(itemType,
				String.Format("WHERE {0} = @id", fieldName)
			).AddCheckCollection(propName);
		}

		/// <summary>
		/// Создает класс описания удаления на основе свойства в родителе. 
		/// </summary>
		/// <param name="propertyRefExpr">Лямда функция указывающая на свойство, пример (e => e.Name)</param>
		/// <typeparam name="TObject">Тип объекта доменной модели</typeparam>
		public static DeleteDependenceInfo CreateFromParentPropery<TObject> (Expression<Func<TObject, object>> propertyRefExpr, NHibernate.Cfg.Configuration cfg){
			string propName = PropertyUtil.GetName (propertyRefExpr);
			var parentMap = cfg.GetClassMapping(typeof(TObject));
			var propertyMap = cfg.GetClassMapping (typeof(TObject)).GetProperty (propName).Value as ManyToOne;
			Type itemType = propertyMap.Type.ReturnedClass;
			var itemMap = cfg.GetClassMapping(itemType);
			var parentTable = parentMap.Table.Name;
			string fieldName = propertyMap.ColumnIterator.First ().Text;
			return new DeleteDependenceInfo{
				IsCascade = true,
				ParentPropertyName = propName,
				ObjectClass = itemType,
				WhereStatment = String.Format("WHERE {0}.id = (SELECT {1} FROM {2} WHERE id = @id)", itemMap.Table.Name, fieldName, parentTable)
			};
		}
	}

}
