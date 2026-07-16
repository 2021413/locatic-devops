# CI/CD — GitHub Actions

Pipeline défini dans [`workflows/ci.yml`](workflows/ci.yml).

Jobs :
- `build-test` : restore, build, tests, `dotnet format`
- `docker-build` : image Docker (sans push)
- `scan` : Gitleaks (secrets) + Trivy (vulnérabilités HIGH/CRITICAL)

La publication GHCR sur `main` arrive dans une PR dédiée (`feat/ci-publish-ghcr`).

Voir aussi [`docs/ci-cd.md`](../docs/ci-cd.md).
