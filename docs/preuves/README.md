# Preuves

Captures, extraits de logs et exports démontrant chaque point de la grille d'évaluation.
Nommer les fichiers par thème et numéroter les séquences (ex. `rollback-1-avant.png`,
`rollback-2-apres.png`).

## Checklist des captures attendues

### CI/CD et GitHub (grille : bonnes pratiques GitHub, pipeline, image)

- [ ] **Pipeline vert sur `main`** : les 4 jobs `build-test` → `docker-build` → `scan` → `publish` verts (page Actions).
- [ ] **Détail du job `build-test`** montrant les tests xUnit réellement exécutés (`dotnet test`).
- [ ] **Job `scan`** : Gitleaks + Trivy passés (politique HIGH/CRITICAL bloquante visible).
- [ ] **Image publiée sur GHCR** : page Packages avec les tags `latest` et `sha-xxxxxxx`.
- [ ] **PR mergée avec checks requis** : PR de démonstration montrant revue + checks verts obligatoires + protection de `main` (pas de push direct).

### Infra et déploiement (grille : Terraform, Ansible, K8s/Nginx)

- [ ] **`terraform apply` OK** : sortie avec créations namespace `locatic` + PVC `locatic-sqlite-pvc`, et `terraform output -json ansible_vars`.
- [ ] **`ansible-playbook deploy.yml` OK** : sortie complète (prérequis, outputs Terraform lus, `helm upgrade --install`, rollout, URL affichée).
- [ ] **Pods et services** : `kubectl get pods,svc,pvc -n locatic` — tout Ready/Bound, app en ClusterIP, nginx en NodePort 30080.
- [ ] **App accessible via Nginx** : navigateur sur l'URL `minikube service locatic-nginx -n locatic --url` (l'app n'est pas accessible autrement).
- [ ] **Persistance SQLite** : donnée créée dans l'UI → `kubectl delete pod` du pod app → pod recréé → donnée toujours présente (capture avant/après).

### Monitoring (grille : Prometheus + Grafana, bonus alertes + dashboard)

- [ ] **Cibles Prometheus up** : page Targets (app :8080, nginx-exporter :9113, kube-state-metrics).
- [ ] **Dashboard Grafana « Locatic — Overview »** : panels App / Nginx / Pods / Stockage PVC / cibles monitoring au vert.
- [ ] **Alerte FIRING testée** : `kubectl scale deployment/locatic-app --replicas=0` → capture de `LocaticAppDown` en FIRING (Prometheus/Alertmanager) + panel App à 0 dans Grafana → rescale → capture du retour au vert.

### Rollback (bonus obligatoire)

- [ ] **Avant** : `helm history locatic -n locatic` + image courante du deployment.
- [ ] **Exécution** : sortie de `ansible-playbook rollback.yml -e revision=N` (ou `helm rollback locatic N`).
- [ ] **Après** : `helm history` (ligne "Rollback to N"), pods Ready, image revenue à la révision précédente, app répondant via Nginx.

Procédures détaillées : [../exploitation.md](../exploitation.md) (rollback, persistance)
et [../monitoring.md](../monitoring.md) (test des alertes).
