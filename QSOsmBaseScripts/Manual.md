Для работы скриптов необходимо установить следующее:
```shell
zypper in osm2pgsql perl-DBD-Pg
```

## Установка постгреса
Нужно установить дополниельный пакет:
```shell
    zypper in postgresql10-contrib
```
## Первоначальная установка постреса
1. После установки инициализируем папку с данным, можно просто запустить службу постгреса
2. Для работы osm2pgsql у пользователя с помощью которого продключаемся к базе, не должно быть пароля(окуратнее если к серверу есть доступ с наружи). Редактируем файл $pgdir/pg_hba.conf
```
    # "local" is for Unix domain socket connections only
    local   all             all                                     *trust*
    # IPv4 local connections:
    host    all             all             127.0.0.1/32            *trust*
```
3. Создаем саму базу
```shell
    createdb -U postgres osmgis
    psql -U postgres -d osmgis -c 'CREATE EXTENSION postgis; CREATE EXTENSION hstore;'
```

4. Можно переходить к последовательности в обновлении базы. Начиная с шага 2.

## Последовательность действий для обновления базы ##

1. Перед обновлением базы можно сделать бекап скриптом BackupCurrentBase.sh
2. Запускаем скрипт `CreateBaseOnPostgres.sh`, он автоматически скачает последний дамп данных, и создаст или перезапишет базу на постгресе.
3. Запускаем программу подготовки базы для работы нашего сервера `mono OsmBaseScripts.exe`
4. Выполняем шаги 1 и 2 в программе OsmBaseScripts.
5. Все пользуемся.


