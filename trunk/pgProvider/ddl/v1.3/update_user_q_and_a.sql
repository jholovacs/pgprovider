CREATE OR REPLACE FUNCTION update_user_q_and_a(_user_name character varying, _application_name character varying, _password_question character varying, _answer_salt character varying, _password_answer bytea) RETURNS boolean
    LANGUAGE plpgsql
    AS $$begin
update users
set
	password_question = _password_question,
	password_answer = _password_answer,
	answer_salt = _answer_salt
where
	lower(application_name) = lower(_application_name)
	and lower(user_name) = lower(_user_name);
return true;
end;$$;
