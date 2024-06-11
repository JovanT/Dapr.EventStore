dapr run `
    --app-id es-test `
    --app-port 5003 `
    --dapr-http-port 3503 `
    --resources-path components `
    dotnet run