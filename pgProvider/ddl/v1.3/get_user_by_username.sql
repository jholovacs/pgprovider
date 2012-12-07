CREATE OR REPLACE FUNCTION get_user_by_username(_user_name character varying, _application_name character varying, _online boolean) RETURNS SETOF user_record
    LANGUAGE plpgsql
    AS $$begin
if _online then
	return query
	update users
	set 
		last_activity = current_timestamp
	where 
		lower(application_name) = lower(_application_name)
		and lower(user_name) = lower(_user_name)
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
		lower(application_name) = lower(_application_name)
		and lower(user_name) = lower(_user_name);
end if;
end;
$$;