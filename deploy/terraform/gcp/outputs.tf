output "artifact_registry_repo" {
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${data.google_artifact_registry_repository.apps.repository_id}"
  description = "Prefix for image tags, e.g. <this>/movieapi-backend:<git-sha>"
}

output "workload_identity_provider" {
  value       = data.google_iam_workload_identity_pool_provider.github.name
  description = "Value for the GCP_WORKLOAD_IDENTITY_PROVIDER GitHub Actions secret."
}

output "deployer_service_account" {
  value       = google_service_account.deployer.email
  description = "Value for the GCP_DEPLOYER_SA GitHub Actions secret."
}

output "runtime_service_account" {
  value       = google_service_account.runtime.email
  description = "Value for the GCP_RUNTIME_SA GitHub Actions secret."
}

output "db_url_secret_ids" {
  value       = { for k, s in google_secret_manager_secret.db_url : k => s.secret_id }
  description = "Secret Manager secret IDs - populate versions with the Neon stack's connection-string outputs, e.g. `terraform output -raw prod_connection_string | gcloud secrets versions add <id> --data-file=-`."
}

output "openiddict_signing_key_secret_ids" {
  value       = { for k, s in google_secret_manager_secret.openiddict_signing_key : k => s.secret_id }
  description = "Secret Manager secret IDs - populate versions with a fresh RSA key per environment, e.g. `openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 | openssl pkcs8 -topk8 -nocrypt -outform DER | base64 -w0 | gcloud secrets versions add <id> --data-file=-`."
}

output "openiddict_encryption_key_secret_ids" {
  value       = { for k, s in google_secret_manager_secret.openiddict_encryption_key : k => s.secret_id }
  description = "Secret Manager secret IDs - populate versions with e.g. `openssl rand -base64 32 | gcloud secrets versions add <id> --data-file=-`."
}

output "admin_password_secret_ids" {
  value       = { for k, s in google_secret_manager_secret.admin_password : k => s.secret_id }
  description = "Secret Manager secret IDs - populate versions with a strong random password per environment, e.g. `openssl rand -base64 24 | gcloud secrets versions add <id> --data-file=-`. Pair with the ADMIN_EMAIL GitHub Actions secret."
}
