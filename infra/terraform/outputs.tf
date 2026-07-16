output "namespace" {
  description = "Namespace Kubernetes créé pour Locatic."
  value       = kubernetes_namespace_v1.locatic.metadata[0].name
}

output "pvc_name" {
  description = "Nom du PVC SQLite (monté sur /data par le chart Helm)."
  value       = kubernetes_persistent_volume_claim_v1.sqlite.metadata[0].name
}

output "pvc_storage" {
  description = "Taille demandée pour le PVC SQLite."
  value       = var.pvc_size
}

output "storage_class_name" {
  description = "StorageClass utilisée pour le PVC."
  value       = var.storage_class_name
}

output "ansible_vars" {
  description = "Bloc prêt à consommer par Ansible (terraform output -json ansible_vars)."
  value = {
    namespace          = kubernetes_namespace_v1.locatic.metadata[0].name
    pvc_name          = kubernetes_persistent_volume_claim_v1.sqlite.metadata[0].name
    pvc_storage       = var.pvc_size
    storage_class_name = var.storage_class_name
  }
}
