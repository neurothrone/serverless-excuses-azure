variable "subscription_id" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type        = string
  default     = "Sweden Central"
  description = "Resource group location"
}

variable "cosmosdb_account_name" {
  type = string
}

variable "cosmosdb_database_name" {
  type = string
}

variable "cosmosdb_container_name" {
  type = string
}

variable "service_plan_name" {
  type = string
}

variable "storage_account_name" {
  type = string
}

variable "function_app_name" {
  type = string
}

variable "static_site_name" {
  type = string
}

variable "static_site_location" {
  type = string
}
