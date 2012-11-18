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
