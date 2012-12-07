CREATE OR REPLACE FUNCTION get_all_roles(_application_name character varying) RETURNS SETOF roles
    LANGUAGE plpgsql
    AS $$
begin
return query
select
	*
from
	roles
where
	lower(application_name) = lower(_application_name)
order by
	lower(role_name);
end;
$$;