create or replace function get_user_name_by_email(
	_email varchar(250),
	_application_name varchar(250)
	) returns varchar(250) as $$
declare username varchar(250);
begin
	select
		user_name
	from
		users
	where
		application_name = _application_name 
		and email = _email
	limit 1
	into 
		username;
	return username;
end;
$$ language plpgsql;
