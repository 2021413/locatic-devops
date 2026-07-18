# Kubernetes — manifests Kustomize

## Rôle

`k8s/` décrit le déploiement de Locatic en YAML brut, organisé en **Kustomize** :
une base commune et un overlay par environnement. C'est le **fallback documenté** du
chart Helm ([helm.md](helm.md)), qui reste le chemin nominal utilisé par Ansible.

```
k8s/
├── base/
│   ├── deployment-app.yaml      # App .NET (port 8080, PVC /data, probes, annotations Prometheus)
│   ├── service-app.yaml         # ClusterIP locatic-app 80 → 8080
│   ├── configmap-nginx.yaml     # nginx.conf (proxy_pass http://locatic-app)
│   ├── deployment-nginx.yaml    # Reverse proxy nginx:1.27-alpine
│   ├── service-nginx.yaml       # NodePort 30080 — seul point d'entrée
│   ├── secret-app.yaml          # Template locatic-secrets (aucune valeur réelle)
│   └── kustomization.yaml       # namespace: locatic + liste des ressources
└── overlays/
    └── dev/
        └── kustomization.yaml   # Surcharge du tag d'image
```

## Ressources de la base

| Ressource | Nom | Points notables |
| --- | --- | --- |
| Deployment | `locatic-app` | `replicas: 1`, image `ghcr.io/2021413/locatic`, port 8080, PVC monté sur `/data`, liveness `/health`, readiness `/health/ready`, requests/limits CPU-mémoire, annotations `prometheus.io/*` |
| Service | `locatic-app` | ClusterIP, `80 → 8080` — **jamais exposé à l'extérieur** |
| ConfigMap | `locatic-nginx-conf` | `nginx.conf` avec `proxy_pass http://locatic-app` et en-têtes `X-Forwarded-*` |
| Deployment | `locatic-nginx` | `nginx:1.27-alpine`, ConfigMap montée en `subPath` sur `/etc/nginx/nginx.conf` |
| Service | `locatic-nginx` | **NodePort 30080** — seul point d'entrée du système |
| Secret | `locatic-secrets` | Template versionné (`EXAMPLE_KEY: changeme`), consommé en `envFrom` avec `optional: true` |

Le namespace `locatic` est posé par le `kustomization.yaml` de la base ; il est **créé par
Terraform**, pas par ces manifests.

## Choix notables

- **`replicas: 1` pour l'app** : SQLite ne supporte pas les écritures concurrentes de
  plusieurs pods sur le même fichier, et le PVC est en accès RWO. La montée en réplicas
  supposerait de changer de base de données.
- **L'app n'est jamais exposée** : Service ClusterIP uniquement ; tout le trafic externe
  passe par Nginx (NodePort 30080). Nginx transmet `Host`, `X-Real-IP`, `X-Forwarded-For`
  et `X-Forwarded-Proto`, que l'app consomme via le middleware `ForwardedHeaders`.
- **PVC référencé, pas créé** : `deployment-app.yaml` référence `locatic-sqlite-pvc`
  provisionné par Terraform (lot B). Supprimer/réappliquer les manifests ne touche donc
  jamais les données.
- **Secret en template** : `secret-app.yaml` ne contient aucune valeur réelle. En local,
  remplacer les valeurs via `kubectl create secret` ou un overlay non versionné avant apply.
- **Probes distinctes** : liveness sur `/health` (check `self`), readiness sur
  `/health/ready` (teste réellement la base via EF Core) — un pod dont la base est
  inaccessible sort du Service sans être redémarré en boucle.
- **Annotations Prometheus** sur le pod app (`prometheus.io/scrape: "true"`,
  `prometheus.io/port: "8080"`, `prometheus.io/path: "/metrics"`) : découverte automatique
  par le Prometheus de `monitoring/` (voir [monitoring.md](monitoring.md)).

## Overlay `dev`

`k8s/overlays/dev/kustomization.yaml` référence la base et surcharge uniquement le tag
d'image :

```yaml
images:
  - name: ghcr.io/2021413/locatic
    newTag: latest
```

### Mettre à jour le tag d'image

Après publication d'une image par la CI (voir [ci-cd.md](ci-cd.md)), remplacer `newTag`
par le SHA court publié :

```yaml
images:
  - name: ghcr.io/2021413/locatic
    newTag: sha-xxxxxxx
```

ou en ligne de commande :

```bash
cd k8s/overlays/dev
kustomize edit set image ghcr.io/2021413/locatic:sha-xxxxxxx
```

## Commandes

```bash
# Prévisualiser le rendu
kubectl kustomize k8s/overlays/dev

# Appliquer (namespace et PVC déjà créés par Terraform)
kubectl apply -k k8s/overlays/dev

# Vérifier
kubectl get pods,svc -n locatic
kubectl rollout status deployment/locatic-app -n locatic

# Accéder à l'app (via Nginx uniquement)
minikube service locatic-nginx -n locatic --url
```

> Ne pas mélanger les deux modes de déploiement : si la release Helm `locatic` est déjà
> installée (chemin nominal via Ansible), ne pas appliquer ces manifests par-dessus —
> ils créent des ressources aux mêmes noms.
