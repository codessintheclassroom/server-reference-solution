# Server Reference Solution [![Build Status](https://dev.azure.com/sierrasoftworks/opensource/_apis/build/status/codessintheclassroom/server-reference-solution?branchName=master)](https://dev.azure.com/sierrasoftworks/opensource/_build/latest?definitionId=10&branchName=master)
**An example implementation of the [Shelter API][]**

This repository includes an example implementation of the [Shelter API][] written in C# on the ASP.NET Core web framework.
It leverages Azure AD as an authentication and authorization provider for the protected endpoints within the API and makes use
of various best practices and standard libraries.

## Running the Server

```powershell
> dotnet run -p Shelter
```

## Running the Test Suite

```powershell
> dotnet test
```

[Shelter API]: https://codessintheclassroomshelter.docs.apiary.io
