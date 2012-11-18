create or replace function role_exists(
	_role_name varchar(250),
	_application_name varchar(250)
	) returns boolean as $$
declare
	retval boolean;
begin

select exists(
	select null from roles where application_name = _application_name and role_name = _role_name)
into retval;

return retval;
end;
$$ language plpgsql;
