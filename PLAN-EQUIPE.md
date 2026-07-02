# Plan de réalisation — Équipe de 3

Découpage du mini-projet DevOps **Locatic** en **3 lots** parallélisables, avec les dépendances,
les contrats d'interface partagés et une ligne du temps.

> **Tous les bonus sont obligatoires** sur ce projet : Helm, alertes testées,
> dashboard comparatif, rollback documenté et démontré, pipeline propre découpé en jobs.

---

## 1. Vue d'ensemble

| Lot | Responsable | Périmètre | Livrables principaux |
| --- | --- | --- | --- |
| **A — App, Docker & CI/CD** | Personne A | Adapter l'app Locatic, la conteneuriser, pipeline GitHub Actions, registry, protection de `main` | `app/` (health + tests + config env), `app/Dockerfile`, `.github/workflows/ci.yml`, image GHCR |
| **B — Infra locale (Terraform + Ansible)** | Personne B | minikube, provisioning Terraform, orchestration Ansible, rollback | `infra/terraform/`, `ansible/`, procédure rollback |
| **C — Kubernetes, Nginx, Helm & Monitoring** | Personne C | Manifests K8s, reverse proxy Nginx, chart Helm, Prometheus + Grafana | `k8s/`, `helm/locatic/`, `monitoring/` |

Chaque lot représente **≈ 6-7 points** de la grille (voir §7). Les trois sont interdépendants :
**A produit l'image → C la déploie → B l'orchestre.** D'où l'importance des contrats du §3.

```
  Personne A ──(image ghcr.io/.../locatic:tag)──▶ Personne C ──(manifests / chart Helm)──▶ Personne B
      │                                                │                                       │
      └── CI, PR, secrets GitHub                       └── Nginx + App + PVC + monitoring       └── Terraform (namespace+PVC) + Ansible (apply)
```

---

## 2. Décisions à figer ensemble AVANT de coder (réunion de lancement)

Ces valeurs sont des **contrats** : une fois décidées, personne ne les change sans prévenir les autres.
Les valeurs proposées ci-dessous sont des défauts recommandés — validez-les tels quels si rien ne s'y oppose.

| Élément | Valeur convenue (défaut) | Utilisé par |
| --- | --- | --- |
| Registry / image | `ghcr.io/<org-ou-user>/locatic` | A (push), C (deploy) |
| Tag d'image | SHA court du commit (`sha-xxxxxxx`) + `latest` sur `main` | A, C, B |
| Namespace Kubernetes | `locatic` | B (crée), C (déploie dans) |
| Port HTTP de l'app | `8080` (non privilégié, `ASPNETCORE_URLS=http://+:8080`) | A, C |
| Endpoint santé | `/health` (live) et `/health/ready` (readiness, teste la base) | A, C |
| Endpoint métriques | `/metrics` (format Prometheus) | A, C |
| Chemin SQLite | `/data/locatic.db` via `ConnectionStrings__LocaticDb=Data Source=/data/locatic.db` | A, C, B |
| PVC SQLite | nom `locatic-sqlite-pvc`, monté sur `/data` | B (crée via Terraform), C (monte) |
| Service app (interne) | `locatic-app` (ClusterIP) `80 → 8080` | C |
| Point d'entrée | `nginx` exposé en `NodePort` (ou `minikube service`) | C |
| Secret applicatif | `locatic-secrets` (template versionné, valeurs réelles hors Git) | A, B, C |
| Nom de release Helm | `locatic` | B, C |

> Consigner ces valeurs dans `docs/architecture.md` dès le jour 1.

---

## 3. Lot A — Application, Docker & CI/CD (Personne A)

**Objectif :** livrer une image Docker propre et publiée, produite par un pipeline qui teste réellement l'app.

