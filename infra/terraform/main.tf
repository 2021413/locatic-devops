resource "kubernetes_namespace_v1" "locatic" {
  metadata {
    name   = var.namespace
    labels = var.labels
  }
}

resource "kubernetes_persistent_volume_claim_v1" "sqlite" {
  metadata {
    name      = var.pvc_name
    namespace = kubernetes_namespace_v1.locatic.metadata[0].name
    labels    = var.labels
  }

  spec {
    access_modes       = ["ReadWriteOnce"]
    storage_class_name = var.storage_class_name != "" ? var.storage_class_name : null

    resources {
      requests = {
        storage = var.pvc_size
      }
    }
  }

  wait_until_bound = false
}
