# Helm — chart `locatic`

## Rôle

`helm/locatic/` packe le déploiement complet (App + Nginx + Secret) en un chart
configurable. C'est le **chemin nominal de déploiement** : le playbook Ansible
(`ansible/deploy.yml`) installe/à jour la release `locatic` via
`helm upgrade --install` (bonus obligatoire « chart Helm utilisé par Ansible »).
Les manifests bruts de `k8s/` restent le fallback ([kubernetes.md](kubernetes.md)).

## Structure du chart

```
helm/locatic/
├── Chart.yaml                       # apiVersion v2, name locatic, version 0.1.0
├── values.yaml                      # Valeurs par défaut (contrats du projet)
└── templates/
    ├── deployment-app.yaml          # App : image/tag paramétrés, PVC, probes, annotations Prometheus
    ├── service-app.yaml             # ClusterIP locatic-app
    ├── configmap-nginx.yaml         # nginx.conf + endpoint stub_status (si exporter activé)
    ├── deployment-nginx.yaml        # Nginx + sidecar nginx-prometheus-exporter
    ├── service-nginx.yaml           # NodePort 30080 (seul point d'entrée)
    └── secret-app.yaml              # Template locatic-secrets
```

## Values principales

Extrait de `helm/locatic/values.yaml` (valeurs par défaut alignées sur les contrats) :

| Clé | Défaut | Description |
| --- | --- | --- |
| `image.repository` | `ghcr.io/2021413/locatic` | Image publiée par la CI |
| `image.tag` | `latest` | Tag déployé — surchargé par Ansible (`sha-xxxxxxx`) |
| `image.pullPolicy` | `IfNotPresent` | Politique de pull |
| `replicaCount` | `1` | Fixé à 1 : SQLite ne supporte pas les écritures concurrentes multi-pods |
| `service.type` / `service.port` | `ClusterIP` / `80` | Service interne `locatic-app` |
| `nginx.enabled` | `true` | Déploie le reverse proxy |
| `nginx.image.repository` / `tag` | `nginx` / `1.27-alpine` | Image Nginx |
| `nginx.service.type` | `NodePort` | Point d'entrée unique |
| `nginx.service.nodePort` | `30080` | Port exposé sur le nœud minikube |
| `nginx.exporter.enabled` | `true` | Sidecar `nginx-prometheus-exporter` (métriques Nginx) |
| `nginx.exporter.image.*` | `nginx/nginx-prometheus-exporter:0.11.0` | Image de l'exporter |
| `nginx.exporter.port` | `9113` | Port `/metrics` de l'exporter |
| `persistence.enabled` | `true` | Monte le volume de données |
| `persistence.existingClaim` | `locatic-sqlite-pvc` | PVC **créé par Terraform**, seulement référencé |
| `persistence.mountPath` | `/data` | Point de montage (base `/data/locatic.db`) |
| `env.ASPNETCORE_URLS` | `http://+:8080` | Port d'écoute de l'app |
| `env.connectionString` | `Data Source=/data/locatic.db` | Chaîne de connexion SQLite |
| `resources.*` | requests 100m/128Mi, limits 500m/256Mi | Ressources du conteneur app |
| `probes.liveness.path` | `/health` | Liveness probe |
| `probes.readiness.path` | `/health/ready` | Readiness probe (teste la base) |
| `secret.create` / `secret.name` | `true` / `locatic-secrets` | Secret template (aucune valeur réelle) |
| `monitoring.enabled` | `true` | Bascule les annotations `prometheus.io/scrape` sur App et Nginx |

Détail notable : quand `nginx.exporter.enabled` est vrai, la ConfigMap Nginx ajoute un
serveur interne `127.0.0.1:8081/stub_status` scrappé par le sidecar, qui expose les
métriques au format Prometheus sur le port `9113` du pod (annoté `prometheus.io/*`).

## Commandes

```bash
# Vérifier le chart
helm lint helm/locatic

# Prévisualiser le rendu (relire avant tout upgrade)
helm template locatic helm/locatic -n locatic

# Installer / mettre à jour la release (ce que fait Ansible)
helm upgrade --install locatic helm/locatic \
  -n locatic \
  --set image.tag=sha-xxxxxxx \
  --wait

# État de la release et historique des révisions
helm status locatic -n locatic
helm history locatic -n locatic
```

Le namespace `locatic` et le PVC `locatic-sqlite-pvc` doivent exister avant
l'installation (créés par `terraform apply`, voir [terraform.md](terraform.md)).

## Lien avec Ansible (chemin nominal)

Le playbook `ansible/deploy.yml` ([ansible.md](ansible.md)) :

1. vérifie les prérequis (minikube, kubectl, helm, terraform) ;
2. lit `terraform output -json` (namespace, nom du PVC) ;
3. exécute `helm upgrade --install locatic helm/locatic -n locatic --set image.tag=... --wait` ;
4. attend le `rollout status` puis affiche l'URL d'accès.

Chaque exécution crée une **révision Helm**, ce qui rend le rollback trivial :
`helm rollback locatic <revision>` ou `ansible-playbook rollback.yml -e revision=N`
(procédure complète dans [exploitation.md](exploitation.md)).
