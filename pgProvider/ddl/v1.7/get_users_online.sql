create or replace function get_users_online(
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
		application_name = _appliction_name
		and last_activity::timestamp with time zone + cast(_session_timeout || ' minutes' as interval) > current_timestamp;	
end;
$$ language plpgsql;
