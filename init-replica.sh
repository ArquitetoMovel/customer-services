#!/bin/bash

# Aguarda até que todos os nós do MongoDB estejam disponíveis
until mongosh --host mongo1:27017 --eval "print(\"waited for connection\")"
do
    sleep 2
done
echo "Connection to mongo1 succeeded"
until mongosh --host mongo2:27017 --eval "print(\"waited for connection\")"
do
    sleep 2
done
echo "Connection to mongo2 succeeded"
until mongosh --host mongo3:27017 --eval "print(\"waited for connection\")"
do
    sleep 2
done
echo "Connection to mongo3 succeeded"

# Inicializa o replica set
mongosh --host mongo1:27017 <<EOF
rs.initiate({
  _id: "rs0",
  members: [
    { _id: 0, host: "mongo1:27017" },
    { _id: 1, host: "mongo2:27017" },
    { _id: 2, host: "mongo3:27017" }
  ]
})
EOF

echo "Replica set initialized"
