### О проекте
Домашнее задание по микросервисам.  
Проект состоит из следующих компонентов:  
* Солюшен .NET в папке ./server, который собирается в два образа: core:local и dialogs:local (контейнеры core и dialogs).
* Dockerfile и сид базы данных postgres в папке ./db, который собирается в образ db:local (контейнер pg_master).
* В папке tests находятся запросы для расширения VSCode REST Client и экспорты коллекций и окружений Postman.
* В docker-compose.yml подключаются Redis, Redis Insight, RabbitMQ, PGAdmin.
### Начало работы
Склонировать проект, сделать cd в корень репозитория и запустить Docker Compose.  
Дождаться статуса healthy на контейнерах postgres.  
```bash
https://github.com/npctheory/highload-microservices.git
cd highload-microservices
docker compose up --build -d
```
### Разделение монолита
Солюшен собирается в два приложения:  
Проекты с префиксом Core собираются в образ core:local, который запускается в контейнере core.  
На core:80 (http://localhost:8080) работают эндпоинты:  
register  
login  
user/get/{id}  
user/search  
friend/list  
friend/set/{friend_id}  
friend/delete/{friend_id}  
post/list  
post/feed  
post/get/{id}  
post/delete/{post_id}  
post/create  
post/update  
dialog/list  
dialog/{agentId}/list  
dialog/{receiverId}/send  

Проекты с префиксом Dialogs собираются в образ dialogs:local, который запускается в контейнере dialogs.  
На dialogs:80 (http://localhost:8180) работают эндпоинты:  
dialog/list  
dialog/{agentId}/list  
dialog/{receiverId}/send  

### Взаимодействия монолитного сервиса и сервиса чатов  
На dialogs:82 (http://localhost:8182) работает сервер gRPC.  
Интерфейс для сервиса gRPC генерируется из файла [dialogs.proto](https://github.com/npctheory/highload-microservices/blob/main/server/Dialogs.Api/Protos/dialogs.proto)  
Методы контроллера [Core.Api.Controllers.DialogController](https://github.com/npctheory/highload-microservices/blob/main/server/Core.Api/Controllers/DialogController.cs) переписаны и теперь делают запросы через grpc-клиент к новому микросервису. Из библиотек Core.Application и Core.Domain удалены логика и модели для работы с диалогами.   
Пример запросов к серверу диалогов через grpcurl:  

[grpc_test.webm](https://github.com/user-attachments/assets/38ce1b54-60ee-4786-bd26-2ac846b68654)  

### X-request-id  
x-request-id добавляется в миддлваре [Core.Api.Middleware.RequestIdMiddleware](https://github.com/npctheory/highload-microservices/blob/main/server/Core.Api/Middleware/RequestIdMiddleware.cs), [Dialogs.Api.Middleware.RequestIdMiddleware](https://github.com/npctheory/highload-microservices/blob/main/server/Dialogs.Api/Middleware/RequestIdMiddleware.cs)  

### Сквозное логирование  
Контроллер диалогов старого API [Core.Api.Controllers.DialogController](https://github.com/npctheory/highload-microservices/blob/main/server/Core.Api/Controllers/DialogController.cs) добавляет x-request-id в метадату запросов gRPC к новому API.  

Пример работы сначала с новым API http://dialogs:80(http://localhost:8180), а потом старым (http://core:80(http://localhost:8080)).  
Запросы к новому API логируются только на машине dialogs. Запросы к старому API логируются и на core и на dialogs.  

[new_api_test_and_legacy_api_test.webm](https://github.com/user-attachments/assets/585f1ee5-c5d4-4993-85d9-00bfc1685cab)
