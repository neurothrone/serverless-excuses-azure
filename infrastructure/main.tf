provider "azurerm" {
  features {}

  subscription_id = var.subscription_id
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

resource "azurerm_cosmosdb_account" "cosmos" {
  name                = var.cosmosdb_account_name
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  capabilities {
    name = "EnableServerless"
  }

  depends_on = [
    azurerm_resource_group.rg
  ]
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = var.cosmosdb_database_name
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name

  depends_on = [
    azurerm_cosmosdb_account.cosmos
  ]
}

resource "azurerm_cosmosdb_sql_container" "container" {
  name                = var.cosmosdb_container_name
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_paths = [
    "/id"
  ]

  depends_on = [
    azurerm_cosmosdb_sql_database.db
  ]
}

resource "azurerm_service_plan" "plan" {
  name                = var.service_plan_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "Y1"

  depends_on = [azurerm_cosmosdb_account.cosmos]
}

resource "azurerm_storage_account" "storage" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  depends_on = [azurerm_cosmosdb_account.cosmos]
}

resource "azurerm_linux_function_app" "func" {
  name                        = var.function_app_name
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  service_plan_id             = azurerm_service_plan.plan.id
  storage_account_name        = azurerm_storage_account.storage.name
  storage_account_access_key  = azurerm_storage_account.storage.primary_access_key
  functions_extension_version = "~4"

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet-isolated"
    "CosmosDbEndpoint"         = azurerm_cosmosdb_account.cosmos.endpoint
    "CosmosDbKey"              = azurerm_cosmosdb_account.cosmos.primary_key
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }

    cors {
      allowed_origins = [
        "https://${azurerm_static_web_app.frontend.default_host_name}"
      ]
    }
  }

  depends_on = [azurerm_cosmosdb_account.cosmos]
}

resource "azurerm_static_web_app" "frontend" {
  name                = var.static_site_name
  resource_group_name = var.resource_group_name
  location            = var.static_site_location
  sku_tier            = "Free"
  sku_size            = "Free"

  depends_on = [azurerm_linux_function_app.func]
}
