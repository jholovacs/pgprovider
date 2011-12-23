create or replace function create_user(
	username varchar(250),
	emailaddress varchar(250),
	approved boolean,
	email_is_unique boolean
	) returns int as $$
	declare userid integer;
	begin
		
	if email_is_unique
		and exists(select null from users as u where u.email = emailaddress) then
		
		raise exception 'The email address specified is already in use, and email addresses are configured to be unique.' using errcode='DUPEM';
	end if;

	if exists(select null from users as u where u.user_name = username) then
		raise exception 'The user name specified is already in use.' using errcode='DUPUN';
	end if;

	insert into users
		(
		user_name,
		email
		)
	values
		(
		username,
		emailaddress
		)
	returning user_id into userid;
		
	return userid;
	end;
	
$$ language plpgsql;