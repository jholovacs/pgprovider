create or replace function create_user(
	_user_name varchar(250),
	_application_name varchar(250),
	_email varchar(250),
	_approved boolean,
	_email_is_unique boolean
	) returns int as $$
	declare userid integer;
	begin
		
	if _email_is_unique
		and exists(select null from users as u where u.application_name = _application_name and  u.email = _email) then
		raise exception 'The email address specified is already in use for this application, and email addresses are configured to be unique.' using errcode='DUPEM';
	end if;

	if exists(select null from users as u where u.application_name = _application_name and u.user_name = _user_name) then
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
	
$$ language plpgsql;