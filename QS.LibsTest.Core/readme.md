= Тесты с использованием контейнеров

Ряд тестов в QS.LibsTest.Core используют контейнеры Docker, например для развертывания базы данных.

== Установка на Linux

Первоначально нужно установить докер.

=== Для openSUSE

  zypper install docker

  systemctl enable docker
  systemctl start docker

Далее нужно добавить своего пользователя в группу docker, чтобы не запускать докер от root.

  usermod -aG docker $USER
  
