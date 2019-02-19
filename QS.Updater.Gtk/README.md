## Как добавить в проект, обновление программы. ##

1. Добавить в проект QSUpdater
2. Для выполенине проверки в фоновом режиме можно вызвать `CheckUpdate.StartCheckUpdateThread (UpdaterFlags.StartInThread);`
3. Для выполение проверки из пункта меню вызываем `CheckUpdate.StartCheckUpdateThread (UpdaterFlags.ShowAnyway);`
4. Что бы программа предлагала обновиться при подключении к базе с большей версией можно использовать следующией код:
```c#
QSSupportLib.MainSupport.LoadBaseParameters ();
 
if (!QSSupportLib.MainSupport.CheckVersion (this)) {//Проверяем версию базы
	CheckUpdate.StartCheckUpdateThread (UpdaterFlags.ShowAnyway | UpdaterFlags.UpdateRequired);
	return;
}
```
