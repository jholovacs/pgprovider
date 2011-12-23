create or replace function create_role(
	_role_name varchar(250),
	_role_description text
	) returns int as $$
declare _role_id integer;
begin
	insert into roles
		(
		role_name,
		role_description
		)
	values
		(
		_role_name,
		_role_description
		)
	returning role_id into _role_id;

	return _role_id;
end;
$$ language plpgsql;