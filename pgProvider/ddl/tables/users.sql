-- Required: user 'security' owning the current database.

SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;
SET search_path = public, pg_catalog;
SET default_tablespace = '';
SET default_with_oids = false;

CREATE TABLE users (
    user_id integer NOT NULL,
    user_name character varying(250) NOT NULL,
    last_activity timestamp with time zone,
    created timestamp with time zone DEFAULT now() NOT NULL,
    email character varying(250),
    salt character varying(250),
    password bytea,
    approved boolean DEFAULT true NOT NULL,
    last_lockout timestamp with time zone,
    last_login timestamp with time zone,
    last_password_changed timestamp with time zone DEFAULT now() NOT NULL,
    password_question character varying(1000),
    password_answer bytea,
    answer_salt character varying(250),
    comment text
);

ALTER TABLE public.users OWNER TO security;

COMMENT ON COLUMN users.approved IS 'approved=enabled.  If an account is not approved, it cannot be authenticated against.';

CREATE SEQUENCE users_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;

ALTER TABLE public.users_user_id_seq OWNER TO security;

ALTER SEQUENCE users_user_id_seq OWNED BY users.user_id;

ALTER TABLE users ALTER COLUMN user_id SET DEFAULT nextval('users_user_id_seq'::regclass);

ALTER TABLE ONLY users
    ADD CONSTRAINT pk_users PRIMARY KEY (user_id);

ALTER TABLE ONLY users
    ADD CONSTRAINT ux_users_user_name UNIQUE (user_name);

CREATE INDEX ix_users_email ON users USING btree (email);
