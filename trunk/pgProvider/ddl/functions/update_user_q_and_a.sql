create or replace function update_user_q_and_a(
	username varchar(250),
	questiontext varchar(1000),
	answersalt varchar(250),
	answerhash bytea
	) returns bool as $$

begin

update users
set
	password_question = questiontext,
	password_answer = answerhash,
	answer_salt = answersalt
where
	user_name = username;

return true;

end;
$$ language plpgsql;