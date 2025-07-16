#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
K3D_DATA_DIR="$SCRIPT_DIR/data"
LOCAL_PATH_DATA_BASE_DIR="$K3D_DATA_DIR/k3s-storage" 

K3D_CLUSTER_NAME="lactose"
K3D_NETWORK_NAME="lactose"

mkdir -p "$LOCAL_PATH_DATA_BASE_DIR/server"
mkdir -p "$LOCAL_PATH_DATA_BASE_DIR/agent0"
mkdir -p "$LOCAL_PATH_DATA_BASE_DIR/agent1"
mkdir -p "$LOCAL_PATH_DATA_BASE_DIR/agent2"

k3d cluster create $K3D_CLUSTER_NAME \
    --network $K3D_NETWORK_NAME \
    --registry-config "$SCRIPT_DIR/../config/registries.yaml" \
    --agents 3 \
    -p "5001:80@loadbalancer" \
    -p "5002:443@loadbalancer" \
    -v "$LOCAL_PATH_DATA_BASE_DIR/server:/var/lib/rancher/k3s/storage:shared@server:0" \
    -v "$LOCAL_PATH_DATA_BASE_DIR/agent0:/var/lib/rancher/k3s/storage:shared@agent:0" \
    -v "$LOCAL_PATH_DATA_BASE_DIR/agent1:/var/lib/rancher/k3s/storage:shared@agent:1" \
    -v "$LOCAL_PATH_DATA_BASE_DIR/agent2:/var/lib/rancher/k3s/storage:shared@agent:2"

"$SCRIPT_DIR/../create.cluster.sh"

echo "k3d cluster '$K3D_CLUSTER_NAME' created successfully."
echo "Persistent data directories are located in: $LOCAL_PATH_DATA_BASE_DIR"