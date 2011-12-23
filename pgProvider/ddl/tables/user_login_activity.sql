-- Requirements: user security exists and owns the current database.

SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;
SET search_path = public, pg_catalog;
SET default_tablespace = '';
SET default_with_oids = false;

CREATE TABLE user_login_activity (
    activity_id integer NOT NULL,
    "when" timestamp with time zone DEFAULT now() NOT NULL,
    "from" character varying(250) NOT NULL,
    success boolean NOT NULL,
    user_name character varying(250) NOT NULL
);

ALTER TABLE public.user_login_activity OWNER TO security;

CREATE SEQUENCE user_login_activity_activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;

ALTER TABLE public.user_login_activity_activity_id_seq OWNER TO security;

ALTER SEQUENCE user_login_activity_activity_id_seq OWNED BY user_login_activity.activity_id;

ALTER TABLE user_login_activity ALTER COLUMN activity_id SET DEFAULT nextval('user_login_activity_activity_id_seq'::regclass);

ALTER TABLE ONLY user_login_activity
    ADD CONSTRAINT pk_user_login_activity PRIMARY KEY (activity_id);

CREATE INDEX ix_user_login_activity_user_name ON user_login_activity USING btree (user_name);