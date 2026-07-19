# Infrastructure locale — Terraform

Provisionne sur **minikube** :
- le namespace `locatic`
- le PVC `locatic-sqlite-pvc` (SQLite)
- des **outputs** consommés ensuite par Ansible

## Prérequis

- minikube démarré (`minikube start`)
- `kubectl` configuré sur le cluster
- Terraform >= 1.5

## Usage

```bash
cd infra/terraform
terraform init
terraform fmt
terraform validate
terraform plan -var-file=example.tfvars
terraform apply -var-file=example.tfvars
terraform output -json
```

Outputs utiles pour Ansible : `namespace`, `pvc_name`, `ansible_vars`.

**Ne jamais commiter** `*.tfstate`, `terraform.tfvars`.

Détails : [docs/terraform.md](../../docs/terraform.md).
