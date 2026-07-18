# Architecture

## Vue d'ensemble

Le projet Locatic reprend une application de location de voitures (ASP.NET Core 9 MVC,
EF Core, SQLite) et l'entoure d'une chaîne DevOps complète : CI GitHub Actions qui publie
une image sur GHCR, provisioning **Terraform**, orchestration **Ansible**, déploiement
**Helm** sur **minikube** derrière un reverse proxy **Nginx**, supervision
**Prometheus + Grafana**.

## Schéma des flux

```
                        ┌───────────────────────── GitHub ─────────────────────────┐
                        │  PR → CI (build-test → docker-build → scan → publish)    │
                        │                    push sur main uniquement              │
                        └───────────────┬──────────────────────────────────────────┘
                                        │ image ghcr.io/2021413/locatic:{latest, sha-xxxxxxx}
                                        ▼
   Poste local :  terraform apply ──▶ ansible-playbook deploy.yml ──▶ helm upgrade --install locatic
                  (namespace + PVC)   (lit les outputs Terraform)     (release "locatic", ns locatic)
                                        │
────────────────────────────────────────┼──────────── cluster minikube (namespace locatic) ────────
                                        ▼
 Utilisateur ──▶ Service locatic-nginx ──▶ Pod Nginx ──▶ Service locatic-app ──▶ Pod locatic-app
                 (NodePort 30080,          (reverse       (ClusterIP 80→8080)     (ASP.NET Core :8080)
                  seul point d'entrée)      proxy)                                      │
                                                                                        ▼
                                                                              PVC locatic-sqlite-pvc
                                                                              monté sur /data
                                                                              (/data/locatic.db)

 Prometheus ──scrape──▶ App (/metrics :8080), Nginx (exporter :9113), kube-state-metrics
     │
     └──datasource──▶ Grafana (dashboard "Locatic — Overview")
```

Points structurants :

- **L'app n'est jamais exposée directement** : le Service `locatic-app` est en ClusterIP,
  seul le Service `locatic-nginx` (NodePort `30080`) est accessible de l'extérieur.
- **La CI s'arrête à la publication de l'image** : aucun job GitHub ne touche minikube.
  Le déploiement est déclenché localement (Terraform puis Ansible).
- **SQLite impose `replicas: 1`** pour le pod applicatif (pas d'écritures concurrentes
  multi-pods sur un même fichier, PVC en accès RWO).

## Contrats d'interface

Valeurs figées entre les trois lots (voir `PLAN-EQUIPE.md` §2) — personne ne les change
sans prévenir les autres :

| Élément | Valeur convenue | Utilisé par |
| --- | --- | --- |
| Registry / image | `ghcr.io/2021413/locatic` | A (push), C (deploy) |
| Tags d'image | SHA court (`sha-xxxxxxx`) + `latest` sur `main` | A, B, C |
| Namespace Kubernetes | `locatic` | B (crée via Terraform), C (déploie dans) |
| Port HTTP de l'app | `8080` (`ASPNETCORE_URLS=http://+:8080`) | A, C |
| Endpoints santé | `/health` (liveness) et `/health/ready` (readiness, teste la base) | A, C |
| Endpoint métriques | `/metrics` (format Prometheus, prometheus-net) | A, C |
| Chemin SQLite | `/data/locatic.db` via `ConnectionStrings__LocaticDb=Data Source=/data/locatic.db` | A, B, C |
| PVC SQLite | `locatic-sqlite-pvc`, monté sur `/data` | B (crée), C (monte) |
| Service app (interne) | `locatic-app`, ClusterIP `80 → 8080` | C |
| Point d'entrée | Service `locatic-nginx`, NodePort `30080` | C |
| Secret applicatif | `locatic-secrets` (template versionné, valeurs réelles hors Git) | A, B, C |
| Nom de release Helm | `locatic` | B, C |

## Qui produit quoi, qui consomme quoi

| Composant | Produit | Consommé par |
| --- | --- | --- |
| CI GitHub Actions (`.github/workflows/ci.yml`) | Image `ghcr.io/2021413/locatic` (`latest` + `sha-xxxxxxx`) | Chart Helm (`image.tag`), overlay Kustomize |
| Terraform (`infra/terraform/`) | Namespace `locatic`, PVC `locatic-sqlite-pvc`, output `ansible_vars` | Ansible (`terraform output -json`) ; manifests/chart référencent le PVC sans le créer |
| Ansible (`ansible/deploy.yml`) | Release Helm `locatic` déployée (`helm upgrade --install`) | Exploitation (rollback via `ansible/rollback.yml`) |
| Chart Helm (`helm/locatic/`) | Deployments App + Nginx, Services, ConfigMap Nginx, Secret | Ansible (chemin nominal de déploiement) |
| Manifests Kustomize (`k8s/`) | Mêmes ressources en YAML brut | Fallback documenté `kubectl apply -k` |
| App (`app/`) | Endpoints `/health`, `/health/ready`, `/metrics` sur le port 8080 | Probes K8s, HEALTHCHECK Docker, Prometheus |
| Monitoring (`monitoring/`) | Prometheus, kube-state-metrics, Grafana provisionné | Supervision + alertes (démonstration) |

## Chaîne de déploiement (chemin nominal)

1. **Terraform** crée le socle : namespace `locatic` + PVC `locatic-sqlite-pvc`
   (voir [terraform.md](terraform.md)).
2. **Ansible** (`ansible/deploy.yml`) vérifie les prérequis, lit
   `terraform output -json`, puis lance
   `helm upgrade --install locatic helm/locatic -n locatic --set image.tag=... --wait`
   (voir [ansible.md](ansible.md)).
3. **Helm** rend les manifests : App (PVC monté sur `/data`, probes, annotations
   Prometheus) + Nginx (ConfigMap `proxy_pass http://locatic-app`, NodePort 30080)
   (voir [helm.md](helm.md)).
4. Le **monitoring** est déployé à part : `kubectl apply -k monitoring/`
   (voir [monitoring.md](monitoring.md)).

## Documentation associée

| Doc | Sujet |
| --- | --- |
| [ci-cd.md](ci-cd.md) | Pipeline, protection de `main`, publication GHCR |
| [terraform.md](terraform.md) | Namespace, PVC, outputs |
| [ansible.md](ansible.md) | Playbooks deploy / rollback |
| [kubernetes.md](kubernetes.md) | Manifests base + overlay dev |
| [helm.md](helm.md) | Chart, values, release |
| [monitoring.md](monitoring.md) | Prometheus, Grafana, alertes |
| [exploitation.md](exploitation.md) | Runbook, rollback, incidents |
| [deploiement-local.md](deploiement-local.md) | Guide bout en bout |
