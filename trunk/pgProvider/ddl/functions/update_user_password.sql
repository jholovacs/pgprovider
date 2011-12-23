create or replace function update_user_password(
	username varchar(250),
	saltchars varchar(250),
	passwordhash bytea
	) returns bool as $$

begin

update users
set
	salt = saltchars,
	password = passwordhash,
	last_password_changed = current_timestamp
where
	user_name = username;

return true;

end;
$$ language plpgsql;