---------------------------------------temp_osm_streets-------------------------------
DROP TABLE IF EXISTS temp_osm_streets;
DROP SEQUENCE IF EXISTS temp_osm_street_id_seq;

CREATE SEQUENCE temp_osm_street_id_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 1
  CACHE 1;
ALTER TABLE temp_osm_street_id_seq
  OWNER TO postgres;


CREATE TABLE temp_osm_streets
(
  id bigint NOT NULL DEFAULT nextval('temp_osm_street_id_seq'::regclass),
  name text,
  district text,
  city text,
  CONSTRAINT temp_osm_streets_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE temp_osm_streets
  OWNER TO postgres;
GRANT ALL ON TABLE temp_osm_streets TO postgres;
GRANT SELECT ON TABLE temp_osm_streets TO public;

---------------------------------------osm_streets-------------------------------
DROP TABLE IF EXISTS osm_streets;

CREATE TABLE osm_streets
(
	id bigint NOT NULL,
	name text,
	city_id bigint NOT NULL,
	CONSTRAINT osm_streets_pkey PRIMARY KEY (id)
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_streets
	OWNER TO postgres;
GRANT ALL ON TABLE osm_streets TO postgres;
GRANT SELECT ON TABLE osm_streets TO public;

---------------------------------------osm_streets_to_districts-------------------------------
DROP TABLE IF EXISTS osm_streets_to_districts;

CREATE TABLE osm_streets_to_districts
(
	street_id bigint NOT NULL,
	district_id bigint NOT NULL
)
WITH (
	OIDS=FALSE
);
ALTER TABLE osm_streets_to_districts
	OWNER TO postgres;
GRANT ALL ON TABLE osm_streets_to_districts TO postgres;
GRANT SELECT ON TABLE osm_streets_to_districts TO public;