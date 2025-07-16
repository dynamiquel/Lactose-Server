for file in cluster/secrets.*.yaml; do
    kubectl apply -f "$file"
done

kubectl apply -f cluster/trafeik.yaml
kubectl apply -f cluster/certs/external.issuer.yaml
kubectl apply -f cluster/certs/internal.root.issuer.yaml
kubectl apply -f cluster/certs/internal.issuer.yaml
