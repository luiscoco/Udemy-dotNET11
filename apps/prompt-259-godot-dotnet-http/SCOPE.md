# Executable extensibility lesson scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Real Godot 4.7.2 standard GDScript client renders its actual native viewport and sends loopback HTTP requests to a .NET 11 Kestrel backend. Godot does not embed .NET 11 here. This explicit process boundary avoids claiming engine CLR support. The owned client/server fixture validates protocol versions and server-authoritative moves. Godot is a separate prerequisite; set COURSE_GODOT if it is not in the course workspace.
