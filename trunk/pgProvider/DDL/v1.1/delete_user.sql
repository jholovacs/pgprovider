create or replace function delete_user(
	_user_name varchar(250),
	_application_name varchar(250),
	_delete_related boolean
	) returns boolean as $$
begin
if _delete_related then
	delete from user_login_activity as ula
	using users as u
	where u.application_name = _application_name
		and u.user_name = _user_name
		and u.user_id = ula.user_id;
end if;
delete from users
where application_name = _application_name 
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;
