create or replace function get_user_by_id(
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