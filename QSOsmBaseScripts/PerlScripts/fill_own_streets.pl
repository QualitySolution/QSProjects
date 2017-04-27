#!/usr/bin/perl

use DBI;
my $dbh = DBI->connect('dbi:Pg:dbname=osmgis;host=localhost','postgres','postgres',{AutoCommit=>1,RaiseError=>1,PrintError=>1});
my $total = 0;

############################ Создание записей в temp_osm_streets ##########################################

my $sth_get_streets = $dbh->prepare("SELECT tags->'addr:street' AS name, tags->'addr:district' AS district, tags->'addr:city' AS city
	FROM planet_osm_polygon 
	GROUP BY tags->'addr:district', tags->'addr:street', tags->'addr:city' 
	ORDER BY tags->'addr:street';");
my $sth_fill_streets = $dbh->prepare("INSERT INTO temp_osm_streets(id, name, district, city) VALUES (?, ?, ?, ?);");
my $i = 1;
$sth_get_streets->execute();
while (my ($name, $district, $city) = $sth_get_streets->fetchrow_array())
{       
        $sth_fill_streets->execute($i, $name, $district, $city);
        $total += $sth_fill_streets->rows;
	$i += 1;
        print "Копирование улиц в отдельную таблицу: $total\r";
}
print "Копирование улиц в отдельную таблицу: $total\n";
$sth_get_streets->finish();
$sth_fill_streets->finish();

############################ Удаление неправильных улиц из города ####################################

my $sth_remove_wrong_streets = qq(DELETE FROM temp_osm_streets WHERE (district IS NULL OR district = '' OR name IS NULL OR name = '') AND city = 'Санкт-Петербург';);
my $rv = $dbh->do($sth_remove_wrong_streets) or die $DBI::errstr;
if( $rv < 0 ){
	print $DBI::errstr;
}else{
	print "Удаление некорректных улиц. Всего удалено $rv записей\n";
}

$dbh->disconnect();
