# Introduction #
If you are setting up for the first time, and you are configuring retrievable passwords, you will need to generate an encryption key and specify the value in the MembershipProvider configuration.

This document specifies how you can generate that value for the first time.

**Note:** Once you start using the encryption key in reversible encryption, **DO NOT CHANGE IT**.  None of your users will be able to log in unless they reset their password!  Password retrieval will likewise be impossible.

# Instructions #
  * Load the `pgProvider` solution.
  * Add a C# console project to the solution.  Ensure it is set to use the .NET 4 profile, not the .NET 4 Client profile.
  * Add a reference in the console project to the pgProvider project
  * In the console application's `Program.cs` file, replace the contents with the following code:
```
using System;
using pgProvider;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(EncryptionHelper.GenerateAESKey().ToBase64());
            Console.WriteLine("Hit Enter to end.");
            Console.ReadLine();
        }
    }
}
```
  * Run the application, and copy the first line of the output into the clipboard.  This is your shiny new encryption key, and it goes into the web.config file as per [Configuring the MembershipProvider](ConfiguringTheMembershipProvider.md).
  * Remove the console application from your solution and and remove the files from the solution folder.