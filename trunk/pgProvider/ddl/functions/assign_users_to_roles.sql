create or replace function assign_users_to_roles(
	_users varchar(250)[],
	_roles varchar(250)[] ) returns boolean as $$
begin

insert into users_roles
	(
	user_id,
	role_id
	)
select distinct
	u.user_id,
	r.role_id
from
	unnest(_users) as _user_name
inner join
	users as u 
	on u.user_name = _user_name
cross join
	unnest(_roles) as _role_name
inner join
	roles as r
	on r.role_name = _role_name
where not exists(
	select null
	from users_roles
	where
		user_id = _user_id
		and role_id = _role_id
	);

return true;

end;
$$ language plpgsql;