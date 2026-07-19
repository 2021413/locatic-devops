# Ansible — orchestration du déploiement local

## Rôle

Ansible est le **chef d'orchestre** du déploiement local : une seule commande
enchaîne toutes les étapes, sans action manuelle sur Kubernetes.

```
Terraform (namespace locatic + PVC locatic-sqlite-pvc)
        │  terraform output -json
        ▼
Ansible (ansible/deploy.yml)
        │  helm upgrade --install locatic ../helm/locatic
        ▼
Release Helm `locatic` (App + Nginx + montage PVC) sur minikube
```

Ansible **ne crée pas** l'infra (rôle de Terraform, voir [terraform.md](terraform.md))
et **ne décrit pas** les objets Kubernetes (rôle du chart Helm, voir [helm.md](helm.md)) :
il consomme les deux et vérifie que le résultat est sain.

## Fichiers

| Fichier | Rôle |
| --- | --- |
| `ansible/ansible.cfg` | Inventaire par défaut, sortie YAML lisible |
| `ansible/inventory/hosts.yml` | `localhost` en connexion locale (pas de SSH) |
| `ansible/group_vars/all.yml` | Variables par défaut (chemins, release, tag, timeouts) |
| `ansible/deploy.yml` | Playbook de déploiement |
| `ansible/rollback.yml` | Playbook de rollback Helm |

## Déroulé de `deploy.yml`

1. **Prérequis** — `terraform`, `kubectl`, `helm`, `minikube` présents dans le
   PATH (`command -v`) et `minikube status` OK. Échec immédiat avec un message
   explicite sinon (`assert`).
2. **Outputs Terraform** — `terraform output -json` dans `infra/terraform`,
   parsé avec `from_json`. Le playbook échoue proprement si l'infra n'a pas été
   appliquée (sortie vide ou erreur) en rappelant la commande `terraform apply`.
   Il en extrait le namespace et le nom du PVC (output `ansible_vars`).
3. **Déploiement Helm** — chemin nominal (bonus obligatoire) :

   ```bash
   helm upgrade --install locatic ../helm/locatic \
     --namespace locatic \
     --set image.tag=<image_tag> \
     --set persistence.existingClaim=locatic-sqlite-pvc \
     --wait --timeout 5m
   ```

4. **Vérification** — `kubectl rollout status` sur les deployments
   `locatic-app` et `locatic-nginx` (échoue si un rollout ne converge pas).
5. **URL d'accès** — affichage de `http://$(minikube ip):30080`
   (Nginx en NodePort, seul point d'entrée).

## Variables

Défauts dans `ansible/group_vars/all.yml`, surchargeables avec `-e` :

| Variable | Défaut | Description |
| --- | --- | --- |
| `terraform_dir` | `{{ playbook_dir }}/../infra/terraform` | Dossier Terraform (outputs) |
| `chart_path` | `{{ playbook_dir }}/../helm/locatic` | Chart Helm déployé |
| `helm_release` | `locatic` | Nom de la release (contrat d'équipe) |
| `image_tag` | `latest` | Tag de `ghcr.io/2021413/locatic` |
| `helm_timeout` | `5m` | Timeout `helm --wait` |
| `rollout_timeout` | `120s` | Timeout `kubectl rollout status` |
| `nginx_node_port` | `30080` | NodePort Nginx affiché en fin de run |

## Commandes

| Commande | Effet |
| --- | --- |
| `ansible-playbook deploy.yml` | Déploiement complet (tag `latest`) |
| `ansible-playbook deploy.yml -e image_tag=sha-abc1234` | Déploiement d'un tag précis |
| `ansible-playbook deploy.yml --check` | Simulation (lectures seules) |
| `ansible-playbook rollback.yml -e revision=N` | Retour à la révision Helm `N` |

À lancer depuis `ansible/` (l'inventaire par défaut est résolu par `ansible.cfg`).

## Idempotence

Le playbook est relançable à volonté :

- les tâches de contrôle (`command -v`, `minikube status`, `terraform output`,
  `rollout status`) sont en lecture seule (`changed_when: false`) ;
- `helm upgrade --install` **converge** vers l'état décrit par le chart et les
  `--set` : même chart + même tag = même résultat, pods non recréés inutilement ;
- `--check` fonctionne : les lectures s'exécutent (`check_mode: false`), les
  commandes mutantes (`helm`, `kubectl rollout`) sont sautées.

Chaque exécution de `helm upgrade` crée néanmoins une nouvelle **révision**
dans l'historique Helm — c'est ce qui rend le rollback possible.

## Rollback (`rollback.yml`)

La révision cible est **obligatoire** (le playbook échoue avec un message
d'usage sinon) :

```bash
# 1. Identifier la révision saine
helm history locatic -n locatic        # (le playbook l'affiche aussi avant d'agir)

# 2. Revenir dessus
ansible-playbook rollback.yml -e revision=2
```

Le playbook relit le namespace dans les outputs Terraform (même source de
vérité que `deploy.yml`), affiche `helm history`, exécute
`helm rollback locatic <revision> --wait`, puis vérifie le rollout des deux
deployments. Le rollback crée lui-même une nouvelle révision (ex. rev 4 =
retour à la rev 2) : l'historique reste complet. Preuves avant/après dans
[preuves/](preuves/).

## Fallback sans Helm

Si Helm est indisponible, les manifests bruts restent applicables :

```bash
kubectl apply -k k8s/overlays/dev
```

Limites : valeurs figées dans l'overlay (pas de `--set image.tag=...`) et pas
de rollback par révision — à réserver au dépannage.

## Dépannage courant

| Symptôme | Cause probable | Correctif |
| --- | --- | --- |
| `Binaire(s) introuvable(s)...` | Outil absent du PATH | Installer terraform/kubectl/helm/minikube |
| `minikube ne répond pas` | Cluster arrêté | `minikube start` |
| `Impossible de lire les outputs Terraform` | `terraform apply` jamais lancé (state vide) | `cd infra/terraform && terraform init && terraform apply -var-file=example.tfvars` |
| `helm upgrade` en timeout | Image non tirable ou probes en échec | `kubectl get pods -n locatic`, `kubectl describe pod ...`, vérifier `image_tag` |
| `rollout status` en échec | Pod en CrashLoopBackOff | `kubectl logs deploy/locatic-app -n locatic` puis `rollback.yml` |
| `Variable revision obligatoire` | `-e revision=N` oublié | `helm history locatic -n locatic` puis relancer avec `-e revision=N` |
| URL injoignable | NodePort/tunnel selon driver | `minikube service locatic-nginx -n locatic --url` |