### A.1 — Adapter l'application (`app/`)
- [ ] Ajouter un **endpoint de santé** : `builder.Services.AddHealthChecks()` + check EF Core, `app.MapHealthChecks("/health")` et `/health/ready`.
- [ ] Rendre la config **par variable d'environnement** (la chaîne de connexion l'est déjà : `ConnectionStrings__LocaticDb`).
- [ ] Rendre le **chemin SQLite configurable** et pointer par défaut sur `/data/locatic.db` en conteneur.
- [ ] Gérer le fonctionnement **derrière un reverse proxy** : `ForwardedHeaders` + désactiver `UseHttpsRedirection` en conteneur (Nginx termine le HTTP).
- [ ] Exposer les **métriques Prometheus** (`prometheus-net.AspNetCore`, `/metrics`).
- [ ] Retirer `locatic.db` du versionnement (déjà couvert par `.gitignore`).

### A.2 — Tests automatisés
- [ ] Créer `app/tests/Locatic.Tests` (xUnit) : au moins des tests de services (règles métier réservations/parc) + un test d'intégration health/DB (SQLite in-memory).
- [ ] `dotnet test` doit passer et être **exécuté par la CI** (pas juste un build vert).

### A.3 — Dockerfile (`app/Dockerfile`)
- [ ] Multi-stage (SDK `net9.0` pour build/publish → runtime ASP.NET léger).
- [ ] **Utilisateur non-root**, `EXPOSE 8080`, `ENV ASPNETCORE_URLS=http://+:8080`.
- [ ] `VOLUME /data`, `HEALTHCHECK` sur `/health`.
- [ ] Build reproductible (`dotnet restore` cache, `--no-restore`).

### A.4 — Pipeline GitHub Actions (`.github/workflows/ci.yml`)
Jobs séparés et lisibles (bonus « pipeline propre » = obligatoire) :
- [ ] `build-test` : checkout → setup-dotnet 9 → restore → build → **test** → lint/format (`dotnet format --verify-no-changes`).
- [ ] `docker-build` : build de l'image (dépend de `build-test`).
- [ ] `scan` : **scan de vulnérabilités image** (Trivy) + **scan de secrets** (Gitleaks) — échoue si high/critical.
- [ ] `publish` : push vers GHCR **uniquement sur `main`** (condition `if: github.ref == 'refs/heads/main'`), avec `permissions: packages: write` et `GITHUB_TOKEN`.
- [ ] Déclencheurs : `pull_request` (tout sauf publish) et `push` sur `main`.
- [ ] Le pipeline **ne fait pas** de Terraform/Ansible/minikube.

### A.5 — Bonnes pratiques GitHub
- [ ] Protéger `main` : pas de push direct, PR obligatoire, **checks CI requis avant merge**, historique lisible.
- [ ] Créer une **PR de démonstration** représentative du workflow.
- [ ] Secrets dans GitHub Secrets (jamais dans Git).

**Interfaces fournies :** image `ghcr.io/.../locatic:<tag>`, endpoints `/health`, `/health/ready`, `/metrics`, port `8080`.
**Docs à écrire :** `docs/ci-cd.md`.

---

## 4. Lot B — Infrastructure locale : Terraform & Ansible (Personne B)

**Objectif :** provisionner l'infra locale et orchestrer tout le déploiement depuis le poste, sans action manuelle sur K8s.

### B.1 — minikube
- [ ] Documenter `minikube start` (driver, ressources), addons utiles (`metrics-server`).
- [ ] Vérifier l'accès `kubectl` au cluster.

### B.2 — Terraform (`infra/terraform/`) — provider `kubernetes`
- [ ] Créer le **namespace** `locatic`.
- [ ] Créer le **PVC** `locatic-sqlite-pvc` (stockage persistant SQLite).
- [ ] Éventuellement le `Secret` applicatif à partir de variables locales.
- [ ] **Outputs** : nom du namespace, nom du PVC, valeurs utiles à Ansible.
- [ ] Backend d'état **local**, `terraform.tfstate` **jamais commité** (déjà dans `.gitignore`), fournir `example.tfvars`.
- [ ] `terraform fmt` + `terraform validate` propres.

