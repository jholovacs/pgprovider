create or replace function update_user_q_and_a(
	_user_name varchar(250),
	_application_name varchar(250),
	_password_question varchar(1000),
	_answer_salt varchar(250),
	_password_answer bytea
	) returns boolean as $$
begin
update users
set
	password_question = _password_question,
	password_answer = _password_answer,
	answer_salt = _answer_salt
where
	application_name = _application_name
	and user_name = _user_name;
return true;
end;
$$ language plpgsql;
