create or replace function update_user_password(
	_user_name varchar(250),
	_application_name varchar(250),
	_salt varchar(250),
	_password bytea
	) returns boolean as $$
begin
update users
set
	salt = _salt,
	password = _password,
	last_password_changed = current_timestamp
where
	application_name = _application_name
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;
