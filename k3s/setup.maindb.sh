# This script setups the replica set and authentication for the maindb.
# It also imports starting data into the database.
# I was tired doing this manually every single time.

NAMESPACE="lactose"
SERVICE_NAME="maindb-service"
REPLICA_SET_NAME="rs0"
SECRET_NAME="secret.mongo.auth"
DB_NAME="maindb"
IMPORT_DATA_DIR="../data/maindb"

echo "Reading credentials from secret '$SECRET_NAME' in namespace '$NAMESPACE'..."
MONGO_USER=$(kubectl get secret "$SECRET_NAME" -n "$NAMESPACE" -o jsonpath='{.data.username}' | base64 --decode)
MONGO_PASS=$(kubectl get secret "$SECRET_NAME" -n "$NAMESPACE" -o jsonpath='{.data.password}' | base64 --decode)
# Need to URL-encode the password, in case it contains URL special characters.
ENCODED_PASS=$(python -c 'import urllib.parse, sys; print(urllib.parse.quote_plus(sys.argv[1]))' "$MONGO_PASS")

if [ -z "$MONGO_USER" ] || [ -z "$MONGO_PASS" ]; then
    echo "Error: Could not retrieve username or password from the secret"
    exit 1
fi

echo "Initialising MongoDB Replica Set '$REPLICA_SET_NAME' in namespace '$NAMESPACE'..."

kubectl exec -it maindb-0 -n $NAMESPACE -- mongosh <<EOF
  rs.initiate({
    _id: "$REPLICA_SET_NAME",
    members: [
      { _id: 0, host: "$DB_NAME-0.$SERVICE_NAME.$NAMESPACE.svc.cluster.local:27017" },
      { _id: 1, host: "$DB_NAME-1.$SERVICE_NAME.$NAMESPACE.svc.cluster.local:27017" },
      { _id: 2, host: "$DB_NAME-2.$SERVICE_NAME.$NAMESPACE.svc.cluster.local:27017" }
    ]
  });
EOF


echo "Replica set initiation command sent. Waiting for primary to be elected..."
sleep 10 # Give time for the election process

echo "Creating admin user '$MONGO_USER'..."
kubectl exec -it $DB_NAME-0 -n $NAMESPACE -- mongosh <<EOF
  use admin
  db.createUser({
    user: "$MONGO_USER",
    pwd: "$MONGO_PASS",
    roles: [{ role: "root", db: "admin" }]
  });
EOF

echo "Starting data import from '$IMPORT_DATA_DIR'..."

if [ ! -d "$IMPORT_DATA_DIR" ]; then
    echo "Error: Import directory '$IMPORT_DATA_DIR' not found"
    exit 1
fi

for db_dir in "$IMPORT_DATA_DIR"/*/; do
    if [ -d "$db_dir" ]; then
        database_name=$(basename "$db_dir")
        echo "Importing into database '$database_name'..."
        
        for file in "$db_dir"/*.json; do
            if [ -f "$file" ]; then
                collection_name=$(basename "$file" .json)
                echo "   - Importing collection '$collection_name' from '$file'..."

                # The mongoimport tool will automatically create the database if it doesn't exist
                kubectl exec -i $DB_NAME-0 -n "$NAMESPACE" -- mongoimport \
                    --uri "mongodb://$MONGO_USER:$ENCODED_PASS@$DB_NAME-0.$SERVICE_NAME.$NAMESPACE.svc.cluster.local:27017/$database_name?authSource=admin" \
                    --collection "$collection_name" \
                    --file /dev/stdin --jsonArray < "$file"
                
                if [ $? -ne 0 ]; then
                    echo "   Error importing '$collection_name'."
                else
                    echo "   Successfully imported '$collection_name'."
                fi
            fi
        done
    fi
done

echo "All imports complete"

echo "MongoDB setup complete"