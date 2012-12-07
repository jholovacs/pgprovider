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

drop type if exists creds cascade;
create type creds as (
	"salt" varchar(250),
	"password" bytea,
	"password_answer" bytea,
	"answer_salt" varchar(250),
	"last_lockout" timestamp with time zone
	);

drop type if exists profile_info cascade;
create type profile_info as(
	property_name varchar(250),
	property_type varchar(250),
	property_value text
	);
