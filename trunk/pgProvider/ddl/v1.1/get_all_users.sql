create or replace function get_all_users(
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