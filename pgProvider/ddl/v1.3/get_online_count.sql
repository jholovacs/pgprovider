CREATE OR REPLACE FUNCTION get_online_count(_session_timeout integer, _application_name character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$
begin
	return (
		select 
			count(*) 
		from 
			users
		where 
			lower(application_name) = lower(_application_name)
			and last_activity::time + cast(_session_timeout + ' minutes' as interval) < current_timestamp);
end;
$$;