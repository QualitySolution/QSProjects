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

echo "Which version to upload?"
echo "1) Release"
echo "2) Debug"
read case;

# ssh root@saas.qsolution.ru "systemctl stop qsserver"

case $case in
    1)
rsync -vizaP ./bin/Release/ admin@$SERVER:/home/admin/OsmBaseScripts
;;
    2)
rsync -vizaP ./bin/Debug/ admin@$SERVER:/home/admin/OsmBaseScripts
;;
esac

# ssh  root@saas.qsolution.ru "sudo systemctl start qsserver"
