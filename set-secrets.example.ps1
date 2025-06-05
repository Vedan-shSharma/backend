# Database Connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your_database_connection_string_here"

# SMTP Settings
dotnet user-secrets set "Smtp:Host" "your_smtp_host"
dotnet user-secrets set "Smtp:Port" "your_smtp_port"
dotnet user-secrets set "Smtp:Username" "your_smtp_username"
dotnet user-secrets set "Smtp:Password" "your_smtp_password"
dotnet user-secrets set "Smtp:From" "your_smtp_from_email"

# JWT Settings
dotnet user-secrets set "Jwt:Key" "your_jwt_secret_key"
dotnet user-secrets set "Jwt:Issuer" "your_jwt_issuer"
dotnet user-secrets set "Jwt:Audience" "your_jwt_audience"

# Azure Storage Settings
dotnet user-secrets set "AzureStorage:ConnectionString" "your_azure_storage_connection_string"
dotnet user-secrets set "AzureStorage:ContainerName" "your_container_name" 