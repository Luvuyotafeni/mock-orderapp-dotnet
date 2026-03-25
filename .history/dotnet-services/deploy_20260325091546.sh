#!/bin/bash
set -euo pipefail

NAMESPACE="dotnet-microservices"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Service directories
USER_SVC_DIR="${SCRIPT_DIR}/UserService"
ORDER_SVC_DIR="${SCRIPT_DIR}/OrderService"
INVENTORY_SVC_DIR="${SCRIPT_DIR}/InventoryService"
PAYMENT_SVC_DIR="${SCRIPT_DIR}/PaymentService"

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║     .NET Microservices — K8s Deployment                 ║"
echo "║     Runtime: containerd (nerdctl)                       ║"
echo "╚══════════════════════════════════════════════════════════╝"

# ── CHECK TOOLS ─────────────────────────────────────────────
echo ""
echo "[ CHECK ] Verifying tools..."

command -v nerdctl >/dev/null 2>&1 || { echo "✗ nerdctl not found"; exit 1; }
command -v kubectl  >/dev/null 2>&1 || { echo "✗ kubectl not found"; exit 1; }

echo "✓ nerdctl found"
echo "✓ kubectl found"

# ── CHECK FOLDERS ───────────────────────────────────────────
echo ""
echo "[ CHECK ] Verifying service folders..."

for dir in "$USER_SVC_DIR" "$ORDER_SVC_DIR" "$INVENTORY_SVC_DIR" "$PAYMENT_SVC_DIR"; do
  if [ ! -d "$dir" ]; then
    echo "✗ Missing folder: $dir"
    exit 1
  fi

  if [ ! -f "$dir/Dockerfile" ]; then
    echo "✗ Missing Dockerfile in: $dir"
    exit 1
  fi

  echo "✓ $(basename "$dir")"
done

# ── STEP 1: BUILD IMAGES ────────────────────────────────────
echo ""
echo "[ STEP 1 ] Building Docker images with nerdctl..."

nerdctl --namespace k8s.io build -t dotnet-user-service:latest "$USER_SVC_DIR"
echo "✓ user-service built"

nerdctl --namespace k8s.io build -t dotnet-order-service:latest "$ORDER_SVC_DIR"
echo "✓ order-service built"

nerdctl --namespace k8s.io build -t dotnet-inventory-service:latest "$INVENTORY_SVC_DIR"
echo "✓ inventory-service built"

nerdctl --namespace k8s.io build -t dotnet-payment-service:latest "$PAYMENT_SVC_DIR"
echo "✓ payment-service built"

# ── STEP 2: APPLY K8S ───────────────────────────────────────
echo ""
echo "[ STEP 2 ] Applying Kubernetes YAML..."

kubectl apply -f "${SCRIPT_DIR}/project.yaml"

echo "✓ Resources applied"

# ── STEP 3: WAIT FOR MYSQL ──────────────────────────────────
echo ""
echo "[ STEP 3 ] Waiting for MySQL..."

kubectl rollout status deployment/mysql -n ${NAMESPACE} --timeout=180s

echo "✓ MySQL ready"

# ── STEP 4: WAIT FOR SERVICES ───────────────────────────────
echo ""
echo "[ STEP 4 ] Waiting for microservices..."

kubectl rollout status deployment/user-service      -n ${NAMESPACE} --timeout=180s
kubectl rollout status deployment/order-service     -n ${NAMESPACE} --timeout=180s
kubectl rollout status deployment/inventory-service -n ${NAMESPACE} --timeout=180s
kubectl rollout status deployment/payment-service   -n ${NAMESPACE} --timeout=180s

echo "✓ All services running"

# ── DONE ────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║          ✅ .NET DEPLOYMENT SUCCESSFUL                  ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║                                                          ║"
echo "║  USER SERVICE      → http://localhost:30008              ║"
echo "║  ORDER SERVICE     → http://localhost:30009              ║"
echo "║  INVENTORY SERVICE → http://localhost:30010              ║"
echo "║  PAYMENT SERVICE   → http://localhost:30011              ║"
echo "║                                                          ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║  Useful Commands                                         ║"
echo "║                                                          ║"
echo "║  kubectl get pods -n ${NAMESPACE}                        ║"
echo "║  kubectl logs <pod> -n ${NAMESPACE}                      ║"
echo "║  kubectl get svc -n ${NAMESPACE}                         ║"
echo "╚══════════════════════════════════════════════════════════╝"