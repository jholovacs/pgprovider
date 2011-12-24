/**************************************************************************************************
Assumptions:
	You installed, have access to, and permissions to create objects on a PostgreSQL 8.4+ database server.
	You have created a user/role account called "security", and a database by that name, which is owned by
		the "security" account.
	You are running this script in that database, which should BE EMPTY.
**************************************************************************************************/

SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = true;
SET client_min_messages = warning;
SET search_path = public, pg_catalog;
SET escape_string_warning = off;
SET default_tablespace = '';
SET default_with_oids = false;

-- See the bottom of this script to change the default owner of the database objects.

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;
COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';

-- Create the users table
DROP TABLE IF EXISTS users;
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
DROP TABLE IF EXISTS roles;
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
DROP TABLE IF EXISTS user_login_activity;
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
DROP TABLE IF EXISTS users_roles;
CREATE TABLE users_roles (
    user_id integer NOT NULL,
    role_id integer NOT NULL
);
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_pkey PRIMARY KEY (user_id, role_id);
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_role_id_fkey FOREIGN KEY (role_id) REFERENCES roles(role_id) ON UPDATE CASCADE ON DELETE CASCADE;
ALTER TABLE ONLY users_roles ADD CONSTRAINT users_roles_user_id_fkey FOREIGN KEY (user_id) REFERENCES users(user_id) ON UPDATE CASCADE ON DELETE CASCADE;

/***************************************************************************************************************
Functions and types
***************************************************************************************************************/
drop type if exists user_record cascade;
create type user_record as(
	user_id int,
	user_name varchar(250),
	last_activity timestamp with time zone,
	created timestamp with time zone,
	email varchar(250),
	approved boolean,
	last_lockout timestamp with time zone,
	last_login timestamp with time zone,
	last_password_changed timestamp with time zone,
	password_question varchar(1000),
	comment text
);

create function get_all_users(
	_application_name varchar(250)) 
	returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		application_name = _application_name
	order by
		user_id asc;

end;
$$ language plpgsql;

create function get_users_by_email(
	partial_email varchar(250),
	_application_name varchar(250)
	)returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		application_name = _application_name 
		and email ilike '%' || partial_email || '%'
	order by
		user_id asc;
end;
$$ language plpgsql;

create function get_users_by_username(
	partial_username varchar(250),
	_application_name varchar(250)
	)returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		application_name = _application_name
		and user_name ilike '%' || partial_username || '%'
	order by
		user_id asc;
end;
$$ language plpgsql;

create function get_users_online(
	_session_timeout integer,
	_appliction_name varchar(250)
	) returns setof user_record as $$
begin

	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		application_name = _application_name
		and last_activity::time + cast(_session_timeout || ' minutes' as interval) < current_timestamp;	
end;
$$ language plpgsql;

create or replace function get_online_count(
	_session_timeout integer,
	_application_name varchar(250)
	) returns integer as $$
begin
	return (
		select 
			count(*) 
		from 
			users
		where 
			application_name = _application_name
			and last_activity::time + cast(_session_timeout + ' minutes' as interval) < current_timestamp);
end;
$$ language plpgsql;

create function get_user_by_username(
	_user_name varchar(250),
	_application_name varchar(250),
	_online boolean
	) returns setof user_record as $$
begin
if _online then
	return query
	update users
	set 
		last_activity = current_timestamp
	where 
		application_name = _application_name
		and user_name = _user_name
	returning
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment;
else
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		application_name = _application_name
		and user_name = _user_name;
end if;
end;
$$ language plpgsql;

create function get_user_by_id(
	_user_id integer,
	_online boolean
	) returns setof user_record as $$
begin
if _online then
	return query
	update users
	set 
		last_activity = current_timestamp
	where 
		user_id = _user_id
	returning
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment;
else
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		user_id = _user_id;
end if;
end;
$$ language plpgsql;

create or replace function assign_users_to_roles(
	_users varchar(250)[],
	_roles varchar(250)[],
	_application_name varchar(250)
	) returns boolean as $$
