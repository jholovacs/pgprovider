create or replace function delete_role(
	_role_name varchar(250),
	_throw_on_populated boolean
	) returns boolean as $$
declare _role_id integer;
begin
	if _throw_on_populated and exists(
		select null from roles as r
		inner join users_roles as ur
		on ur.role_id = r.role_id) then
		return false;
	end if;

	delete from roles
	where role_name = _role_name;
	return true;
end;
$$ language plpgsql;