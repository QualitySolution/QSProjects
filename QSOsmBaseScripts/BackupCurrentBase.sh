#!/bin/bash

pg_dump -U postgres osmgis > ../osm_backups/osmgis--$(date +%y%m%d).sql
