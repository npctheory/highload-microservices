# highload-microservices

```
grpcurl -plaintext localhost:8182 list
grpcurl -plaintext localhost:8182 list dialog.DialogService
grpcurl -plaintext -d '{"user_id": "User"}' localhost:8182 dialog.DialogService/ListDialogs
grpcurl -plaintext -d '{"user_id": "User", "agent_id": "LadyGaga"}' localhost:8182 dialog.DialogService/ListMessages
grpcurl -plaintext -d '{"sender_id": "User", "receiver_id": "LadyGaga", "text": "Sup 123"}' localhost:8182 dialog.DialogService/SendMessage

```