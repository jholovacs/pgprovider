create or replace function get_user_password(
	username varchar(250)
	) returns bytea as $$
declare r bytea;
begin

	return query
	select
		password
	from
		users
	where
		user_name = username
	limit 1
	into r;

	return r;
end;
$$ language plpgsql;