# Déploiement local — guide bout en bout

Ordre exact des actions pour déployer Locatic sur un poste local, de zéro jusqu'à l'app
accessible derrière Nginx avec monitoring. Le pipeline GitHub s'arrête à la publication
de l'image ([ci-cd.md](ci-cd.md)) ; tout ce qui suit se joue **localement**.

## 1. Prérequis

| Outil | Version indicative | Vérification |
| --- | --- | --- |
| Docker | 24+ | `docker version` |
| minikube | 1.33+ | `minikube version` |
| kubectl | 1.30+ | `kubectl version --client` |
| Terraform | 1.7+ | `terraform version` |
| Ansible | 2.16+ (ansible-core) | `ansible --version` |
| Helm | 3.14+ | `helm version` |

Une image doit avoir été publiée sur GHCR par la CI (merge sur `main`) : relever son tag
`sha-xxxxxxx` (page Actions du job `publish` ou page Packages du dépôt).

## 2. Démarrer minikube

```bash
minikube start --driver=docker --cpus=2 --memory=4096
minikube addons enable metrics-server

# Vérifier l'accès au cluster
kubectl get nodes
```

## 3. Provisionner l'infra (Terraform)

Crée le namespace `locatic` et le PVC `locatic-sqlite-pvc` (détails :
[terraform.md](terraform.md)) :

```bash
cd infra/terraform
terraform init
terraform plan  -var-file=example.tfvars
terraform apply -var-file=example.tfvars

# Vérifier
kubectl get namespace locatic
kubectl get pvc -n locatic
terraform output -json ansible_vars
```

## 4. Déployer l'application (Ansible → Helm)

Le playbook vérifie les prérequis (minikube up, kubectl, helm, terraform), lit les
outputs Terraform, applique la release Helm `locatic` puis attend le rollout (détails :
[ansible.md](ansible.md)) :

```bash
cd ../../ansible
ansible-playbook deploy.yml
# Tag explicite si besoin :
ansible-playbook deploy.yml -e image_tag=sha-xxxxxxx
```

En fin de playbook, l'URL d'accès est affichée.

## 5. Vérifier

```bash
# Pods Ready (app + nginx), services, PVC Bound
kubectl get pods,svc,pvc -n locatic
kubectl rollout status deployment/locatic-app -n locatic

# Release Helm
helm status locatic -n locatic

# Accès utilisateur — via Nginx uniquement (NodePort 30080)
minikube service locatic-nginx -n locatic --url
curl -s "$(minikube service locatic-nginx -n locatic --url)/health"
```

Ouvrir l'URL dans un navigateur : l'application Locatic doit répondre. Test de
persistance : créer une donnée dans l'UI, supprimer le pod app
(`kubectl delete pod -n locatic -l app.kubernetes.io/name=locatic-app`), vérifier que la
donnée est toujours là (SQLite sur PVC — voir [exploitation.md](exploitation.md)).

## 6. Déployer le monitoring

```bash
cd ..
kubectl apply -k monitoring/

kubectl get pods -n locatic          # prometheus, grafana, kube-state-metrics Ready
minikube service grafana -n locatic --url      # dashboard "Locatic — Overview"
minikube service prometheus -n locatic --url   # cibles + alertes
```

Détails (annotations de scrape, alertes, dashboards) : [monitoring.md](monitoring.md).

## 7. Nettoyage

```bash
# Supprimer la stack applicative et le monitoring
helm uninstall locatic -n locatic
kubectl delete -k monitoring/

# Détruire l'infra Terraform (namespace + PVC — supprime les données SQLite)
cd infra/terraform
terraform destroy -var-file=example.tfvars

# Supprimer complètement le cluster
minikube delete
```

## Récapitulatif

```
minikube start ─▶ terraform apply ─▶ ansible-playbook deploy.yml ─▶ vérifications ─▶ kubectl apply -k monitoring/
                  (ns + PVC)         (Helm : app + nginx)            (pods Ready,      (Prometheus + Grafana)
                                                                      URL via Nginx)
```
