CREATE OR REPLACE FUNCTION role_exists(_role_name character varying, _application_name character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
declare
	retval boolean;
begin

select exists(
	select null from roles where lower(application_name) = lower(_application_name) and lower(role_name) = lower(_role_name))
into retval;

return retval;
end;
$$;