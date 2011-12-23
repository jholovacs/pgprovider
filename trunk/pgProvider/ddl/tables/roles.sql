SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;
SET search_path = public, pg_catalog;
SET default_tablespace = '';
SET default_with_oids = false;

CREATE TABLE roles (
    role_id integer NOT NULL,
    role_name character varying(250) NOT NULL,
    role_description text
);

ALTER TABLE public.roles OWNER TO security;

CREATE SEQUENCE roles_role_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;

ALTER TABLE public.roles_role_id_seq OWNER TO security;

ALTER SEQUENCE roles_role_id_seq OWNED BY roles.role_id;

ALTER TABLE roles ALTER COLUMN role_id SET DEFAULT nextval('roles_role_id_seq'::regclass);

ALTER TABLE ONLY roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (role_id);

ALTER TABLE ONLY roles
    ADD CONSTRAINT roles_role_name_key UNIQUE (role_name);
