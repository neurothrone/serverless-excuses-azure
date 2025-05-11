# Terraform

## Setup

### Create

```shell
cd infrastructure
terraform init
terraform plan -var-file=env.tfvars
terraform apply -var-file=env.tfvars
```

### Destroy

```shell
cd infrastructure
terraform destroy -var-file=env.tfvars
```