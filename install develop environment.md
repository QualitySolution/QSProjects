# Подсказки по установке и подготовке окружение разработчика

1. Установка Monodevelop

### На openSUSE можно использовать репозитории CentOS:
```
sudo zypper ar https://download.mono-project.com/repo/centos8-vs.repo
sudo zypper ref
sudo zypper in monodevelop
```

2. Нужен git
   
### Для openSUSE:
```
sudo zypper in git
```

3. Чтобы Nuget смог скачивать пакеты нужно добавить сертификаты
   
### openSUSE:
```
cert-sync --user /etc/ssl/ca-bundle.pem
```
