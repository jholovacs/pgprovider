CREATE OR REPLACE FUNCTION delete_user(_user_name character varying, _application_name character varying, _delete_related boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$begin
if _delete_related then
	delete from user_login_activity as ula
	using users as u
	where lower(u.application_name) = lower(_application_name)
		and lower(u.user_name) = lower(_user_name)
		and u.user_id = ula.user_id;
end if;
delete from users
where lower(application_name) = lower(_application_name )
	and lower(user_name) = lower(_user_name);
return true;
end;
$$;