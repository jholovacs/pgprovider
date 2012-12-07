CREATE OR REPLACE FUNCTION user_is_in_role(_user_name character varying, _role_name character varying, _application_name character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
	declare
	retval boolean;
begin
-- per the roleprovider pattern, throw an exception if the role does not exist
if not exists(select null from roles where lower(role_name) = lower(_role_name) and lower(application_name) = lower(_application_name)) then
	raise exception 'The specified role does not exist.' using errcode='NOROL';
end if;
-- per the roleprovider pattern, throw an exception if the user does not exist
if not exists(select null from users where lower(user_name) = lower(_user_name) and lower(application_name) = lower(_application_name)) then
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
			lower(u.user_name) = lower(_user_name)
			and lower(u.application_name) =lower( _application_name)
			and lower(r.role_name) = lower( _role_name)
			and lower(r.application_name) = lower(_application_name)
		)
into retval;
return retval;
end;$$;