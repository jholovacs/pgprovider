drop type if exists creds cascade;
create type creds as (
	"salt" varchar(250),
	"password" bytea,
	"password_answer" bytea,
	"answer_salt" varchar(250),
	"last_lockout" timestamp with time zone
	);

create or replace function get_user_credentials(
	username varchar(250)
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
		user_name = username
	limit 1;

end;
$$ language plpgsql;