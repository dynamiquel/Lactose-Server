#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
K3D_DATA_DIR="$SCRIPT_DIR/data"

K3D_CLUSTER_NAME="lactose"
K3D_NETWORK_NAME="lactose"

k3d cluster create $K3D_CLUSTER_NAME \
    --network $K3D_NETWORK_NAME \
    --registry-config "$SCRIPT_DIR/../config/registries.yaml" \
    --agents 3 \
    -p "5001:80@loadbalancer" \
    -p "5002:443@loadbalancer"

"$SCRIPT_DIR/../create.cluster.sh"

echo "k3d cluster '$K3D_CLUSTER_NAME' created successfully."
