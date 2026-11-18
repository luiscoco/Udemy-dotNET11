# Native receipt CLI dependencies

The verified target is win-x64. Publishing requires the Microsoft C++ linker and Windows SDK. The application uses the .NET base libraries and source-generated System.Text.Json metadata. The closed Receipt DTO is included at compile time.

Runtime IL generation, arbitrary assembly loading and unrestricted reflection-based serialization require dependency review. Unsupported/ is a deliberate publish failure probe. Native AOT executables are published separately for each supported operating system and architecture.
