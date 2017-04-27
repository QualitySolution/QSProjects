#!/bin/bash
echo "Which version to upload?"
echo "1) Release"
echo "2) Debug"
read case;

# ssh root@saas.qsolution.ru "systemctl stop qsserver"

case $case in
    1)
rsync -vizaP ./bin/Release/ admin@saas.qsolution.ru:/home/admin/OsmBaseScripts
;;
    2)
rsync -vizaP ./bin/Debug/ admin@saas.qsolution.ru:/home/admin/OsmBaseScripts
;;
esac

# ssh  root@saas.qsolution.ru "sudo systemctl start qsserver"
