CREATE OR REPLACE FUNCTION update_user(_user_id integer, _user_name character varying, _application_name character varying, _email character varying, _approved boolean, _comment text, _email_is_unique boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
	begin
if _email_is_unique and exists(
	select null from users where lower(application_name) = lower(_application_name)
		and lower(email) = lower(_email) and user_id <> _user_id) then
	raise exception 'The email address specified is already in use by another user.' using errcode='DUPEM';
end if;
update users
set
	user_name = _user_name,
	email = _email,
	approved = _approved,
	comment = _comment
where
	user_id = _user_id;
return true;
end;$$;
