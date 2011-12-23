drop type if exists user_record cascade;
create type user_record as(
	user_id int,
	user_name varchar(250),
	last_activity timestamp with time zone,
	created timestamp with time zone,
	email varchar(250),
	approved boolean,
	last_lockout timestamp with time zone,
	last_login timestamp with time zone,
	last_password_changed timestamp with time zone,
	password_question varchar(1000),
	comment text
);

create function get_all_users() 
	returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	order by
		user_id asc;

end;
$$ language plpgsql;

create function get_users_by_email(
	partial_email varchar(250)
	)returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		email ilike '%' || partial_email || '%'
	order by
		user_id asc;

end;
$$ language plpgsql;

create function get_users_by_username(
	partial_username varchar(250)
	)returns setof user_record as $$
begin
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		user_name ilike '%' || partial_username || '%'
	order by
		user_id asc;

end;
$$ language plpgsql;

create function get_users_online(
	session integer
	) returns setof user_record as $$
begin

	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		last_activity::time + cast(session + ' minutes' as interval) < current_timestamp;	

end;
$$ language plpgsql;

create or replace function get_online_count(
	session integer
	) returns integer as $$
begin
	return (select count(*) from users
		where last_activity::time + cast(session + ' minutes' as interval) < current_timestamp);
end;
$$ language plpgsql;

create function get_user_by_username(
	_user_name varchar(250),
	_online boolean
	) returns setof user_record as $$
begin
if _online then
	return query
	update users
	set 
		last_activity = current_timestamp
	where 
		user_name = _user_name
	returning
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment;
else
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		user_name = _user_name;
end if;
end;
$$ language plpgsql;

create function get_user_by_id(
	_user_id integer,
	_online boolean
	) returns setof user_record as $$
begin
if _online then
	return query
	update users
	set 
		last_activity = current_timestamp
	where 
		user_id = _user_id
	returning
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment;
else
	return query
	select
		user_id,
		user_name,
		last_activity,
		created,
		email,
		approved,
		last_lockout,
		last_login,
		last_password_changed,
		password_question,
		comment
	from
		users
	where
		user_id = _user_id;
end if;
end;
$$ language plpgsql;