#!/usr/bin/perl

use DBI;
my $dbh = DBI->connect('dbi:Pg:dbname=osmgis;host=localhost','postgres','postgres',{AutoCommit=>1,RaiseError=>1,PrintError=>1});
my $total = 0;

#my $sth_clear_wrong = qq(
#	DELETE FROM planet_osm_polygon 
#	WHERE ((NOT ST_Contains((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Санкт-Петербург'), way)) 
#	AND (NOT ST_Contains((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Ленинградская область'), way))) 
#	AND (admin_level <> '4' OR admin_level IS NULL););

my $sth_clear_wrong = qq(
	DELETE FROM planet_osm_polygon 
	WHERE ((NOT ST_Covers((SELECT way FROM planet_osm_polygon WHERE name = 'Санкт-Петербург и Ленинградская область'), way))) 
	AND (admin_level <> '4' OR admin_level IS NULL););


my $rv = $dbh->do($sth_clear_wrong) or die $DBI::errstr;
if( $rv < 0 ){
	print $DBI::errstr;
} else {
	print "Total number of rows deleted : $rv\n";
}

$dbh->disconnect();
