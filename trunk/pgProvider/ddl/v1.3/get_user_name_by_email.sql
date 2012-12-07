CREATE OR REPLACE FUNCTION get_user_name_by_email(_email character varying, _application_name character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$
	declare username varchar(250);
begin
	select
		user_name
	from
		users
	where
		lower(application_name) = lower(_application_name)
		and lower(email) = lower(_email)
	limit 1
	into 
		username;
	return username;
end;$$;