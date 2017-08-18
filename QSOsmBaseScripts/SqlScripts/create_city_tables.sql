-------Удаляем старые таблицы--------
DROP TABLE IF EXISTS osm_city_ways; 

---------------------------------------osm_cities-------------------------------
DROP TABLE IF EXISTS osm_cities;

CREATE TABLE osm_cities
(
	id bigint NOT NULL,
	name text NOT NULL,
	place text NOT NULL,
	suburb_district_id bigint,
	CONSTRAINT osm_cities_pkey PRIMARY KEY (id)
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_cities
	OWNER TO postgres;
GRANT ALL ON TABLE osm_cities TO postgres;
GRANT SELECT ON TABLE osm_cities TO public;

---------------------------------------osm_city_ways-------------------------------

CREATE TABLE osm_city_ways
(
	id SERIAL NOT NULL,
	city_id bigint NOT NULL,
	way geometry(Geometry,3857) NOT NULL,
	CONSTRAINT osm_city_ways_pkey PRIMARY KEY (id),
	CONSTRAINT osm_city_fk FOREIGN KEY (city_id)
      REFERENCES osm_cities (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_city_ways
	OWNER TO postgres;
GRANT ALL ON TABLE osm_city_ways TO postgres;
GRANT SELECT ON TABLE osm_city_ways TO public;

---------------------------------------osm_city_districts-------------------------------
DROP TABLE IF EXISTS osm_city_districts;

CREATE TABLE osm_city_districts
(
	id bigint NOT NULL,
	name text NOT NULL,
	way geometry(Geometry,3857) NOT NULL,
	CONSTRAINT osm_city_districts_pkey PRIMARY KEY (id)
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_city_districts
	OWNER TO postgres;
GRANT ALL ON TABLE osm_city_districts TO postgres;
GRANT SELECT ON TABLE osm_city_districts TO public;

---------------------------------------osm_suburb_districts-------------------------------
DROP TABLE IF EXISTS osm_suburb_districts;

CREATE TABLE osm_suburb_districts
(
	id bigint NOT NULL,
	name text NOT NULL,
	way geometry(Geometry,3857) NOT NULL,
	CONSTRAINT osm_suburb_districts_pkey PRIMARY KEY (id)
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_suburb_districts
	OWNER TO postgres;
GRANT ALL ON TABLE osm_suburb_districts TO postgres;
GRANT SELECT ON TABLE osm_suburb_districts TO public;