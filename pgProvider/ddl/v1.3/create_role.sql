CREATE OR REPLACE FUNCTION create_role(_role_name character varying, _application_name character varying, _role_description text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
declare _role_id integer;
begin
	-- per the role provider pattern, need to throw an exception if the role exists already.
	if exists(select null from roles where application_name = _application_name and role_name = _role_name) then
		raise exception 'The role already exists in this application.' using errcode='DUPRL';
	end if;

	insert into roles
		(
		role_name,
		application_name,
		role_description
		)
	values
		(
		_role_name,
		_application_name,
		_role_description
		)
	returning role_id into _role_id;
	return _role_id;
end;
$$;