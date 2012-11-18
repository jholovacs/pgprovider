create or replace function remove_users_from_roles(
	_users varchar(250)[],
	_roles varchar(250)[],
	_application_name varchar(250)
	) returns boolean as $$
begin
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
if exists (select null from usernames as un left outer join users as u on u.application_name = _application_name and u.user_name = un.username where u.user_id is null)
	or exists (select null from rolenames as rn left outer join roles as r on r.application_name = _application_name and r.role_name = rn.rolename where r.role_id is null) then
	raise exception 'At least one user name or role specified does not exist in the application scope.' using errcode='MSING';
end if;

-- Insert the records linking the users to the roles, excluding pre-existing relationships.
delete from users_roles
using
	usernames as un
inner join
	users as u on u.application_name = _application_name and u.user_name = un.username
cross join
	rolenames as rn
inner join
	roles as r on r.application_name = _application_name and r.role_name = rn.rolename
where 
	users_roles.user_id = u.user_id
	and users_roles.role_id = r.role_id;
return true;
end;
$$ language plpgsql;
