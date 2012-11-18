create or replace function get_users_in_role(
	_role_name varchar(250),
	_application_name varchar(250),
	_partial_username varchar(250)
	) returns setof user_record as $$
begin

if not exists (select null from roles where application_name = _application_name and role_name = _role_name) then
	raise exception 'The specified role does not exist.' using errcode='ROLNA';
end if;

return query
select
	u.user_id,
	u.user_name,
	u.last_activity,
	u.created,
	u.email,
	u.approved,
	u.last_lockout,
	u.last_login,
	u.last_password_changed,
	u.password_question,
	u.comment
from
	roles as r
inner join
	users_roles as ur
	on ur.role_id = r.role_id
inner join
	users as u
	on u.user_id = ur.user_id
where
	r.role_name = _role_name
	and r.application_name = _application_name
	and u.user_name ilike '%' || _partial_username || '%'
	and u.application_name = _application_name
order by
	u.user_name;
end;
$$ language plpgsql;
