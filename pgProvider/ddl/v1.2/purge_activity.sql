CREATE OR REPLACE FUNCTION purge_activity(olderthan bigint)
  RETURNS boolean AS
$BODY$
begin
delete from user_login_activity
where
	"from" < now() - cast(olderthan || ' seconds' as interval);
return true;
end;
$BODY$ LANGUAGE plpgsql;