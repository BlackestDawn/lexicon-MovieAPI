variable "project_id" {
  type        = string
  description = "GCP project this app deploys into."
}

variable "region" {
  type        = string
  default     = "europe-west1"
  description = "Region for Cloud Run and Artifact Registry."
}

variable "github_org" {
  type        = string
  description = "GitHub org/username that owns the repos allowed to deploy into this project."
}

variable "github_repo" {
  type        = string
  default     = "lexicon-MovieAPI"
  description = "GitHub repository name, exact case as it appears in the repo URL. Used only for the Workload Identity Federation binding below (GitHub's OIDC token claim preserves this casing) - not for naming any GCP resources, see local.app_name in main.tf for that."
}
