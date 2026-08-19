# Deployment

```
merge to main ──▶ build & push images ──▶ deploy Cloud Run staging (backend + frontend)
                                                          │
                                              (manual trigger, approval gate)
                                                          ▼
                                        reuse both images unchanged ──▶ deploy Cloud Run prod
```

- **Compute**: Google Cloud Run, two services (`movieapi-backend-*`, `movieapi-frontend-*`) per environment.
- **Database**: [Neon](https://neon.tech) serverless Postgres — prod and a copy-on-write staging branch.
- **Cache/logging**: config-driven, not infrastructure — `OutputCache:Provider` and `Logging:Sink` (see [`backend/src/MovieAPI.Api/Program.cs`](../backend/src/MovieAPI.Api/Program.cs)) select `Memory`/`GoogleCloudLogging` for this deployment, so no Redis or Elasticsearch is provisioned.
- **Auth**: GitHub Actions authenticates to GCP via Workload Identity Federation — no long-lived service account keys.
- **Provisioning**: `deploy/terraform/gcp` and `deploy/terraform/neon`. The GCP stack references a shared Workload Identity Pool and Artifact Registry repo, provisioned once by a separate bootstrap stack ([BlackestDawn/various-terraform](https://github.com/BlackestDawn/various-terraform)) and reused (via Terraform data sources, not recreated) by every app deployed into this GCP project.
- **Migrations**: applied by a dedicated CI step (`dotnet ef database update`), not on container boot — see the comment on `ApplyMigrationsOnStartup` in `Program.cs`. Multiple Cloud Run replicas racing migrations against the same database at boot is exactly what this avoids.
- **Frontend/backend image reuse**: both images are built once during the staging deploy and promoted unchanged to prod — `BACKEND_URL` is read at runtime by [`frontend/src/proxy.ts`](../frontend/src/proxy.ts), not baked in at build time, so no per-environment rebuild is needed for either service.

## One-time bootstrap

> **Prerequisite**: the shared Workload Identity Pool/provider (`github-actions`) and the `apps` Artifact Registry repo must already exist in the target GCP project before running anything below — `deploy/terraform/gcp` only reads them via data sources, it doesn't create them. Apply [BlackestDawn/various-terraform](https://github.com/BlackestDawn/various-terraform) first if this is a new GCP project.

1. **GCP stack** — creates this app's deployer/runtime service accounts, IAM bindings, and empty Secret Manager secret containers.

   ```bash
   cd deploy/terraform/gcp
   cp terraform.tfvars.example terraform.tfvars   # fill in project_id, github_org if different
   terraform init
   terraform apply
   ```

2. **Neon stack** — creates the Neon project plus a staging branch.

   ```bash
   cd deploy/terraform/neon
   export NEON_API_KEY=...                        # https://console.neon.tech/app/settings/api-keys
   cp terraform.tfvars.example terraform.tfvars
   terraform init
   terraform apply
   ```

3. **Populate secrets** — Terraform only creates the secret containers, not their values.

   ```bash
   cd deploy/terraform/neon
   terraform output -raw prod_connection_string    | gcloud secrets versions add movieapi-prod-database-url    --data-file=-
   terraform output -raw staging_connection_string | gcloud secrets versions add movieapi-staging-database-url --data-file=-

   # OpenIddict requires an asymmetric signing key - a plain `openssl rand`
   # symmetric key will not work (see the comment in Program.cs). Generate a
   # fresh RSA key pair per environment:
   openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 | openssl pkcs8 -topk8 -nocrypt -outform DER | base64 -w0 \
     | gcloud secrets versions add movieapi-staging-openiddict-signing-key --data-file=-
   openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 | openssl pkcs8 -topk8 -nocrypt -outform DER | base64 -w0 \
     | gcloud secrets versions add movieapi-prod-openiddict-signing-key --data-file=-

   # The encryption key can stay symmetric.
   openssl rand -base64 32 | gcloud secrets versions add movieapi-staging-openiddict-encryption-key --data-file=-
   openssl rand -base64 32 | gcloud secrets versions add movieapi-prod-openiddict-encryption-key    --data-file=-

   # Admin account password (see AdminUserSeeder) - without this, staging/prod
   # never seed an admin account at all, since that seeder is a deliberate
   # no-op when unconfigured. Use a different random password per environment.
   openssl rand -base64 24 | gcloud secrets versions add movieapi-staging-admin-password --data-file=-
   openssl rand -base64 24 | gcloud secrets versions add movieapi-prod-admin-password    --data-file=-
   ```

4. **GitHub repo secrets** (Settings → Secrets and variables → Actions):

   From `deploy/terraform/gcp` outputs:
   - `GCP_PROJECT_ID`
   - `GCP_WORKLOAD_IDENTITY_PROVIDER` → `terraform output workload_identity_provider`
   - `GCP_DEPLOYER_SA` → `terraform output deployer_service_account`
   - `GCP_RUNTIME_SA` → `terraform output runtime_service_account`

   Set by hand, matching whatever you used for `region` in `terraform.tfvars` and the domain from step 7 below:
   - `GCP_REGION` (e.g. `europe-west1`)
   - `DOMAIN` (e.g. `alexstauch.app` — the workflows build `movieapi.$DOMAIN` / `movieapi-api.$DOMAIN` from it)
   - `ADMIN_EMAIL` — email for the seeded admin account (`Seed:AdminEmail`), shared by both staging and prod; the password comes from the per-environment Secret Manager secret populated in step 3

5. **GitHub environment** — create an environment named `production` (Settings → Environments) with a required reviewer, so `deploy-prod.yml` always pauses for manual approval.

6. **First deploy** — push to `main` (or run "Deploy to staging" manually) to create the staging Cloud Run services, then run "Deploy to production" manually once staging looks good.

   To seed the sample movie catalog on that first deploy (`DbSeeder` — genres, people, movies, reviews), trigger the workflow manually (Actions → Deploy to staging/production → Run workflow) with `seed_example_data` checked. It's a no-op once the `Movies` table has any rows, so it's safe to leave unchecked on every deploy after the first — there's no need to turn it back off.

   The runtime service account also needs `roles/logging.logWriter` for `Logging:Sink=GoogleCloudLogging` to work — already granted by the GCP stack's Terraform (`deploy/terraform/gcp/main.tf`), no separate step needed.

7. **Domain mappings** — Cloud Run custom domains aren't in Terraform; one-time CLI step per service:

   ```bash
   PROJECT=your-gcp-project-id
   REGION=your-gcp-region
   DOMAIN=your-domain

   gcloud beta run domain-mappings create --service=movieapi-frontend-prod    --domain=movieapi.$DOMAIN             --region=$REGION --project=$PROJECT
   gcloud beta run domain-mappings create --service=movieapi-frontend-staging --domain=movieapi-staging.$DOMAIN     --region=$REGION --project=$PROJECT
   gcloud beta run domain-mappings create --service=movieapi-backend-prod     --domain=movieapi-api.$DOMAIN         --region=$REGION --project=$PROJECT
   gcloud beta run domain-mappings create --service=movieapi-backend-staging  --domain=movieapi-api-staging.$DOMAIN --region=$REGION --project=$PROJECT
   ```

   Then create the matching DNS record in whichever Cloud DNS zone hosts `$DOMAIN` for each — Cloud Run subdomain mappings resolve via a CNAME to `ghs.googlehosted.com.`:

   ```bash
   PROJECT=your-gcp-project-id
   DOMAIN=your-domain
   ZONE=your-cloud-dns-zone-name   # e.g. `gcloud dns managed-zones list --project=$PROJECT`

   gcloud dns record-sets create movieapi.$DOMAIN. \
     --zone=$ZONE \
     --type=CNAME \
     --ttl=300 \
     --rrdatas=ghs.googlehosted.com. \
     --project=$PROJECT
   ```

   Repeat for `movieapi-staging`, `movieapi-api`, and `movieapi-api-staging`.
