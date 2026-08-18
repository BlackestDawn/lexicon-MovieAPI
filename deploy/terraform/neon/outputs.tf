# Npgsql does not parse `postgres://` URI-style connection strings (verified
# directly against NpgsqlConnectionStringBuilder - it throws), unlike most
# Node/Postgres clients, so these build ADO.NET keyword=value strings from
# Neon's individual computed attributes instead of using connection_uri.
# Password is single-quoted in case it ever contains a character (';', '=')
# that would otherwise need escaping in the connection string.

output "prod_connection_string" {
  value       = "Host=${neon_project.this.database_host};Port=5432;Database=${neon_project.this.database_name};Username=${neon_project.this.database_user};Password='${neon_project.this.database_password}';Ssl Mode=Require"
  description = "Push into Secret Manager: terraform output -raw prod_connection_string | gcloud secrets versions add movieapi-prod-database-url --data-file=-"
  sensitive   = true
}

output "staging_connection_string" {
  value       = "Host=${neon_endpoint.staging.host};Port=5432;Database=${neon_project.this.database_name};Username=${neon_project.this.database_user};Password='${neon_project.this.database_password}';Ssl Mode=Require"
  description = "Push into Secret Manager: terraform output -raw staging_connection_string | gcloud secrets versions add movieapi-staging-database-url --data-file=-"
  sensitive   = true
}
