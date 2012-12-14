CREATE OR REPLACE FUNCTION delete_role(_role_name character varying, _application_name character varying, _throw_on_populated boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
declare _role_id integer;
begin
	if _throw_on_populated and exists(
		select null from roles as r
		inner join users_roles as ur
		on ur.role_id = r.role_id
		where lower(r.application_name) = lower(_application_name) 
		and lower(r.role_name) = lower(_role_name)
		) then
		raise exception 'The specified role is populated; cannot delete.' using errcode='RLPOP';
	end if;

	if not exists(select null from roles where lower(application_name) = lower(_application_name) and lower(role_name) = lower(_role_name)) then
		raise exception 'The specified role does not exist.' using errcode='NOROL';
	end if;

	delete from roles
	where lower(application_name) = lower(_application_name)
		and lower(role_name) = lower(_role_name);
	return true;
end;
$$;