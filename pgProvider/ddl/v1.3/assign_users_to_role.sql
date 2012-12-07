CREATE OR REPLACE FUNCTION assign_users_to_roles(_users character varying[], _roles character varying[], _application_name character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$begin
create temporary table usernames (username varchar(250) not null primary key) on commit drop;
create temporary table rolenames (rolename varchar(250) not null primary key) on commit drop;
-- Create the tables based off the arrays.
insert into usernames
	(
	username
	)
select distinct
	username
from
	unnest(_users) as username;

insert into rolenames
	(
	rolename
	)
select distinct
	rolename
from
	unnest(_roles) as rolename;

-- Per the role provider pattern, an exception is to be thrown if any of the role names or user names specified
-- do not exist for the given application name.
if exists (select null from usernames as un left outer join users as u on lower(u.application_name) = lower(_application_name) 
	and lower(u.user_name) = lower(un.username) where u.user_id is null)
	or exists (select null from rolenames as rn left outer join roles as r on lower(r.application_name) = lower(_application_name) 
		and lower(r.role_name) = lower(rn.rolename) where r.role_id is null) then
	raise exception 'At least one user name or role specified does not exist in the application scope.' using errcode='MSING';
end if;

-- Insert the records linking the users to the roles, excluding pre-existing relationships.
insert into users_roles
	(
	user_id,
	role_id
	)
select distinct
	u.user_id,
	r.role_id
from
	usernames as un
inner join
	users as u on lower(u.application_name) = lower(_application_name) and lower(u.user_name) = lower(un.username)
cross join
	rolenames as rn
inner join
	roles as r on lower(r.application_name) = lower(_application_name) and lower(r.role_name) = lower(rn.rolename)
where not exists(
	select null
	from users_roles
	where
		user_id = u.user_id
		and role_id = r.role_id
	);

return true;
end;
$$;