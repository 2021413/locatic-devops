# CI/CD — GitHub Actions

## Rôle

Le pipeline (`.github/workflows/ci.yml`) construit, teste, scanne puis publie l'image
Docker de l'application sur GHCR. Il **s'arrête à la publication** : aucun déploiement
n'est fait depuis GitHub (le déploiement sur minikube est local, via Terraform + Ansible).

## Déclencheurs

| Événement | Jobs exécutés |
| --- | --- |
| `pull_request` (toute PR) | `build-test`, `docker-build`, `scan` |
| `push` sur `main` | `build-test`, `docker-build`, `scan`, `publish` |

Une `concurrency` (`ci-<workflow>-<ref>`) annule les exécutions obsolètes sur une même
branche (`cancel-in-progress: true`). Les permissions par défaut sont réduites à
`contents: read` ; chaque job qui a besoin de plus les élargit localement.

## Les 4 jobs

```
build-test ──▶ docker-build ──▶ scan ──▶ publish (main uniquement)
```

### 1. `build-test`

| Étape | Commande |
| --- | --- |
| Setup .NET | `actions/setup-dotnet@v4`, version `9.0.x` |
| Restore | `dotnet restore app/Locatic.sln` |
| Build | `dotnet build app/Locatic.sln --no-restore -c Release` |
| Test | `dotnet test app/Locatic.sln --no-build -c Release --verbosity normal` |
| Lint | `dotnet format app/Locatic.sln --verify-no-changes --no-restore` |

Les tests exécutés sont ceux de `app/tests/Locatic.Tests` (xUnit) : tests de services
(règles métier réservations / parc) et tests d'intégration des endpoints de santé via
`WebApplicationFactory` — le pipeline teste réellement l'application, pas seulement le build.

### 2. `docker-build` (dépend de `build-test`)

- Build de l'image via `docker/build-push-action@v6` (contexte `app`, `app/Dockerfile`),
  **sans push** (`push: false`, `load: true`), tag local `locatic:ci`.
- Cache de build GitHub Actions (`cache-from`/`cache-to: type=gha`).
- L'image est sauvegardée (`docker save`) et publiée comme **artifact** (`locatic-image`,
  rétention 1 jour) pour être réutilisée telle quelle par le job `scan` : l'image scannée
  est exactement celle qui a été construite.

### 3. `scan` (dépend de `docker-build`) — bloquant

| Scan | Outil | Politique d'échec |
| --- | --- | --- |
| Secrets dans le dépôt | Gitleaks (`gitleaks/gitleaks-action@v2`, `fetch-depth: 0`) | Échec si un secret est détecté |
| Vulnérabilités image | Trivy (`aquasecurity/trivy-action@0.28.0`) sur l'artifact rechargé | `exit-code: 1` si **HIGH/CRITICAL** (`ignore-unfixed: true`) |

Un finding HIGH ou CRITICAL fait échouer le pipeline, donc bloque le merge et la publication.

### 4. `publish` (dépend de `scan`) — `main` uniquement

Condition : `if: github.event_name == 'push' && github.ref == 'refs/heads/main'`.
Aucune image n'est donc publiée depuis une PR.

- Login GHCR via `docker/login-action@v3` avec `GITHUB_TOKEN` (pas de secret personnalisé).
- Permissions du job : `contents: read`, **`packages: write`** (requis pour pousser sur GHCR).
- Nom d'image normalisé en minuscules : `ghcr.io/2021413/locatic`.
- Tags générés par `docker/metadata-action@v5` :

| Tag | Signification |
| --- | --- |
| `latest` | Dernière image issue de `main` |
| `sha-xxxxxxx` | SHA court du commit — tag immuable, utilisé pour déployer et pour le rollback |

## Secrets et permissions

| Secret / permission | Usage |
| --- | --- |
| `GITHUB_TOKEN` (fourni par GitHub) | Login GHCR (`publish`) et API (Gitleaks) |
| `permissions: packages: write` | Job `publish` uniquement |
| `permissions: security-events: write` | Job `scan` |

Aucun secret applicatif n'est stocké dans le dépôt ; les éventuels secrets vont dans
GitHub Secrets côté CI et dans le Secret Kubernetes `locatic-secrets` côté cluster.

## Protection de `main` et workflow PR

- `main` est **protégée** : pas de push direct, passage obligatoire par une **Pull Request**.
- **Checks requis** avant merge : `build-test`, `docker-build`, `scan` doivent être verts.
- Revue par un autre membre de l'équipe (1 PR = 1 changement cohérent, voir
  `PLAN-EQUIPE.md` §7).
- Le job `publish` ne tourne qu'après merge, sur l'événement `push` de `main`.

Workflow type :

```
feat/<lot>-<sujet> ──▶ PR vers main ──▶ CI (build-test / docker-build / scan)
        ──▶ revue + checks verts ──▶ merge ──▶ CI sur main ──▶ publish GHCR (latest + sha-xxxxxxx)
```

## Récupérer l'image publiée

```bash
docker pull ghcr.io/2021413/locatic:latest
docker pull ghcr.io/2021413/locatic:sha-xxxxxxx   # remplacer par le SHA court réel
```

Le tag `sha-xxxxxxx` est la valeur à passer au déploiement
(`ansible-playbook deploy.yml`, variable `image.tag` du chart Helm — voir
[helm.md](helm.md) et [ansible.md](ansible.md)).
