# Monitoring — Prometheus, Alertmanager & Grafana

Scrape App/Nginx/pods/PVC, dashboard comparatif et alertes testées (Lot C).
Voir [../docs/monitoring.md](../docs/monitoring.md).

## Composants

- **Prometheus** (`prometheus/`) : scrape l'app et Nginx (annotations `prometheus.io/*`),
  kube-state-metrics (état des pods/PVC) et le kubelet de chaque nœud
  (`kubelet_volume_stats_*` pour l'occupation réelle des PVC). Évalue les règles
  d'alerte et les pousse vers Alertmanager.
- **Alertmanager** (`alertmanager/`) : reçoit les alertes (UI sur le port 9093).
  Receiver `default` sans notification externe (démo locale) — voir le commentaire
  dans `alertmanager/alertmanager-config.yaml` pour brancher Slack ou e-mail.
- **Grafana** (`grafana/`) : datasource Prometheus et dashboard « Locatic — Overview »
  provisionnés automatiquement. Le dashboard couvre en un coup d'œil :
  **App** (up + requêtes HTTP), **Nginx** (up), **Stockage** (% utilisé du PVC SQLite,
  seuil rouge à 85 %) et **Monitoring** (table des cibles `up` par job).

Déploiement : `kubectl apply -k monitoring/`

## Alertes (groupe `locatic`)

| Alerte | Condition | Durée | Sévérité |
|---|---|---|---|
| `LocaticAppDown` | aucune cible `locatic-app` up (`absent(up{pod=~"locatic-app.*"}) or up == 0`) | 1m | critical |
| `LocaticNginxDown` | aucune cible `locatic-nginx` up | 1m | critical |
| `LocaticPvcAlmostFull` | `kubelet_volume_stats_used_bytes / kubelet_volume_stats_capacity_bytes > 0.85` sur `locatic-sqlite-pvc` | 5m | warning |
| `LocaticMonitoringTargetDown` | `kube-state-metrics` down | 1m | warning |

## Tester les alertes

Pré-requis : la stack est déployée et les UIs sont accessibles, par exemple :

```bash
kubectl port-forward svc/prometheus 9090:9090 -n locatic     # http://localhost:9090/alerts
kubectl port-forward svc/alertmanager 9093:9093 -n locatic   # http://localhost:9093
```

Pour chaque test : déclencher la panne, attendre le passage PENDING → FIRING dans
Prometheus (onglet *Alerts*) et l'arrivée de l'alerte dans Alertmanager, **capturer
une preuve** (copie d'écran dans `docs/preuves/`), puis remettre en état.

### 1. `LocaticAppDown`

```bash
kubectl scale deployment locatic-app --replicas=0 -n locatic
# Attendre ~1-2 min → LocaticAppDown FIRING (Prometheus + Alertmanager).
# Capture → docs/preuves/alerte-app-down.png
kubectl scale deployment locatic-app --replicas=1 -n locatic
# Vérifier le retour au vert (alerte résolue).
```

### 2. `LocaticNginxDown`

```bash
kubectl scale deployment locatic-nginx --replicas=0 -n locatic
# Attendre ~1-2 min → LocaticNginxDown FIRING.
# Capture → docs/preuves/alerte-nginx-down.png
kubectl scale deployment locatic-nginx --replicas=1 -n locatic
```

### 3. `LocaticPvcAlmostFull`

Remplir le volume à plus de 85 % depuis le pod de l'app (le PVC est monté sur `/data`) :

```bash
# Écrire un gros fichier de bourrage (ajuster count selon la taille du PVC,
# ex. PVC de 1 Gi → ~900 Mo) :
kubectl exec deploy/locatic-app -n locatic -- \
  dd if=/dev/zero of=/data/bourrage.bin bs=1M count=900
# Attendre ~5-6 min (for: 5m + scrape du kubelet) → LocaticPvcAlmostFull FIRING.
# Capture → docs/preuves/alerte-pvc-plein.png
# Nettoyer :
kubectl exec deploy/locatic-app -n locatic -- rm /data/bourrage.bin
```

Le panel « Stockage — PVC SQLite (% utilisé) » du dashboard Grafana doit passer
au rouge (> 85 %) pendant le test.

### 4. `LocaticMonitoringTargetDown`

```bash
kubectl scale deployment kube-state-metrics --replicas=0 -n locatic
# Attendre ~1-2 min → LocaticMonitoringTargetDown FIRING.
# Capture → docs/preuves/alerte-ksm-down.png
kubectl scale deployment kube-state-metrics --replicas=1 -n locatic
```

## Recharger la config sans redémarrer

Prometheus est lancé avec `--web.enable-lifecycle` : après modification d'une
ConfigMap (config ou règles), une fois la ConfigMap propagée dans le pod :

```bash
kubectl exec deploy/prometheus -n locatic -- \
  wget -qO- --post-data='' http://localhost:9090/-/reload
```
