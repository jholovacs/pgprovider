create or replace function get_all_roles(
	_application_name varchar(250)
	) returns setof roles as $$
begin
return query
select
	*
from
	roles
where
	application_name = _application_name
order by
	role_name;
end;
$$ language plpgsql;
