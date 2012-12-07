CREATE OR REPLACE FUNCTION get_user_credentials(_user_name character varying, _application_name character varying) RETURNS SETOF creds
    LANGUAGE plpgsql
    AS $$begin
	return query
	select
		salt,
		password,
		password_answer,
		answer_salt,
		last_lockout
	from
		users
	where
		lower(application_name) = lower(_application_name)
		and lower(user_name) = lower(_user_name)
	limit 1;
end;
$$;