### B.3 — Ansible (`ansible/`)
- [ ] `playbook deploy.yml` qui :
  - vérifie les prérequis (minikube up, kubectl, helm),
  - récupère les **outputs Terraform** (`terraform output -json`),
  - construit les valeurs (image tag, namespace, PVC…),
  - applique le déploiement via **release Helm** (`helm upgrade --install locatic ../helm/locatic`) — Helm est le chemin nominal ici (bonus obligatoire), avec fallback documenté `kubectl apply -k` sur les manifests bruts.
- [ ] Idempotent, `--check` fonctionnel autant que possible.

### B.4 — Rollback (bonus obligatoire)
- [ ] Procédure `helm rollback locatic <revision>` documentée **et démontrée** (capture avant/après).

**Interfaces consommées :** manifests / chart de C, image de A.
**Interfaces fournies :** cluster prêt (namespace + PVC), déploiement déclenché.
**Docs à écrire :** `docs/terraform.md`, `docs/ansible.md`, section rollback de `docs/exploitation.md`.

---

## 5. Lot C — Kubernetes, Nginx, Helm & Monitoring (Personne C)

**Objectif :** décrire tout le déploiement (App derrière Nginx, PVC, monitoring) de façon configurable, packagé en chart Helm.

### C.1 — Manifests Kubernetes (`k8s/base/` + `k8s/overlays/dev/` en Kustomize)
- [ ] `Deployment` **app** : image paramétrable, port 8080, env (`ConnectionStrings__LocaticDb`), montage PVC sur `/data`, **liveness `/health` + readiness `/health/ready`**, requests/limits CPU/mémoire.
- [ ] `Deployment`/config **Nginx** en **reverse proxy** devant l'app (ConfigMap `nginx.conf` → `proxy_pass http://locatic-app`).
- [ ] `Service` app (ClusterIP `locatic-app`) + `Service` Nginx (**NodePort**, seul point d'entrée).
- [ ] `ConfigMap` (config Nginx + app), `Secret` **en template** (pas de valeur réelle).
- [ ] Utiliser le **PVC créé par Terraform** (`locatic-sqlite-pvc`).
- [ ] Annotations `prometheus.io/scrape` pour l'app.

### C.2 — Helm (`helm/locatic/`) — bonus obligatoire
- [ ] Chart configurable via `values.yaml` : `image.repository`, `image.tag`, `replicaCount`, `service.type`, `persistence.*`, `env`, `nginx.*`, `monitoring.enabled`.
- [ ] `helm lint` propre, `helm template` relu. **Utilisé par le playbook Ansible de B.**

### C.3 — Monitoring (`monitoring/`)
- [ ] **Prometheus** : scrape App (`/metrics`), Nginx (via `nginx-prometheus-exporter`), état des pods/services (kube-state-metrics), état du stockage (PVC).
- [ ] **Grafana** : provisioning datasource + dashboards.
  - [ ] **Dashboard comparatif** (bonus obligatoire) montrant en un coup d'œil l'état de **Nginx, App, Stockage, Monitoring**.
- [ ] **Alertes** (bonus obligatoire) : au moins App down, Nginx down, PVC presque plein — **testées** (déclencher et capturer).
- [ ] Chaque service important a **≥ 1 indicateur** visible dans Grafana.

**Interfaces consommées :** image + endpoints de A, PVC + namespace de B.
**Interfaces fournies :** chart Helm + manifests + stack monitoring à B.
**Docs à écrire :** `docs/kubernetes.md`, `docs/helm.md`, `docs/monitoring.md`.

---

## 6. Ligne du temps (dépendances)

> Les 3 lots démarrent en parallèle. Les jalons ⬥ sont des points de synchronisation.

**Phase 1 — Fondations (parallèle)**
- A : health endpoint + tests + Dockerfile qui build en local.
- B : minikube up + squelette Terraform (namespace + PVC) + `terraform apply` OK.
- C : manifests bruts avec une image publique de test (ex. image .NET quelconque) pour valider Nginx→App→PVC.
- ⬥ **Jalon 1** : contrats du §2 figés et écrits dans `docs/architecture.md`.

**Phase 2 — Intégration CI & packaging (parallèle)**
- A : pipeline complet (build/test/scan) + protection `main` + **première PR** + publication image sur `main`.
- C : chart Helm à partir des manifests + stack monitoring (Prometheus/Grafana) déployée.
- B : playbook Ansible qui applique la release Helm à partir des outputs Terraform.
- ⬥ **Jalon 2** : l'**image publiée par A** remplace l'image de test dans les values de C.

**Phase 3 — Déploiement de bout en bout**
- Enchaînement complet : `terraform apply` (B) → `ansible-playbook` (B) → release Helm (C) → App derrière Nginx (C) → monitoring (C).
- ⬥ **Jalon 3** : accès utilisateur via Nginx OK, données SQLite persistantes après redémarrage de pod.

**Phase 4 — Bonus & finition (parallèle)**
- C : dashboard comparatif + alertes testées.
- B : rollback démontré.
- A : nettoyage/séparation des jobs du pipeline.
- Tous : documentation `docs/*` + `docs/preuves/` (captures & logs).

---

## 7. Convention Git commune (à respecter par les 3)

- Branche `main` protégée ; travail sur branches `feat/<lot>-<sujet>` (ex. `feat/ci-pipeline`, `feat/tf-namespace`, `feat/helm-chart`).
- **1 PR = 1 changement cohérent**, revue par un autre membre, checks CI verts avant merge.
- Messages de commit type Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`…).
- Rien de sensible commité (secrets, `*.tfstate`, `.env`, clés) — voir `.gitignore`.

---

## 8. Definition of Done par lot

**Lot A :** `dotnet test` vert en CI • image publiée sur GHCR avec le bon tag • `main` protégée • PR de démo mergée • scans passants.
**Lot B :** `terraform apply` crée namespace+PVC • outputs exploités par Ansible • `ansible-playbook` déploie sans action manuelle • rollback démontré.
**Lot C :** App accessible **uniquement** via Nginx • SQLite sur PVC survit à un redémarrage de pod • chart Helm lint OK et utilisé par Ansible • chaque service visible dans Grafana + alertes testées.

---

## 9. Checklist finale de conformité (grille /20 + bonus)

- [ ] Reprise réelle du projet POO + documentation claire *(2)*
- [ ] Bonnes pratiques GitHub, PR, `main` protégée *(3)*
- [ ] Pipeline CI GitHub Actions fonctionnel *(3)*
- [ ] Build, scan et publication de l'image Docker *(2)*
- [ ] Terraform propre pour l'infra locale *(2)*
- [ ] Ansible orchestre le déploiement local *(2)*
- [ ] Déploiement K8s configurable : Nginx + App + PVC SQLite *(2)*
- [ ] Déploiement fonctionnel sur minikube derrière Nginx *(2)*
- [ ] Monitoring Prometheus + Grafana de chaque service *(2)*
- [ ] **Bonus** : alertes testées *(+1)* • dashboard comparatif *(+1)* • chart Helm utilisé par Ansible *(+2)* • rollback démontré *(+1)* • pipeline clair en jobs *(+1)*

### Pièges éliminatoires à éviter
- ❌ Pipeline vert sans test réel de l'app.
- ❌ Déploiement manuel sans Terraform/Ansible.
- ❌ Image jamais publiée.
- ❌ App exposée sans passer par Nginx.
- ❌ SQLite non montée sur un volume.
- ❌ Monitoring d'un seul service.
- ❌ **Secret ou `terraform.tfstate` commité** (erreur critique).
