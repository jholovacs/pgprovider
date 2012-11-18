create or replace function get_user_by_username(
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
