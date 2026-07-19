# CI/CD — GitHub Actions

Pipeline défini dans [`workflows/ci.yml`](workflows/ci.yml).

Jobs :
- `build-test` : restore, build, tests, `dotnet format`
- `docker-build` : image Docker (sans push)
- `scan` : Gitleaks (secrets) + Trivy (vulnérabilités HIGH/CRITICAL)
- `publish` : push vers GHCR **uniquement sur `main`** (`ghcr.io/<owner>/locatic:sha-xxxxxxx` + `:latest`)

Le pipeline ne déploie pas sur minikube (Terraform / Ansible restent locaux).

Voir aussi [`docs/ci-cd.md`](../docs/ci-cd.md).
