#!/usr/bin/perl

use DBI;
use Encode;
my $dbh = DBI->connect('dbi:Pg:dbname=osmgis;host=localhost','postgres', undef,{AutoCommit=>1,RaiseError=>1,PrintError=>1});
my $total = 0;

################################## Setting cities for buildings #############################################
my $sth_polygon = $dbh->prepare("UPDATE planet_osm_polygon
	SET tags = tags || hstore('addr:city', ?)
	WHERE (building IS NOT NULL OR amenity IS NOT NULL)
	AND ((tags->'addr:city') = '' OR (tags->'addr:city') IS NULL)
	AND ST_Contains(?, way)");
my $sth_point = $dbh->prepare("UPDATE planet_osm_point
	SET tags = tags || hstore('addr:city', ?)
	WHERE (building IS NOT NULL OR amenity IS NOT NULL)
	AND ((tags->'addr:city') = '' OR (tags->'addr:city') IS NULL)
	AND ST_Contains(?, way)");
my $sth_city = $dbh->prepare("SELECT name,
	way
	FROM planet_osm_polygon
	WHERE place IN ('city', 'town', 'village', 'hamlet', 'farm', 'allotments', 'isolated_dwelling')
	AND name IS NOT NULL
	AND name <> ''");
$sth_city->execute();
while (my ($name, $wkt) = $sth_city->fetchrow_array())
{
	$sth_polygon->execute($name, $wkt);
	$sth_point->execute($name, $wkt);
	$total+=$sth_point->rows;
	print "Простановка населённых пунктов на зданиях. Исправлено зданий: $total\r";
}
$sth_city->finish();
$sth_point->finish();
$sth_polygon->finish();
print "Простановка населённых пунктов на зданиях. Исправлено зданий: $total\n";

################################## Setting city districts for buildings #############################################

my $sth_district = $dbh->prepare("SELECT name, way 
	FROM planet_osm_polygon 
	WHERE admin_level = '5' AND boundary = 'administrative';");
my $sth_polygon_district = $dbh->prepare("UPDATE planet_osm_polygon
	SET tags = tags || hstore('addr:district', ?)
	WHERE (building IS NOT NULL OR amenity IS NOT NULL)
	AND ((tags->'addr:district') = '' OR (tags->'addr:district') IS NULL)
	AND ST_Contains(?, way)");
my $sth_point_district = $dbh->prepare("UPDATE planet_osm_point
	SET tags = tags || hstore('addr:district', ?)
	WHERE (building IS NOT NULL OR amenity IS NOT NULL)
	AND ((tags->'addr:district') = '' OR (tags->'addr:district') IS NULL)
	AND ST_Contains(?, way)");
$total = 0;
$sth_district->execute();
while (my ($name, $wkt) = $sth_district->fetchrow_array())
{
	$sth_polygon_district->execute($name, $wkt);
	$sth_point_district->execute($name, $wkt);
	$total+=$sth_polygon_district->rows;
	$total+=$sth_point_district->rows;
	print "Простановка районов на зданиях. Всего исправлено зданий: $total\r";
}
$sth_district->finish();
$sth_point_district->finish();
$sth_polygon_district->finish();
print "Простановка районов на зданиях. Всего исправлено зданий: $total\n";

################################## Setting suburb districts for buildings #############################################

my $sth_suburb_district = $dbh->prepare("SELECT name, way 
	FROM planet_osm_polygon 
	WHERE admin_level = '6' AND boundary = 'administrative';");
my $sth_city_for_district = $dbh->prepare("UPDATE planet_osm_polygon
	SET tags = tags || hstore('addr:suburbdistrict', ?)
	WHERE place IN ('city', 'town', 'village', 'hamlet', 'farm', 'allotments', 'isolated_dwelling')
	AND ((tags->'addr:suburbdistrict') = '' OR (tags->'addr:suburbdistrict') IS NULL)
	AND name IS NOT NULL
	AND name <> ''
	AND ST_Contains(?, way)");
$total = 0;
$sth_suburb_district->execute();
while (my ($name, $wkt) = $sth_suburb_district->fetchrow_array())
{
	$sth_city_for_district->execute($name, $wkt);
	$total+=$sth_city_for_district->rows;
	print "Простановка районов на населенных пунктах. Всего обработано населенных пунктов: $total\r";
}
$sth_suburb_district->finish();
$sth_city_for_district->finish();
print "Простановка районов на населенных пунктах. Всего исправлено населенных пунктов: $total\n";

## Обрабатываем населенные пункты которые выходят за границу района. 
## Как работает: вычисляем полигон которые попадает и в район и в город. Потом считаем площадь каждого из попаданий.
## Для населенного пункта проставляем тот район в который населенный пункт попадает большей площадью.
##
my $sth_city_for_district = $dbh->prepare("UPDATE planet_osm_polygon
	SET tags = tags || hstore('addr:suburbdistrict', 
	(	
		SELECT suburb.name 
		FROM planet_osm_polygon as suburb
		WHERE suburb.admin_level = '6' AND suburb.boundary = 'administrative' 
		AND ST_Area(ST_Intersection(suburb.way, planet_osm_polygon.way)) > 0
		ORDER BY ST_Area(ST_Intersection(suburb.way, planet_osm_polygon.way)) DESC
		LIMIT 1
	))
	WHERE place IN ('city', 'town', 'village', 'hamlet', 'farm', 'allotments', 'isolated_dwelling')
	AND ((tags->'addr:suburbdistrict') = '' OR (tags->'addr:suburbdistrict') IS NULL)
	AND name IS NOT NULL
	AND name <> ''");

$sth_city_for_district->execute();
$total=$sth_city_for_district->rows;
print "Простановка районов на населенных пунктах не полностью включенных в границу района. Обработано: $total\n";
$sth_city_for_district->finish();

# Оставлено для дебага
# my $sth_city_without_district = $dbh->prepare("SELECT name FROM planet_osm_polygon
#	WHERE place IN ('city', 'town', 'village', 'hamlet', 'farm', 'allotments', 'isolated_dwelling')
#	AND ((tags->'addr:suburbdistrict') = '' OR (tags->'addr:suburbdistrict') IS NULL)
#	AND name IS NOT NULL
#	AND name <> ''");

#$sth_city_without_district->execute();
#if($sth_city_without_district->rows > 0)
#{
#	print "Населенные пункты все равно оставшиеся без районов:\n";
#	while (my ($name) = $sth_city_without_district->fetchrow_array())
#	{
#		print encode('utf8', $name);
#		print "\n";
#	}
#}
################################## Deleting all points from wrong regions #############################################

#my $sth_clear_wrong = qq(DELETE FROM planet_osm_polygon WHERE ((NOT ST_ContainsProperly((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Санкт-Петербург'), way)) OR (NOT ST_ContainsProperly((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Ленинградская область'), way))) AND admin_level <> '4';);

#my $rv = $dbh->do($sth_clear_wrong) or die $DBI::errstr;
#if( $rv < 0 ){
#   print $DBI::errstr;
#}else{
#   print "Total number of rows deleted : $rv\n";
#}




$dbh->disconnect();
