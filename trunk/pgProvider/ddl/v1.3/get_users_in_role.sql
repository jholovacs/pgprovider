CREATE OR REPLACE FUNCTION get_users_in_role(_role_name character varying, _application_name character varying, _partial_username character varying) RETURNS SETOF user_record
    LANGUAGE plpgsql
    AS $$
	begin

if not exists (select null from roles where lower(application_name) = lower(_application_name) and lower(role_name) = lower(_role_name)) then
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
	lower(r.role_name) = lower(_role_name)
	and lower(r.application_name) = lower(_application_name)
	and u.user_name ilike '%' || _partial_username || '%'
	and lower(u.application_name) = lower(_application_name)
order by
	u.user_name;
end;
$$;