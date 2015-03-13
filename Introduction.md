# Introduction #

I've spent quite a bit of time trying to find a decent set of ASP.NET membership, role, and profile providers that worked as seamlessly as the MS-provided ones, except for PostgreSQL.  There have been quite a few that were **close**, but limited in some way, incomplete in some way,  and "fixing" someone else's code was unappealing to me.

So now, I won't have to worry about it; and hopefully, other people will not have to feel the frustration I felt.

# Status #
  * The MembershipProvider is complete, and unit tests are available to be run.
  * The RoleProvider is complete, and unit tests are available to be run.

# Details #

The solution is meant to work in Visual Studio 2010.  I have also tested it in Mono, and it works like a champ.  The solution has two projects:

  * The pgProvider project.  This is the class library that would be added to your web project and referenced to gain access to the provider classes.
  * The pgProvider.Tests project.  This is the unit tests for the library.

# System Requirements #
You will need:

  * .NET 4.0 Framework installed
  * Unfettered access to a blank PostgreSQL database with an account that can make DDL changes in the database.
  * Npgsql class library (third party - available through NuGet)
  * Common.Logging class library (third party - available through NuGet)
  * For the unit tests, the NUnit library is required.

That _should_ be it.  Please see the [Instructions](Installation.md) for information on how to set up your database and connection.