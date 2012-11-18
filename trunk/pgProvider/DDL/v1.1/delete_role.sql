create or replace function delete_role(
	_role_name varchar(250),
	_application_name varchar(250),
	_throw_on_populated boolean
	) returns boolean as $$
declare _role_id integer;
begin
	if _throw_on_populated and exists(
		select null from roles as r
		inner join users_roles as ur
		on ur.role_id = r.role_id
		where r.application_name = _application_name 
		and r.role_name = _role_name
		) then
		raise exception 'The specified role is populated; cannot delete.' using errcode='RLPOP';
	end if;

	if not exists(select null from roles where application_name = _application_name and role_name = _role_name) then
		raise exception 'The specified role does not exist.' using errcode='NOROL';
	end if;

	delete from roles
	where application_name = _application_name
		and role_name = _role_name;
	return true;
end;
$$ language plpgsql;
