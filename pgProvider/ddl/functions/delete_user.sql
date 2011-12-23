create or replace function delete_user(
	username varchar(250),
	delete_related boolean
	) returns void as $$
begin
		
if delete_related then
	delete from user_login_activity
	where user_name=username;

	-- TODO: look for role and profile information and delete it
		
end if;

delete from users
where user_name = username;
	
end;

$$ language plpgsql;