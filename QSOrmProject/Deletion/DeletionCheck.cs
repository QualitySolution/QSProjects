using System;
using System.Linq;

namespace QSOrmProject.Deletion
{
	public static partial class DeleteConfig
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static void DeletionCheck()
		{
			foreach(var info in ClassInfos)
			{
				logger.Info("Проверка зависимостей удаления в {0}", info.ObjectClass);
				var classMap = OrmMain.ormConfig.GetClassMapping(info.ObjectClass);
				if(classMap == null)
				{
					logger.Warn("#Мапинг для класса {0} не найден.",
						info.ObjectClass);
					continue;
				}

				foreach(var prop in classMap.PropertyIterator.Where(p => p.IsEntityRelation))
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
			}
				
			foreach(var mapping in OrmMain.ormConfig.ClassMappings)
			{
				if(!ClassInfos.Exists(i => i.ObjectClass == mapping.MappedClass))
				{
					logger.Warn("#Класс {0} настроен в мапинге NHibernate, но для него отсутствуют правила удаления.",
						mapping.MappedClass
					);
				}
			}

			logger.Info("Проверка настроек удаления завершена.");
		}
	}
}

