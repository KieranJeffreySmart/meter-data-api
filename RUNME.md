# Run Accumulated Triangle Solution

## Requirements
To build and run from code you will need the following:
* .Net 9 SDK

## Build Solution
From the root folder:
``` bash
dotnet build
```

## Run Tests
From the root folder:
``` bash
dotnet test
```

## Publish Distributable
From the root folder:
#### Local OS:
``` bash
dotnet publish apps/readingsapi/readingsapi.csproj
```
#### Windows OS:
``` bash
dotnet publish apps/readingsapi/readingsapi.csproj -r win-x64
```

## Run Distributable
From the root folder:
#### Local OS:
``` bash
./apps/readingsapi/bin/Release/net9.0/publish/readingsapi
```

#### Windows OS:
``` bash
./apps/readingsapi/bin/Release/net9.0/win-x64/publish/readingsapi
```