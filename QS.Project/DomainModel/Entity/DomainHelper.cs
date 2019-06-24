using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Utilities.Text;

namespace QS.DomainModel.Entity
{
	public static class DomainHelper
	{
		public static string GetObjectTilte(object value)
		{
			var prop = value.GetType ().GetProperty ("Title");
			if (prop != null) {
				return prop.GetValue (value, null)?.ToString();
			}

			prop = value.GetType ().GetProperty ("Name");
			if (prop != null) {
				return prop.GetValue (value, null)?.ToString();
			}

			return value.ToString ();
		}

		public static int GetId(object value)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value is IDomainObject)
				return (value as IDomainObject).Id;

			var prop = value.GetType ().GetProperty ("Id");
			if (prop == null)
				throw new ArgumentException ($"Для работы метода, тип {value.GetType()} должен иметь свойство Id.");

			return (int)prop.GetValue (value, null);
		}

		public static int? GetIdOrNull(object value)
		{
			return value != null ? (int?)GetId(value) : null;
		}

		public static bool IsSame(this IDomainObject entity1, IDomainObject entity2)
		{
			return EqualDomainObjects(entity1, entity2);
		}

		public static bool EqualDomainObjects (object obj1, object obj2)
		{
			if (obj1 == null || obj2 == null)
				return false;

			if (NHibernateUtil.GetClass (obj1) != NHibernateUtil.GetClass (obj2))
				return false;

			if (obj1 is IDomainObject)
				return (obj1 as IDomainObject).Id == (obj2 as IDomainObject).Id;

			return obj1.Equals (obj2);
		}

		public static AppellativeAttribute GetSubjectNames(object subject)
		{
			return GetSubjectNames (subject.GetType ());
		}

		public static AppellativeAttribute GetSubjectNames(Type subjectType)
		{
			object[] att = subjectType.GetCustomAttributes (typeof(AppellativeAttribute), true);
			if (att.Length > 0) {
				return (att [0] as AppellativeAttribute);
			} else
				return null;
		}

		public static string GetSubjectName(this Type subjectType)
		{
			var atts = subjectType.GetCustomAttributes<AppellativeAttribute>(true);
			var attribute = atts.FirstOrDefault();
			if(attribute != null && !string.IsNullOrWhiteSpace(attribute.Nominative)) {
				return attribute.Nominative.StringToTitleCase();
			} else
				return subjectType.Name;
		}

		public static IEnumerable<Type> GetHavingAttributeEntityTypes<TAttribute>(params Assembly[] assemblies)
			where TAttribute : Attribute
		{
			return GetHavingAttributeEntityTypes<TAttribute>(null, assemblies);
		}

		public static IEnumerable<Type> GetHavingAttributeEntityTypes<TAttribute>(Func<Type, bool> filterFunc, params Assembly[] assemblies)
			where TAttribute : Attribute
		{
			var result = new List<Type>();
			foreach(var assembly in assemblies) {
				IEnumerable<Type> foundTypes = assembly.GetTypes();
				if(filterFunc != null) {
					foundTypes = foundTypes.Where(filterFunc);
				}
				foundTypes = foundTypes.Where(x => x.GetCustomAttribute<TAttribute>() != null);
				result.AddRange(foundTypes);
			}
			return result;
		}

		public static string GetPropertyTitle<TEntity> (System.Linq.Expressions.Expression<Func<TEntity, object>> propertyRefExpr)
		{
			var propInfo = Gamma.Utilities.PropertyUtil.GetPropertyInfo (propertyRefExpr);
			var att = propInfo.GetCustomAttributes (typeof(DisplayAttribute), true);

			return att.Length > 0 ? (att [0] as DisplayAttribute).GetName () : null;
		}

		public static string GetPropertyTitle (Type clazz, string propName)
		{
			var propInfo = clazz.GetProperty (propName);
			if(propInfo == null)
				return propName;
			var att = propInfo.GetCustomAttributes (typeof(DisplayAttribute), true);

			return att.Any() ? (att [0] as DisplayAttribute).GetName () : null;
		}

		/// <summary>
		/// Функция заполняет свойство DTO сущностью
		/// </summary>
		public static void FillPropertyByEntity<TDTO, TProperty>(IUnitOfWork uow, IList<TDTO> listDto, Func<TDTO, int> idSelector, Action<TDTO, TProperty> propertySetter)
			where TProperty : class, IDomainObject
		{
			var entities = uow.GetById<TProperty>(
				listDto.Select(idSelector).Distinct().ToArray());
			foreach(var item in listDto)
			{
				var entity = entities.FirstOrDefault(x => x.Id == idSelector(item));
				if (entity != null)
					propertySetter(item, entity);
			}
		}

		public static string GenerateDialogHashName<TEntity>(int id) where TEntity : IDomainObject
		{
			return GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateDialogHashName(Type entityType, int id)
		{
			if(!typeof(IDomainObject).IsAssignableFrom(entityType))
				throw new ArgumentException("Тип должен реализовывать интерфейс IDomainObject", nameof(entityType));

			return string.Format("{0}_{1}", entityType.Name, id);
		}
	}
}

