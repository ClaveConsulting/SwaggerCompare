# Clave.SwaggerCompare

[![Nuget](https://img.shields.io/nuget/v/Clave.SwaggerCompare)][1] [![Nuget](https://img.shields.io/nuget/dt/Clave.SwaggerCompare)][1] [![Build Status](https://dev.azure.com/ClaveConsulting/Nugets/_apis/build/status/ClaveConsulting.SwaggerCompare?branchName=master)](https://dev.azure.com/ClaveConsulting/Nugets/_build/latest?definitionId=12&branchName=master)

## Installation

Run `dotnet tool install --global Clave.SwaggerCompare`

## First time usage

Run `swagger-compare` in cmd. This will create a `config.json` example file in the folder you're running from. Then open the file and start editing.

## Config file structure

```json
{
  "Client1": {
    "Url": "", // Fully qualified URL. Serves as "base address".
    "Headers": {
      "Authorization": "Bearer AbCdEf123456" // Optionally specify any headers you would like to add to requests.
    }
  },
  "Client2": {
    "Url": "" // Fully qualified URL. Serves as "base address".
  },
  "TestRuns": [
    {
      "ExcludeEndpoints": [], // string[]. Endpoints that should not be called. Use asterisk for wildcard matching, f.ex. ["api/*/invoices", "*/health"]
      "TreatParametersAsRequired": [], // string[]. Often parameters are not *required* in the swagger definition, but in reality they are.
      "UrlParameterTestValues": {
        "param1": "value1"
      },
      "IncludeEndpoints": [
        {
          "Endpoint": "/time-slots",
          "Method": "POST",
          "DataFolder": "..\\test-data\\time-slot",
          "DisregardJsonResponseProperties": ["token"]
        }
      ]
    }
  ]
}
```

## License

The MIT license

[1]: https://www.nuget.org/packages/Clave.SwaggerCompare/