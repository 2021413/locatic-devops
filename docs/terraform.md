# Terraform — infrastructure locale minikube

## Rôle

Terraform prépare l’infra Kubernetes locale **avant** le déploiement applicatif (Helm / Ansible) :
- crée le namespace `locatic`
- crée le PVC `locatic-sqlite-pvc` pour la persistance SQLite
- expose des outputs JSON consommés par Ansible

Terraform **ne déploie pas** l’application ni Nginx. Ces étapes sont orchestrées par Ansible.

## Ressources gérées

| Ressource | Nom | Fichier |
| --- | --- | --- |
| Namespace | `locatic` | `infra/terraform/main.tf` |
| PersistentVolumeClaim | `locatic-sqlite-pvc` | `infra/terraform/main.tf` |

Provider : `hashicorp/kubernetes` (kubeconfig local, typiquement minikube).

## Variables

Voir `infra/terraform/variables.tf` et `infra/terraform/example.tfvars`.

| Variable | Défaut | Description |
| --- | --- | --- |
| `namespace` | `locatic` | Namespace applicatif |
| `pvc_name` | `locatic-sqlite-pvc` | PVC SQLite |
| `pvc_size` | `1Gi` | Taille du volume |
| `storage_class_name` | `standard` | StorageClass minikube |
| `kubeconfig_path` | `~/.kube/config` | Kubeconfig |
| `kubeconfig_context` | _(vide)_ | Contexte (ex. `minikube`) |

## Outputs

| Output | Usage |
| --- | --- |
| `namespace` | Namespace cible pour Helm / kubectl |
| `pvc_name` | PVC à monter sur `/data` |
| `pvc_storage` | Taille demandée |
| `ansible_vars` | Objet JSON prêt pour Ansible |

```bash
terraform output -json ansible_vars
```

## État

Backend **local** (`terraform.tfstate` dans `infra/terraform/`).  
Les fichiers `*.tfstate` et `*.tfvars` (sauf `example.tfvars`) sont dans `.gitignore`.

## Procédure

1. `minikube start` (addon `metrics-server` recommandé)
2. `cd infra/terraform && terraform init`
3. `terraform plan -var-file=example.tfvars`
4. `terraform apply -var-file=example.tfvars`
5. Enchaîner avec Ansible (`ansible-playbook deploy.yml`)
