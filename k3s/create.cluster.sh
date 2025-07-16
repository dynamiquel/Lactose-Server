kubectl create namespace lactose

helm upgrade \
    --install \
    kubernetes-dashboard \
    kubernetes-dashboard/kubernetes-dashboard \
    --namespace kubernetes-dashboard --create-namespace

kubectl create serviceaccount \
    -n kubernetes-dashboard \
    admin-user

kubectl create clusterrolebinding \
    -n kubernetes-dashboard \
    admin-user \
    --clusterrole cluster-admin \
    --serviceaccount=kubernetes-dashboard:admin-user

helm repo add jetstack https://charts.jetstack.io

helm repo update

helm install cert-manager \
    jetstack/cert-manager \
    --namespace cert-manager --create-namespace \
    --version v1.14.5 \
    --set installCRDs=true

./apply.cluster.sh
