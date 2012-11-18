create or replace function user_is_in_role(
	_user_name varchar(250),
	_role_name varchar(250),
	_application_name varchar(250)
	) returns boolean as $$
declare
	retval boolean;
begin
-- per the roleprovider pattern, throw an exception if the role does not exist
if not exists(select null from roles where role_name = _role_name and application_name = _application_name) then
	raise exception 'The specified role does not exist.' using errcode='NOROL';
end if;
-- per the roleprovider pattern, throw an exception if the user does not exist
if not exists(select null from users where user_name = _user_name and application_name = _application_name) then
	raise exception 'The specified user does not exist.' using errcode='NOUSR';
end if;

select
	exists(
		select null
		from users as u
		inner join users_roles as ur
		on ur.user_id = u.user_id
		inner join roles as r
		on r.role_id = ur.role_id
		where
			u.user_name = _user_name
			and u.application_name = _application_name
			and r.role_name = _role_name
			and r.application_name = _application_name)
into retval;
return retval;
end;
$$ language plpgsql;
