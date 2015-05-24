using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate.Mapping;

namespace QSOrmProject.Deletion
{
	public static partial class DeleteConfig
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static List<Type> IgnoreMissingClass = new List<Type> ();

		public static void DeletionCheck()
		{
			logger.Info("Проверка правил удаления по информации NHibernate.");
			foreach(var mapping in OrmMain.ormConfig.ClassMappings)
			{
				var info = ClassInfos.Find(i => i.ObjectClass == mapping.MappedClass);

				if(info == null	&& !IgnoreMissingClass.Contains(mapping.MappedClass))
				{
					logger.Warn("#Класс {0} настроен в мапинге NHibernate, но для него отсутствуют правила удаления.",
						mapping.MappedClass
					);
					continue;
				}

				logger.Info("Проверка зависимостей удаления в {0}", mapping.MappedClass);

				foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation))
				{
					var propType = prop.Type.ReturnedClass;
					var relatedClassInfo = ClassInfos.Find(c => c.ObjectClass == propType);
					if(relatedClassInfo == null)
					{
						logger.Warn("#Класс {0} не имеет правил удаления, но свойство {1}.{2} прописано в мапинге.",
							prop.Type.ReturnedClass.Name,
							info.ObjectClass.Name,
							prop.Name);
						continue;
					}

					if(!relatedClassInfo.DeleteItems.Exists(r => r.PropertyName == prop.Name) 
						&& !relatedClassInfo.ClearItems.Exists(r => r.PropertyName == prop.Name))
					{
						logger.Warn("#Для свойства {0}.{1} не определены зависимости удаления в классе {2}",
							info.ObjectClass.Name,
							prop.Name,
							relatedClassInfo.ObjectClass.Name
						);
						continue;
					}
				}

                foreach(var prop in mapping.PropertyIterator.Where(p => p.Type.IsCollectionType))
                {
                    if (!(prop.Value is Bag))
                        continue;
                    var collectionMap = (prop.Value as Bag);
                    Type itemType = (collectionMap.Element as OneToMany).AssociatedClass.MappedClass;
                    var classInfo = ClassInfos.Find(c => c.ObjectClass == itemType);
                    if(classInfo == null)
                    {
                        logger.Warn("#Класс {0} не имеет правил удаления, но используется в коллекции {1}.{2}.",
                            itemType,
                            mapping.MappedClass.Name,
                            prop.Name);
                        continue;
                    }

                    if(!classInfo.DeleteItems.Exists(r => r.PropertyName == prop.Name) 
                        && !classInfo.ClearItems.Exists(r => r.PropertyName == prop.Name))
                    {
                        logger.Warn("#Для коллекции {0}.{1} не определены зависимости удаления класса {2}",
                            mapping.MappedClass.Name,
                            prop.Name,
                            itemType.Name
                        );
                        continue;
                    }
                }

			}

            //Проверяем что все прописанные в зависимостях свойства имеют тип удаляемого объекта.
            foreach(var info in ClassInfos)
            {
                info.DeleteItems.FindAll(d => !String.IsNullOrEmpty(d.PropertyName) && d.ObjectClass.GetProperty(d.PropertyName).PropertyType != info.ObjectClass)
                    .ForEach(d => logger.Warn("#Свойство {0}.{1} определенное в зависимости удаления для класса {2}, имеет другой тип.",
                        d.ObjectClass.Name,
                        d.PropertyName,
                        info.ObjectClass
                    ));
                info.ClearItems.FindAll(d => !String.IsNullOrEmpty(d.PropertyName) && d.ObjectClass.GetProperty(d.PropertyName).PropertyType != info.ObjectClass)
                    .ForEach(d => logger.Warn("#Свойство {0}.{1} определенное в зависимости очистки для класса {2}, имеет другой тип.",
                        d.ObjectClass.Name,
                        d.PropertyName,
                        info.ObjectClass
                    ));
            }

			logger.Info("Проверка настроек удаления завершена.");
		}
	}
}

