CREATE OR REPLACE FUNCTION get_roles_for_user(_user_name character varying, _application_name character varying) RETURNS SETOF roles
    LANGUAGE plpgsql
    AS $$begin
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
	lower(u.user_name) = lower(_user_name)
	and lower(u.application_name) = lower(_application_name)
	and lower(r.application_name) = lower(_application_name);
end;$$;
