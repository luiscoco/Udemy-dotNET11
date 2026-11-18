These .NET 11 examples were prepared for teaching and were not executed on .NET 11 on the production machine. Install and select a suitable .NET 11 SDK before running them.

Run commands from this practice directory unless the setup script creates a Workspace directory.

Open DebugDemo in VS Code with a compatible C# Dev Kit setup. To reproduce the initial exception, temporarily replace Program.cs with InitialFailure.cs.txt contents; use the profile argument 0. Restore the guarded Program.cs afterward. Verify using dotnet run --project DebugDemo --no-launch-profile -- 2, then 0 and invalid. Expected: 5 for 2; a validation message for invalid inputs.
