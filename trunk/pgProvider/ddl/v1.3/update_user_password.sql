CREATE OR REPLACE FUNCTION update_user_password(_user_name character varying, _application_name character varying, _salt character varying, _password bytea) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
	begin
update users
set
	salt = _salt,
	password = _password,
	last_password_changed = current_timestamp
where
	lower(application_name) = lower(_application_name)
	and lower(user_name) = lower(_user_name);
return true;
end;
$$;