create or replace function get_user_name_by_email(
	emailaddress varchar(250)
	) returns varchar(250) as $$
declare username varchar(250);
begin

	select
		user_name
	from
		users
	where
		email = emailaddress
	limit 1
	into 
		username;

	return username;
end;
$$ language plpgsql;