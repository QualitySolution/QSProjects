#!/bin/bash

wget -N http://data.gis-lab.info/osm_dump/dump/latest/RU-LEN.osm.pbf

osm2pgsql --create --number-processes 4 --cache 20000 -d osmgis -U postgres -j RU-LEN.osm.pbf
