# Optional: deploy the container image from GHCR to Azure App Service (Linux).
# Usage:  terraform init && terraform apply -var="github_owner=<your-github-username>" \
#           -var="db_connection_string=..." -var="jwt_key=..."

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.100"
    }
  }
}

provider "azurerm" {
  features {}
}

variable "github_owner" { type = string }
variable "location" {
  type    = string
  default = "centralindia"
}
variable "app_name" {
  type    = string
  default = "taskflow-api-demo"
}
variable "db_connection_string" {
  type      = string
  sensitive = true
}
variable "jwt_key" {
  type      = string
  sensitive = true
}

resource "azurerm_resource_group" "rg" {
  name     = "rg-taskflow"
  location = var.location
}

resource "azurerm_service_plan" "plan" {
  name                = "plan-taskflow"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "api" {
  name                = var.app_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.plan.id
  https_only          = true

  site_config {
    health_check_path = "/health"
    application_stack {
      docker_image_name   = "${var.github_owner}/taskflow:latest"
      docker_registry_url = "https://ghcr.io"
    }
  }

  app_settings = {
    WEBSITES_PORT              = "8080"
    ConnectionStrings__Default = var.db_connection_string
    Jwt__Key                   = var.jwt_key
    Jwt__Issuer                = "TaskFlow"
    Jwt__Audience              = "TaskFlowClients"
  }
}

output "api_url" {
  value = "https://${azurerm_linux_web_app.api.default_hostname}"
}
