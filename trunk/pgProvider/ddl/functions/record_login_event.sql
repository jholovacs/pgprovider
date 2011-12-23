create or replace function record_login_event(
	username varchar(250),
	origin varchar(250),
	success_indicator boolean,
	attempt_window integer,
	attempt_count integer
	) returns boolean as $$

begin
		
insert into user_login_activity
	(
	"from",
	"success",
	"user_name"
	)
values
	(
	origin,
	success_indicator,
	username
	);

-- check for last x attempts if failure

if not success_indicator and attempt_count <> 0 then
	if (select count(*) from 
		(select "success" from user_login_activity as ula
			where ula.user_name = username
			and ula.when > current_timestamp - cast(attempt_window || ' minutes' as interval)
			order by ula.when desc limit attempt_count) as last_login_attempts
		where "success" = false) >= attempt_count then

	update users
	set
		last_lockout = current_timestamp
	where
		user_name = username;
	end if;
end if;

return true;

end;
$$ language plpgsql;