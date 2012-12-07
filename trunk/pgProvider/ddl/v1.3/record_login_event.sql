CREATE OR REPLACE FUNCTION record_login_event(
	_user_name character varying, 
	_application_name character varying, 
	_origin character varying, 
	_success_indicator boolean, 
	_attempt_window integer, 
	_attempt_count integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
	declare userid integer;
begin
select
	user_id
from users as u
where lower(u.application_name) = lower(_application_name)
	and lower(u.user_name) = lower(_user_name)
limit 1 into userid;

insert into user_login_activity
	(
	"from",
	success,
	"user_id"
	)
values
	(
	_origin,
	_success_indicator,
	userid
	);

-- check for last x attempts if failure
if not _success_indicator and _attempt_count <> 0 then
	if (select count(*) from 
		(select "success" from user_login_activity as ula
			where ula.user_id = userid
			and ula.when > current_timestamp - cast(_attempt_window || ' minutes' as interval)
			order by ula.when desc limit _attempt_count) as last_login_attempts
		where "success" = false) >= _attempt_count then

	update users
	set
		last_lockout = current_timestamp
	where
		user_id = userid;
	end if;
end if;
return true;
end;
$$;