Run tests based on the argument provided: $ARGUMENTS

If no argument provided, run all tests:
```bash
dotnet test
```

If argument is "unit":
```bash
dotnet test tests/UserManagement.Tests.Unit
```

If argument is "integration":
```bash
dotnet test tests/UserManagement.Tests.Integration
```

If argument is "coverage":
```bash
dotnet test --collect:"XPlat Code Coverage"
```

If argument looks like a test name or pattern, run filtered:
```bash
dotnet test --filter "FullyQualifiedName~$ARGUMENTS"
```

Report test results summary.