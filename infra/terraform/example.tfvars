# Copier vers terraform.tfvars (ignoré par Git) si besoin de surcharger.
# cp example.tfvars terraform.tfvars

namespace          = "locatic"
pvc_name           = "locatic-sqlite-pvc"
pvc_size           = "1Gi"
storage_class_name = "standard"

# Décommenter si le contexte minikube n'est pas le contexte courant :
# kubeconfig_path    = "~/.kube/config"
# kubeconfig_context = "minikube"
