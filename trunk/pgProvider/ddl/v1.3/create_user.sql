CREATE OR REPLACE FUNCTION create_user(_user_name character varying, _application_name character varying, _email character varying, _approved boolean, _email_is_unique boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $$	declare userid integer;
	begin
		
	if _email_is_unique
		and exists(select null from users as u where lower(u.application_name) = lower(_application_name) and  lower(u.email) = lower(_email)) then
		raise exception 'The email address specified is already in use for this application, and email addresses are configured to be unique.' using errcode='DUPEM';
	end if;

	if exists(select null from users as u where lower(u.application_name) = lower(_application_name) and lower(u.user_name) = lower(_user_name)) then
		raise exception 'The user name specified is already in use for this application.' using errcode='DUPUN';
	end if;

	insert into users
		(
		user_name,
		application_name,
		email,
		approved
		)
	values
		(
		_user_name,
		_application_name,
		_email,
		_approved
		)
	returning user_id into userid;
		
	return userid;
	end;
	
$$;