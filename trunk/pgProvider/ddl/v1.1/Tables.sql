-- Create the users table
DROP TABLE IF EXISTS users CASCADE;
CREATE TABLE users (
    user_id integer NOT NULL,
    user_name character varying(250) NOT NULL,
	application_name character varying(250) NOT NULL,
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
CREATE SEQUENCE users_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;
ALTER SEQUENCE users_user_id_seq OWNED BY users.user_id;
ALTER TABLE users ALTER COLUMN user_id SET DEFAULT nextval('users_user_id_seq'::regclass);
ALTER TABLE ONLY users ADD CONSTRAINT pk_users PRIMARY KEY (user_id);
CREATE INDEX ix_users_email ON users USING btree (application_name, email);
CREATE INDEX ix_users_last_activity ON users USING btree (application_name, last_activity);
CREATE UNIQUE INDEX ux_users_user_name_application_name ON users USING btree (application_name, user_name);

-- Create the roles table
DROP TABLE IF EXISTS roles CASCADE;
CREATE TABLE roles (
    role_id integer NOT NULL,
    role_name character varying(250) NOT NULL,
	application_name character varying(250) NOT NULL,
    role_description text NULL
);
CREATE SEQUENCE roles_role_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;
ALTER SEQUENCE roles_role_id_seq OWNED BY roles.role_id;
ALTER TABLE roles ALTER COLUMN role_id SET DEFAULT nextval('roles_role_id_seq'::regclass);
ALTER TABLE ONLY roles ADD CONSTRAINT roles_pkey PRIMARY KEY (role_id);
CREATE UNIQUE INDEX ux_roles_role_name_application_name ON roles USING btree (role_name, application_name);

-- Create the user_login_activity table
DROP TABLE IF EXISTS user_login_activity CASCADE;
CREATE TABLE user_login_activity (
    activity_id integer NOT NULL,
    "when" timestamp with time zone DEFAULT now() NOT NULL,
    "from" character varying(250) NOT NULL,
    success boolean NOT NULL,
    user_id integer NOT NULL
);
CREATE SEQUENCE user_login_activity_activity_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;
ALTER SEQUENCE user_login_activity_activity_id_seq OWNED BY user_login_activity.activity_id;
ALTER TABLE user_login_activity ALTER COLUMN activity_id SET DEFAULT nextval('user_login_activity_activity_id_seq'::regclass);
ALTER TABLE ONLY user_login_activity ADD CONSTRAINT pk_user_login_activity PRIMARY KEY (activity_id);
CREATE INDEX ix_user_login_activity_user_id ON user_login_activity USING btree (user_id, "when");

-- Create the users_roles table
DROP TABLE IF EXISTS users_roles CASCADE;
CREATE TABLE users_roles (
    user_id integer NOT NULL,
    role_id integer NOT NULL
);
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_pkey PRIMARY KEY (user_id, role_id);
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_role_id_fkey FOREIGN KEY (role_id) REFERENCES roles(role_id) ON UPDATE CASCADE ON DELETE CASCADE;
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_user_id_fkey FOREIGN KEY (user_id) REFERENCES users(user_id) ON UPDATE CASCADE ON DELETE CASCADE;

DROP TABLE IF EXISTS profiles CASCADE;
