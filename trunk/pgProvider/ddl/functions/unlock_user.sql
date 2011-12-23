create or replace function unlock_user(
	username varchar(250)
	) returns void as $$
begin
		
	update users
	set
		last_lockout = null
	where
		user_name = username;

end;
	
$$ language plpgsql;