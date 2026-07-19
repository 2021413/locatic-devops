variable "kubeconfig_path" {
  type        = string
  description = "Chemin vers le kubeconfig (minikube par défaut)."
  default     = "~/.kube/config"
}

variable "kubeconfig_context" {
  type        = string
  description = "Contexte kubectl. Laisser vide pour utiliser le contexte courant."
  default     = ""
}

variable "namespace" {
  type        = string
  description = "Namespace Kubernetes de l'application Locatic."
  default     = "locatic"
}

variable "pvc_name" {
  type        = string
  description = "Nom du PVC pour la base SQLite."
  default     = "locatic-sqlite-pvc"
}

variable "pvc_size" {
  type        = string
  description = "Taille du volume persistant SQLite."
  default     = "1Gi"
}

variable "storage_class_name" {
  type        = string
  description = "StorageClass (minikube: standard). Chaîne vide = défaut du cluster."
  default     = "standard"
}

variable "labels" {
  type        = map(string)
  description = "Labels communs appliqués aux ressources."
  default = {
    "app.kubernetes.io/name"       = "locatic"
    "app.kubernetes.io/part-of"    = "locatic"
    "app.kubernetes.io/managed-by" = "terraform"
  }
}
