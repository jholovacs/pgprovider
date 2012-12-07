create or replace function get_online_count(
	_session_timeout integer,
	_application_name varchar(250)
	) returns integer as $$
begin
	return (
		select 
			count(*) 
		from 
			users
		where 
			application_name = _application_name
			and last_activity::time + cast(_session_timeout + ' minutes' as interval) < current_timestamp);
end;
$$ language plpgsql;
