create or replace function get_roles_for_user(
	_user_name varchar(250),
	_application_name varchar(250)
	) returns setof roles as $$
begin
return query
select
	r.*
from
	users as u
inner join
	users_roles as ur
	on ur.user_id = u.user_id
inner join
	roles as r
	on r.role_id = ur.role_id
where
	u.user_name = _user_name
	and u.application_name = _application_name
	and r.application_name = _application_name;
end;
$$ language plpgsql;
