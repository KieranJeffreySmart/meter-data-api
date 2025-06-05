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

## Run Meter Readings API Standalone
From the application folder `apps/readingsapi/`:
``` bash
dotnet run
```

## Run Meter Readings API with Aspire
From the application folder `apps/readingsapi.AppHost/`:
``` bash
dotnet run
```

## Database Configuration
By default the application will start expecting a connection string with the name `readingsdb` to a postgres databse
To use an In Memory databse, set the following environment variables:
``` bash
DB_CONNECTION_TYPE = "InMemory"
DB_CONNECTION_NAME = "MyInMemoryDB" # optional
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