# Orchestration locale — Ansible

Playbooks du lot B : déploiement et rollback de Locatic sur minikube via la
release Helm `locatic`, à partir des outputs Terraform (namespace + PVC).
Documentation détaillée : [../docs/ansible.md](../docs/ansible.md).

## Prérequis

- `minikube` démarré (`minikube start`) et `kubectl` pointant dessus
- `terraform` appliqué dans `../infra/terraform` (`terraform apply -var-file=example.tfvars`)
- `helm` ≥ 3 et `ansible` (ansible-core ≥ 2.13) installés

## Commandes

```bash
cd ansible

# Déploiement complet (image ghcr.io/2021413/locatic:latest par défaut)
ansible-playbook deploy.yml

# Déployer un tag précis (recommandé en démo)
ansible-playbook deploy.yml -e image_tag=sha-abc1234

# Simulation (les étapes de lecture s'exécutent, helm/kubectl sont sautés)
ansible-playbook deploy.yml --check

# Rollback vers la révision N (obligatoire, voir `helm history locatic -n locatic`)
ansible-playbook rollback.yml -e revision=2
```

## Variables surchargeables (`-e var=valeur`)

Défauts dans [`group_vars/all.yml`](group_vars/all.yml) :

| Variable | Défaut | Rôle |
| --- | --- | --- |
| `terraform_dir` | `../infra/terraform` | Dossier des outputs Terraform |
| `chart_path` | `../helm/locatic` | Chart Helm à déployer |
| `helm_release` | `locatic` | Nom de la release Helm |
| `image_tag` | `latest` | Tag de l'image `ghcr.io/2021413/locatic` |
| `helm_timeout` | `5m` | Timeout de `helm --wait` |
| `rollout_timeout` | `120s` | Timeout des `kubectl rollout status` |
| `nginx_node_port` | `30080` | NodePort du service Nginx |
| `revision` | _(aucun)_ | **Obligatoire** pour `rollback.yml` |

## Fallback sans Helm

Helm est le chemin nominal. Si Helm est indisponible, les manifests bruts
restent applicables avec Kustomize (mêmes objets, valeurs figées de l'overlay dev) :

```bash
kubectl apply -k k8s/overlays/dev
```

Ce fallback ne gère ni l'historique de révisions ni le rollback `helm rollback`.
