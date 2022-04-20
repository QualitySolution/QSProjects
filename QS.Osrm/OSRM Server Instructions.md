## Поднимаем собственный сервис маршрутизации проще всего через Docker
1. Создаем каталог с данными можно в папке пользователя.
```shell
mkdir osrm
cd osrm/
```
2. Скачиваем файл региона с geofabrik.de
```shell
wget -N http://download.geofabrik.de/russia/northwestern-fed-district-latest.osm.pbf
```
3. Убедимся что докер установлен и запущен.
```shell
sudo systemctl status docker.service
```
 1. Если не установлен устанавливаем
 ```shell
 sudo zypper in docker
 ```
 2. Запускаем
 ```shell
 sudo systemctl start docker.service
 ```
4. Скачиваем образ машины маршрутизации
```shell
sudo docker pull osrm/osrm-backend
```
5. Подготавиливаем данные в папке
```shell
sudo docker run -t -v /home/admin/osrm:/osrm osrm/osrm-backend osrm-extract -p /opt/car.lua /osrm/northwestern-fed-district-latest.osm.pbf
sudo docker run -t -v /home/admin/osrm:/osrm osrm/osrm-backend osrm-contract /osrm/northwestern-fed-district-latest.osrm
```
6. Запускаем сервис
```shell
sudo docker run --restart=always -d -t -i -p 5000:5000 -v /home/admin/osrm:/osrm osrm/osrm-backend osrm-routed /osrm/northwestern-fed-district-latest.osrm
```
Внимание эта команда сохранит образ как службу и будет запускать его всегда при старте, то есть если выполнить 2 раза, будут пытаться запускаться 2 контейрера, пока их не удалишь, если нужно экспериментировать уберите из команды опции --restart=always, и -d 
