#!/bin/bash
cd "$(dirname "$0")"

wget -N http://download.geofabrik.de/russia/northwestern-fed-district-latest.osm.pbf

osm2pgsql --create --number-processes 4 --cache 20000 -d osmgis -U postgres -j northwestern-fed-district-latest.osm.pbf
