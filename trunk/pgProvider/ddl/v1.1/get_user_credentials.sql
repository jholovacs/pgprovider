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
