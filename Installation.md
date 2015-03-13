# Introduction #
Here's a quick rundown of what you need to set up to get things going.

# Create the PostgreSQL Database #
How to do this is out of scope for this document, but there's lots of information to look at online.

You will also need to create a user to own the database.  I recommend naming the database and the user "security", but you don't have to.  If you name the user something else, however, you will need to change ~~the ddl script to properly reflect the object owners, and of course~~ the connection string in your config file.  (Recent versions do not require the user to modify the DDL).

You will need to install the **pl/pgsql** procedural language in your PostgreSQL instance if it is not already installed by default.

The important thing here is that the Npgsql-based client can connect to the database, so the connection string in the web/app.config must be correct.  Another common "gotcha" is that your PostgreSQL server may not be listening, or your credentials are not adequate to connect to the database.

# Create the database objects #
~~In the pgProvider project there is a script **ddl/ScriptDatabase.sql**.  Run this in the context of the PostgreSQL database you are setting up.  This should create all the objects required for the provider.~~
As of version 1.2, as long as the user in your connection string is set as the owner of the provider database, the database objects should be created automatically when initialized, if they are not already there.

# Configure the providers in the web.config #
For the various provider configuration options, look here:

  * [Configuring the MembershipProvider](ConfiguringTheMembershipProvider.md)
  * [Configuring the RoleProvider](ConfiguringTheRoleProvider.md)

# Do your stuff! #
That's all there is to it.