#!/bin/bash
set -euo pipefail

NAMESPACE="dotnet-microservices"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

USER_SVC_DIR="${SCRIPT_DIR}/user-service"
ORDER_SVC_DIR="${SCRIPT_DIR}/order-service"
INVENTORY_SVC_DIR="${SCRIPT_DIR}/inventory-service"
PAYMENT_SVC_DIR="${SCRIPT_DIR}/payment-service"

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║     .NET Microservices — K8s Deployment                 ║"
echo "╚══════════════════════════════════════════════════════════╝"

# CHECK TOOLS
command -v nerdctl >/dev/null || { echo "nerdctl missing"; exit 1; }
command -v kubectl  >/dev/null || { echo "kubectl missing"; exit 1; }

# BUILD IMAGES
echo "[ BUILD ] .NET services..."

nerdctl --namespace k8s.io build -t dotnet-user-service:latest "$USER_SVC_DIR"
nerdctl --namespace k8s.io build -t dotnet-order-service:latest "$ORDER_SVC_DIR"
nerdctl --namespace k8s.io build -t dotnet-inventory-service:latest "$INVENTORY_SVC_DIR"
nerdctl --namespace k8s.io build -t dotnet-payment-service:latest "$PAYMENT_SVC_DIR"

echo "✓ Images built"

# APPLY YAML
echo "[ APPLY ] Kubernetes..."
kubectl apply -f "${SCRIPT_DIR}/k8s.yaml"

# WAIT FOR MYSQL
echo "[ WAIT ] MySQL..."
kubectl rollout status deployment/mysql -n ${NAMESPACE} --timeout=180s

# WAIT FOR SERVICES
echo "[ WAIT ] Services..."
kubectl rollout status deployment/user-service      -n ${NAMESPACE}
kubectl rollout status deployment/order-service     -n ${NAMESPACE}
kubectl rollout status deployment/inventory-service -n ${NAMESPACE}
kubectl rollout status deployment/payment-service   -n ${NAMESPACE}

echo ""
echo "✅ ALL .NET SERVICES RUNNING"
echo ""
echo "User       → http://localhost:30011"
echo "Order      → http://localhost:30012"
echo "Inventory  → http://localhost:30013"
echo "Payment    → http://localhost:30014"