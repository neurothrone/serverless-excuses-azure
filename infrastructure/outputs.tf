output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "function_app_name" {
  value = azurerm_linux_function_app.func.name
}

output "function_app_url" {
  value = "https://${azurerm_linux_function_app.func.default_hostname}/api"
}

output "static_site_name" {
  value = azurerm_static_web_app.frontend.name
}

output "static_site_default_hostname" {
  value = azurerm_static_web_app.frontend.default_host_name
}
