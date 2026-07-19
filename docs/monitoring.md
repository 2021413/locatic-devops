# Monitoring — Prometheus, Grafana, alertes

## Architecture de la stack

Tout est déployé dans le namespace `locatic` via Kustomize :

```bash
kubectl apply -k monitoring/
```

```
monitoring/
├── kustomization.yaml               # namespace locatic + configMapGenerator du dashboard
├── prometheus/
│   ├── prometheus-rbac.yaml         # ServiceAccount + RBAC (découverte des pods)
│   ├── prometheus-config.yaml       # prometheus.yml (scrape_interval 15s)
│   ├── prometheus-deployment.yaml
│   ├── prometheus-service.yaml      # NodePort 30090
│   └── kube-state-metrics.yaml      # État pods / deployments / PVC
└── grafana/
    ├── grafana-datasource-configmap.yaml         # Datasource Prometheus provisionnée
    ├── grafana-dashboard-provider-configmap.yaml # Provider de dashboards
    ├── grafana-deployment.yaml
    ├── grafana-service.yaml         # NodePort 30030
    └── dashboards/locatic-overview.json          # Dashboard "Locatic — Overview"
```

Flux : Prometheus scrape **l'app** (`/metrics:8080`, prometheus-net), **Nginx** (sidecar
`nginx-prometheus-exporter` `:9113`, alimenté par `stub_status`) et
**kube-state-metrics** (état des pods, deployments et PVC). Grafana est provisionné
automatiquement (datasource + dashboard via ConfigMaps) : aucun clic de configuration.

## Découverte par annotations

Prometheus ne liste aucune cible applicative en dur : le job `kubernetes-pods`
(`kubernetes_sd_configs`, rôle `pod`, namespace `locatic`) garde les pods portant
l'annotation `prometheus.io/scrape: "true"` et lit le port/chemin dans les annotations :

| Annotation | App | Nginx (exporter) |
| --- | --- | --- |
| `prometheus.io/scrape` | `"true"` | `"true"` |
| `prometheus.io/port` | `8080` | `9113` |
| `prometheus.io/path` | `/metrics` | `/metrics` |

Ces annotations sont posées par le chart Helm (pilotées par `monitoring.enabled`) et par
les manifests `k8s/base`. Tout nouveau service annoté est découvert sans modifier la
configuration Prometheus. `kube-state-metrics` est scrappé via un job statique
(`kube-state-metrics.locatic.svc.cluster.local:8080`).

## Alertes

Règles Prometheus (bonus obligatoire), évaluées par Prometheus et routées vers
**Alertmanager** (port `9093`, receiver par défaut sans intégration externe — les alertes
se constatent dans les UIs Prometheus/Alertmanager) :

| Alerte | Condition / seuil | Sévérité |
| --- | --- | --- |
| `LocaticAppDown` | App injoignable (`up == 0` sur la cible app), `for: 1m` | critical |
| `LocaticNginxDown` | Exporter Nginx injoignable (`up == 0` sur la cible nginx) | critical |
| `LocaticPvcAlmostFull` | PVC `locatic-sqlite-pvc` rempli à plus de **85 %** (`kubelet_volume_stats_*`) | warning |
| `LocaticMonitoringTargetDown` | Une cible de scrape quelconque est down | — |

## Procédure de test des alertes (à capturer)

Démonstration de `LocaticAppDown` :

```bash
# 1. Couper l'app
kubectl scale deployment/locatic-app -n locatic --replicas=0

# 2. Attendre ~1-2 min (for: 1m), vérifier l'état FIRING
#    Prometheus → onglet "Alerts" : LocaticAppDown passe PENDING puis FIRING
#    (LocaticMonitoringTargetDown se déclenche aussi, la cible app étant down)

# 3. Capturer : docs/preuves/ (alerte FIRING + dashboard Grafana "App — up" à 0)

# 4. Rétablir
kubectl scale deployment/locatic-app -n locatic --replicas=1
kubectl rollout status deployment/locatic-app -n locatic

# 5. Vérifier le retour au vert (alerte résolue, panel "App — up" à 1) et capturer
```

Même principe pour `LocaticNginxDown` avec `deployment/locatic-nginx`.

## Accès aux interfaces

| Interface | Service | Accès |
| --- | --- | --- |
| Prometheus | `prometheus` (NodePort 30090) | `minikube service prometheus -n locatic --url` |
| Alertmanager | port 9093 | `kubectl port-forward -n locatic svc/alertmanager 9093:9093` |
| Grafana | `grafana` (NodePort 30030) | `minikube service grafana -n locatic --url` |

Alternative port-forward :

```bash
kubectl port-forward -n locatic svc/prometheus 9090:9090
kubectl port-forward -n locatic svc/grafana 3000:3000
```

Grafana : identifiants par défaut `admin` / `admin` (à changer à la première connexion ;
aucun secret réel versionné). Prometheus et Grafana sont des outils d'observabilité : leur
exposition en NodePort ne contrevient pas à la règle « l'app uniquement via Nginx ».

## Dashboard comparatif « Locatic — Overview »

Dashboard provisionné (`monitoring/grafana/dashboards/locatic-overview.json`, ConfigMap
générée par Kustomize), montrant en un coup d'œil l'état des quatre briques :

| Panel | Source | Ce qu'il montre |
| --- | --- | --- |
| App — up | `up` (cible app) | Disponibilité de l'application |
| App — requêtes HTTP (rate 5m) | prometheus-net (`http_requests_*`) | Trafic applicatif |
| Nginx — up | `up` (cible exporter) | Disponibilité du reverse proxy |
| Pods — état (namespace locatic) | kube-state-metrics | Santé de tous les pods du namespace |
| Stockage — PVC % utilisé | `kubelet_volume_stats_*` | Remplissage de `locatic-sqlite-pvc` (lié à l'alerte 85 %) |
| Monitoring — cibles up | `up` par job | Autosurveillance de la chaîne de scrape |

Chaque service important (Nginx, App, Stockage, Monitoring) a ainsi au moins un
indicateur visible dans Grafana.
