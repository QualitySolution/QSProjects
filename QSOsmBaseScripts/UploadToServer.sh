#!/bin/bash
echo "Which server?"
echo "1) 192.168.0.16"
echo "2) osm.vod.qsolution.ru"
read case;

case $case in
    1)
SERVER="192.168.0.16"
;;
    2)
SERVER="osm.vod.qsolution.ru"
;;
esac

echo "Which build to upload?"
echo "1) Release"
echo "2) Debug"
read case;

case $case in
    1)
BUILD="Release"
;;
    2)
BUILD="Debug"
;;
esac

msbuild /p:Configuration=$BUILD /p:Platform=x86

rsync -vizaP ./bin/$BUILD/ admin@$SERVER:/home/admin/OsmBaseScripts
