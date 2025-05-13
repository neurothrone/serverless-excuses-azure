# Serverless Excuses

A fully serverless excuse generator using Azure Functions, Cosmos DB, and Azure Static Web Apps.

---

## Setup & Deployment

### 1. Provision Infrastructure

From the root folder:

```bash
cd infrastructure

terraform init
terraform plan -var-file=env.tfvars -out excuses.tfplan
terraform apply -var-file=env.tfvars
```

After applying, run:

```bash
terraform output
```

Copy the following outputs for use as GitHub secrets:

- `function_app_name` → for GitHub Functions workflow
- `function_app_url` → for the frontend API URL
- `resource_group_name` → for GitHub Functions workflow
- `static_web_app_url` → for testing or frontend CORS

---

### 2. Configure GitHub Secrets

#### In **Functions Repo**

- `AZURE_FUNCTIONAPP_NAME` → From Terraform output
- `AZURE_RESOURCE_GROUP` → From Terraform output
- `AZURE_CREDENTIALS` → From Azure CLI (copy entire JSON to value in Secret):

```shell
az ad sp create-for-rbac \
  --name "github-ci-sp-serverless-excuses" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<resource-group-name> \
  --sdk-auth
```

#### In **Frontend Repo**

- `VITE_BASE_API_URL` → Terraform `function_app_url` + `/api`
- `VITE_FUNCTION_KEY` → From Azure Portal → Function App → Keys → default
- `AZURE_STATIC_WEB_APPS_API_TOKEN` → From Azure CLI:

```bash
az staticwebapp secrets list \
  --name <static-site-name> \
  --resource-group <resource-group-name> \
  --query "properties.apiKey" \
  --output tsv
```

---

### 3. Trigger GitHub Workflows

- Go to each repo's **Actions** tab.
- Trigger the deployment workflows manually the first time.

---

### 4. Verify Deployments

- Azure Function App should be running all APIs
- Azure Static Web App should load the React frontend
- CORS is handled automatically
