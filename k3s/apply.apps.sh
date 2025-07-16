for file in apps/secrets/*.yaml; do
    kubectl apply -f "$file"
done

kubectl apply -f apps/landing.yaml
kubectl apply -f apps/lactose.maindb.yaml
kubectl apply -f apps/lactose.mosquitto.yaml
kubectl apply -f apps/lactose.identity.yaml
kubectl apply -f apps/lactose.config.yaml
kubectl apply -f apps/lactose.economy.yaml
kubectl apply -f apps/lactose.simulation.yaml
kubectl apply -f apps/lactose.tasks.yaml

kubectl rollout restart deployment -n lactose landing
kubectl rollout restart deployment -n lactose lactose-mosquitto
kubectl rollout restart deployment -n lactose lactose-identity
kubectl rollout restart deployment -n lactose lactose-config
kubectl rollout restart deployment -n lactose lactose-economy
kubectl rollout restart deployment -n lactose lactose-simulation
kubectl rollout restart deployment -n lactose lactose-tasks

kubectl get pods -n lactose