begin
create temporary table usernames (username varchar(250) not null primary key);
create temporary table rolenames (rolename varchar(250) not null primary key);
-- Create the tables based off the arrays.
insert into usernames
	(
	username
	)
select distinct
	username
from
	unnest(_users) as username;

insert into rolenames
	(
	rolename
	)
select distinct
	rolename
from
	unnest(_roles) as rolename;

-- Per the role provider pattern, an exception is to be thrown if any of the role names or user names specified
-- do not exist for the given application name.
if exists (select null from usernames where not exists (select null from users where application_name = _application_name and user_name = username))
	or exists (select null from roles where not exists (select null from roles where application_name = _application_name and role_name = rolename)) then
	raise exception 'At least one user name or role specified does not exist in the application scope.' using errcode='MSING';
end if;

-- Insert the records linking the users to the roles, excluding pre-existing relationships.
insert into users_roles
	(
	user_id,
	role_id
	)
select distinct
	u.user_id,
	r.role_id
from
	usernames as un
inner join
	users as u on u.application_name = _application_name and u.user_name = un.username
cross join
	rolenames as rn
inner join
	roles as r on r.application_name = _application_name and r.role_name = rn.rolename
where not exists(
	select null
	from users_roles
	where
		user_id = u.user_id
		and role_id = r.role_id
	);

return true;
end;
$$ language plpgsql;

create or replace function create_role(
	_role_name varchar(250),
	_application_name varchar(250),
	_role_description text
	) returns int as $$
declare _role_id integer;
begin
	-- per the role provider pattern, need to throw an exception if the role exists already.
	if exists(select null from roles where application_name = _application_name and role_name = _role_name) then
		raise exception 'The role already exists in this application.' using errcode='DUPRL';
	end if;

	insert into roles
		(
		role_name,
		application_name,
		role_description
		)
	values
		(
		_role_name,
		_application_name,
		_role_description
		)
	returning role_id into _role_id;
	return _role_id;
end;
$$ language plpgsql;

create or replace function create_user(
	_user_name varchar(250),
	_application_name varchar(250),
	_email varchar(250),
	_approved boolean,
	_email_is_unique boolean
	) returns int as $$
	declare userid integer;
	begin
		
	if _email_is_unique
		and exists(select null from users as u where u.email = _email) then
		raise exception 'The email address specified is already in use for this application, and email addresses are configured to be unique.' using errcode='DUPEM';
	end if;

	if exists(select null from users as u where u.application_name = _application_name and u.user_name = _user_name) then
		raise exception 'The user name specified is already in use for this application.' using errcode='DUPUN';
	end if;

	insert into users
		(
		user_name,
		application_name,
		email,
		approved
		)
	values
		(
		_user_name,
		_application_name,
		_email,
		_approved
		)
	returning user_id into userid;
		
	return userid;
	end;
	
$$ language plpgsql;

create or replace function delete_role(
	_role_name varchar(250),
	_application_name varchar(250),
	_throw_on_populated boolean
	) returns boolean as $$
declare _role_id integer;
begin
	if _throw_on_populated and exists(
		select null from roles as r
		inner join users_roles as ur
		on ur.role_id = r.role_id
		where r.application_name = _application_name 
		and r.role_name = _role_name
		) then
		raise exception 'The specified role is populated; cannot delete.' using errcode='RLPOP';
	end if;

	if not exists(select null from roles where application_name = _application_name and role_name = _role_name) then
		raise exception 'The specified role does not exist.' using errcode='NOROL';
	end if;

	delete from roles
	where application_name = _application_name
		and role_name = _role_name;
	return true;
end;
$$ language plpgsql;

create or replace function delete_user(
	_user_name varchar(250),
	_application_name varchar(250),
	_delete_related boolean
	) returns boolean as $$
begin
if _delete_related then
	delete from user_login_activity as ula
	using users as u
	where u.application_name = _application_name
		and u.user_name = _user_name
		and u.user_id = ula.user_id;
