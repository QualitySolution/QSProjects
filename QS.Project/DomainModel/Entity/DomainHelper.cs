using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Utilities.Text;

namespace QS.DomainModel.Entity
{
	/// <summary>
	/// Упрощает работу с объектами доменной модели, реализуя простые часто используемые функции.
	/// </summary>
	public static class DomainHelper
	{
		/// <summary>
		/// Метод возвращает название объекта через рефлексию.
		/// Название пытается опытается определить в следующем порядке:
		/// 1. Если у объекта имеется свойство <c>Title</c>, возвращается его значение.
		/// 2. Если у объекта имеется свойство <c>Name</c>, возвращается его значение.
		/// 3. Возвращается результат вызова <c>ToString()</c>
		/// </summary>
		/// <param name="value">Value.</param>
		public static string GetTitle(this object value)
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

		/// <summary>
		/// Метод возвращает <c>Id</c> не типизированого объекта.
		/// Если класс реализует <c>IDomainObject</c>, используется интерфейс для ускорения.
		/// Иначе ищется свойство <c>Id</c>, у каласа и возвращается значение.
		/// Если свойство <c>Id</c> не найдено вызовется эксепшен ArgumentException
		/// </summary>
		public static int GetId(this object value)
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

		public static int? GetIdOrNull(this object value)
		{
			return value != null ? (int?)GetId(value) : null;
		}

		/// <summary>
		/// Метод расширения для <c>IDomainObject</c> позволяющий сравнивнить экземпляр с другим объектом доменной модели по <c>Id</c> и типу.
		/// То есть могут быть загружены из разных сессий.
		/// </summary>
		/// <returns><c>true</c>, если объекты одинаковые, иначе <c>false</c>.</returns>
		public static bool IsSame(this IDomainObject entity1, IDomainObject entity2)
		{
			return EqualDomainObjects(entity1, entity2);
		}

		/// <summary>
		/// Метод сравнивает два объекта доменной модели по типу и <c>Id</c>, они не обязательно должны быть одним и тем же экземпляром класса.
		/// То есть могут быть загружены из разных сессий.
		/// </summary>
		/// <returns><c>true</c>, если объекты одинаковые, иначе <c>false</c>.</returns>
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

		/// <summary>
		/// Метод возвращает все наименования тип сущьности указанные через атрибут <c>AppellativeAttribute</c>
		/// </summary>
		/// <param name="subject">Экземпляр сущьности</param>
		public static AppellativeAttribute GetSubjectNames(this object subject)
		{
			return GetSubjectNames (subject.GetType ());
		}

		/// <summary>
		/// Метод возвращает все наименования тип сущьности указанные через атрибут <c>AppellativeAttribute</c>
		/// </summary>
		/// <param name="subjectType">Тип сущьности</param>
		public static AppellativeAttribute GetSubjectNames(this Type subjectType)
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

		[Obsolete("Нужно в будущем выпилить отсюда этот метод. Во первых они полностью дублирую аналогичные в DialogHelper, во вторых они очень косвенно относятся к доменной модели и в DialogHelper им самое место.")]
		public static string GenerateDialogHashName<TEntity>(int id) where TEntity : IDomainObject
		{
			return GenerateDialogHashName(typeof(TEntity), id);
		}

		[Obsolete("Нужно в будущем выпилить отсюда этот метод. Во первых они полностью дублирую аналогичные в DialogHelper, во вторых они очень косвенно относятся к доменной модели и в DialogHelper им самое место.")]
		public static string GenerateDialogHashName(Type entityType, int id)
		{
			if(!typeof(IDomainObject).IsAssignableFrom(entityType))
				throw new ArgumentException("Тип должен реализовывать интерфейс IDomainObject", nameof(entityType));

			return string.Format("{0}_{1}", entityType.Name, id);
		}

		/// <summary>
		/// Метод перегружает из базы внутренню коллекцию объектов в родительском объекте. Например строки заказа, не трогая сам заказ.
		/// Пример использования: order.ReloadChildCollection(x => x.Items, x.Order, Uow.Sesion);
		/// </summary>
		/// <param name="entity">Объект в котором находится перегружаемая коллекция</param>
		/// <param name="listSelector">Лямбда указывающая на свойство объекта содержащее коллекцию дочерних объектов.</param>
		/// <param name="parentPropertySelector">Лямбда указывающая на свойство дочернего объекта, которое ссылается на родителя.</param>
		/// <param name="session">Session - этим все сказано ;)</param>
		/// <typeparam name="TEntity">Тип родительской сущьности</typeparam>
		/// <typeparam name="TChildEntity">Тип дочерней сущьности</typeparam>
		public static void ReloadChildCollection<TEntity, TChildEntity>(this TEntity entity, Expression<Func<TEntity, IList<TChildEntity>>> listSelector, Expression<Func<TChildEntity, TEntity>> parentPropertySelector, ISession session)
			where TEntity : class, IDomainObject
			where TChildEntity : class, IDomainObject
		{
			var listPropery = PropertyUtil.GetPropertyInfo(listSelector);
			//Забываем про элементы старого списка
			var oldList = listPropery.GetValue(entity) as IList<TChildEntity>;
			oldList.ToList().ForEach(session.Evict);
			//Грузим новый список
			var parentPropertyName = PropertyUtil.GetPropertyInfo(parentPropertySelector).Name;
			var newList = session.QueryOver<TChildEntity>().Where(Restrictions.Eq(parentPropertyName + ".Id", entity.Id)).List();
			oldList.Clear();
			foreach(var item in newList) {
				oldList.Add(item);	
			}
		}
	}
}

