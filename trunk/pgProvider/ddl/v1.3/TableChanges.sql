drop index if exists ux_roles_role_name_application_name;
create unique index ux_roles_role_name_application_name on roles using btree (lower(role_name), lower(application_name));

drop index if exists ix_users_email;
create index ix_users_email on users using btree (lower(application_name), lower(email));

drop index if exists ix_users_last_activity;
create index ix_users_last_activity on users using btree (lower(application_name), last_activity);

drop index if exists ux_users_user_name_application_name;
create unique index ux_users_user_name_application_name on users using btree(lower(application_name), lower(user_name));

create table versions
	(
	name varchar(250) not null primary key,
	version varchar(15) not null
	);

insert into versions
	(name, version)
values
	('application', '1.3');