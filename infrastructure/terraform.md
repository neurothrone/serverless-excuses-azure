# Terraform

## Commands

### Setup

```shell
terraform init
```

### Create

```shell
terraform plan -var-file=env.tfvars -out excuses.tfplan
terraform show excuses.tfplan
```

```shell
terraform apply -var-file=env.tfvars excuses.tfplan
```

### Destroy

```shell
terraform destroy -var-file=env.tfvars
```

```shell
az functionapp config appsettings list \
  --name serverless-excuses-func \
  --resource-group rg-serverless-excuses
```