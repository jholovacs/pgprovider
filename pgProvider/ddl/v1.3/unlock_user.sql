CREATE OR REPLACE FUNCTION unlock_user(_user_name character varying, _application_name character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
	begin	
	update users
	set
		last_lockout = null
	where
		lower(application_name) = lower(_application_name)
		and lower(user_name) = lower(_user_name);
	return true;
end;$$;