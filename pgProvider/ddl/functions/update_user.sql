create or replace function update_user(
	userid int,
	username varchar(250),
	emailaddress varchar(250),
	isapproved boolean,
	comments text,
	email_is_unique boolean
	) returns void as $$
begin

if email_is_unique and exists(
	select null from users where email = emailaddress and user_id <> userid) then
	raise exception 'The email address specified is already in use by another user.' using errcode='DUPEM';
end if;

update users
set
	user_name = username,
	email = emailaddress,
	approved = isapproved,
	comment = comments
where
	user_id = userid;

end;
$$ language plpgsql;