end if;
delete from users
where application_name = _application_name 
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;

drop type if exists creds cascade;
create type creds as (
	"salt" varchar(250),
	"password" bytea,
	"password_answer" bytea,
	"answer_salt" varchar(250),
	"last_lockout" timestamp with time zone
	);

create or replace function get_user_credentials(
	_user_name varchar(250),
	_application_name varchar(250)
	) returns setof creds as $$
begin
	return query
	select
		salt,
		password,
		password_answer,
		answer_salt,
		last_lockout
	from
		users
	where
		application_name = _application_name
		and user_name = _user_name
	limit 1;
end;
$$ language plpgsql;

create or replace function get_user_name_by_email(
	_email varchar(250),
	_application_name varchar(250)
	) returns varchar(250) as $$
declare username varchar(250);
begin
	select
		user_name
	from
		users
	where
		application_name = _application_name 
		and email = _email
	limit 1
	into 
		username;
	return username;
end;
$$ language plpgsql;

create or replace function record_login_event(
	_user_name varchar(250),
	_application_name varchar(250),
	_origin varchar(250),
	_success_indicator boolean,
	_attempt_window integer,
	_attempt_count integer
	) returns boolean as $$
declare userid integer;
begin
select
	user_id
from users as u
where u.application_name = _application_name
	and u.user_name = _user_name
limit 1 into userid;

insert into user_login_activity
	(
	"from",
	success,
	"user_id"
	)
values
	(
	_origin,
	_success_indicator,
	userid
	);

-- check for last x attempts if failure
if not _success_indicator and _attempt_count <> 0 then
	if (select count(*) from 
		(select "success" from user_login_activity as ula
			where ula.user_id = userid
			and ula.when > current_timestamp - cast(_attempt_window || ' minutes' as interval)
			order by ula.when desc limit _attempt_count) as last_login_attempts
		where "success" = false) >= _attempt_count then

	update users
	set
		last_lockout = current_timestamp
	where
		user_id = userid;
	end if;
end if;
return true;
end;
$$ language plpgsql;

create or replace function unlock_user(
	_user_name varchar(250),
	_application_name varchar(250)
	) returns boolean as $$
begin	
	update users
	set
		last_lockout = null
	where
		application_name = _application_name
		and user_name = _user_name;

	return true;
end;
$$ language plpgsql;

create or replace function update_user(
	_user_id int,
	_user_name varchar(250),
	_application_name varchar(250),
	_email varchar(250),
	_approved boolean,
	_comment text,
	_email_is_unique boolean
	) returns boolean as $$
begin
if _email_is_unique and exists(
	select null from users where application_name = _application_name
		and email = _email and user_id <> _user_id) then
	raise exception 'The email address specified is already in use by another user.' using errcode='DUPEM';
end if;
update users
set
	user_name = _user_name,
	email = _email,
	approved = _approved,
	comment = _comment
where
	user_id = _user_id;
return true;
end;
$$ language plpgsql;

create or replace function update_user_password(
	_user_name varchar(250),
	_application_name varchar(250),
	_salt varchar(250),
	_password bytea
	) returns boolean as $$
begin
update users
set
	salt = _salt,
	password = _password,
	last_password_changed = current_timestamp
where
	application_name = _application_name
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;

create or replace function update_user_q_and_a(
	_user_name varchar(250),
	_application_name varchar(250),
	_password_question varchar(1000),
	_answer_salt varchar(250),
	_password_answer bytea
	) returns boolean as $$
begin
update users
set
	password_question = _password_question,
	password_answer = _password_answer,
	answer_salt = _answer_salt
where
	application_name = _application_name
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;

/**********************************************************************************************************
Set object owners
**********************************************************************************************************/
update pg_class SET relowner = (SELECT oid FROM pg_roles WHERE rolname = 'security')
where relnamespace = (select oid from pg_namespace where nspname = 'public' limit 1);

update pg_proc set proowner = (select oid from pg_roles where rolname = 'security')
where pronamespace = (select oid from pg_namespace where nspname = 'public' limit 1);