=  Объяснение

Солюшен со старыми библиотеками перенесен в отдельную папку чтобы имелась возможность положить рядом с ним global.json который фиксирует сборку с помощью dotnet SDK не выше 3.1 так как во всех последующих поломана сборка на линуксе если в зависимостях есть фреймворк. Так будет пока не откажемся совсем от библиотек на фреймворке и соответственно GTK#, или пока не откажемся от поддержки сборки на линуксе.

Проводили множество экспериментов в попытке все это запустить на dotnet 8, в результате все время что-то не работало, нормально. Даже если все старые библиотеки сконверить в dotnet, на винде получилось собрать в таком окружении на linux нет.