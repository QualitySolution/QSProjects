# Проблемы
Если после сборки библиотеки под Net Core библиотека не компилируется и вываливается ошибка сборки:
/usr/lib/mono/xbuild/Microsoft/NuGet/Microsoft.NuGet.targets(5,5): Error: Your project does not reference ".NETFramework,Version=v4.0" framework. Add a reference to ".NETFramework,Version=v4.0" in the "TargetFrameworks" property of your project file and then re-run NuGet restore. (QS.Utilities)

Нужно удалить каталог /obj
