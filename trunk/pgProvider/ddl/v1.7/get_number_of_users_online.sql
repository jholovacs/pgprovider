create or replace function get_number_of_users_online(
	_session_timeout integer,
	_application_name varchar(250)
	) returns integer as $$
declare record_count integer;
begin

	select
		count(*) into record_count
	from
		users as u
	where
		u.application_name = _application_name
		and last_activity::timestamp with time zone + cast(_session_timeout || ' minutes' as interval) > current_timestamp;	

	return record_count;
end;
$$ language plpgsql;
