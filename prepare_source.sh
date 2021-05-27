#!/bin/bash
echo "Что делаем?"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "4) cleanup bin and obj directories"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

cd "$(dirname "$0")"

case $case in
    *4*)
rm -v -f -R ../GammaBinding/*/bin/*
rm -v -f -R ../QSProjects/*/bin/*
rm -v -f -R ../My-FyiReporting/*/bin/*
rm -v -f -R ../GammaBinding/*/obj/*
rm -v -f -R ../QSProjects/*/obj/*
rm -v -f -R ../My-FyiReporting/*/obj/*
;;&
    *3*)
rm -v -f -R ./packages/*
rm -v -f -R ../My-FyiReporting/packages/*
;;&
    *1*)
cd ../Gtk.DataBindings
git pull --autostash
cd ../GammaBinding
git pull --autostash
cd ../My-FyiReporting
git pull --autostash
cd ../QSProjects
git pull --autostash
;;&
    *2*)
nuget restore ../QSProjects/QSProjectsLib.sln;
nuget restore ../My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
;;&
esac


read -p "Press enter to exit"
