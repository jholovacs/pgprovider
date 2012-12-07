create or replace function update_user(
	_user_id int,
	_user_name varchar(250),
	_application_name varchar(250),
	_email varchar(250),
	_approved boolean,
	_comment text,
	_email_is_unique boolean
	) returns boolean as $$
begin
if _email_is_unique and exists(
	select null from users where application_name = _application_name
		and email = _email and user_id <> _user_id) then
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
end;
$$ language plpgsql;
