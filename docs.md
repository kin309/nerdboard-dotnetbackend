# 📡 Documentação do SignalR - RoomHub

Este documento detalha os métodos disponíveis no backend via SignalR para comunicação em tempo real.

## 🚀 Conectando ao Hub
Antes de chamar qualquer método, certifique-se de que a conexão com o SignalR está estabelecida:

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://seuservidor.com/roomHub")
    .build();

connection.start()
    .then(() => console.log("Conectado ao SignalR"))
    .catch(err => console.error("Erro ao conectar:", err));
```

## 📌 Métodos Disponíveis

### 🏠 Criar uma sala
```typescript
connection.invoke("CreateRoom", "Nome da Sala", { id: "123", username: "JohnDoe" })
    .catch(err => console.error("Erro ao criar sala:", err));
```
📤 **Retorno (Evento no Frontend)**:
```typescript
connection.on("RoomCreated", (roomName) => {
    console.log("Sala criada:", roomName);
});
```

---

### 👤 Adicionar usuário a uma sala
```typescript
connection.invoke("AddUserToRoom", "room123", { id: "456", username: "JaneDoe" })
    .catch(err => console.error("Erro ao adicionar usuário:", err));
```
📤 **Retorno (Evento no Frontend)**:
```typescript
connection.on("UserAdded", (username) => {
    console.log("Usuário adicionado:", username);
});
```

---

### ❌ Remover usuário de uma sala
```typescript
connection.invoke("RemoveUserFromRoom", "room123", "456")
    .catch(err => console.error("Erro ao remover usuário:", err));
```
📤 **Retorno (Evento no Frontend)**:
```typescript
connection.on("UserRemoved", (userId) => {
    console.log("Usuário removido:", userId);
});
```

---

### ✉️ Enviar mensagem para a sala
```typescript
connection.invoke("SendMessageToRoom", "room123", "JohnDoe", "Olá, pessoal!")
    .catch(err => console.error("Erro ao enviar mensagem:", err));
```
📤 **Retorno (Evento no Frontend)**:
```typescript
connection.on("ReceiveMessage", (userName, text) => {
    console.log(`${userName} disse: ${text}`);
});
```

---

### 📌 Listar salas ativas
```typescript
connection.invoke("GetRooms")
    .then(rooms => console.log("Salas disponíveis:", rooms))
    .catch(err => console.error("Erro ao buscar salas:", err));
```
📤 **Retorno esperado:**
```typescript
interface Room {
    roomId: string;
    roomName: string;
    createdBy: string;
    users: { [key: string]: FirestoreUser };
}

connection.on("GetRooms", (rooms: Room[]) => {
    console.log("Salas encontradas:", rooms);
});
```

---

### 👫 Listar usuários de uma sala
```typescript
connection.invoke("GetUsersInRoom", "room123")
    .then(users => console.log("Usuários na sala:", users))
    .catch(err => console.error("Erro ao buscar usuários:", err));
```

---

## 🛠 Erros Comuns e Soluções

1. **Erro: `Failed to invoke...`**
   - Verifique se o backend está rodando e acessível.
   - Confirme que os parâmetros enviados correspondem à estrutura esperada.
   
2. **Erro de CORS**
   - No backend, adicione permissões de CORS para permitir conexão do frontend:
   
   ```csharp
   services.AddCors(options => {
       options.AddPolicy("CorsPolicy",
           builder => builder.AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials()
                             .WithOrigins("http://localhost:3000"));
   });
   ```

3. **Conexão não inicia**
   - Certifique-se de que o SignalR está corretamente configurado no frontend.
   - Tente iniciar a conexão com um retry:
     ```typescript
     async function startConnection() {
         try {
             await connection.start();
             console.log("Conectado ao SignalR");
         } catch (err) {
             console.error("Erro ao conectar, tentando novamente em 5s:", err);
             setTimeout(startConnection, 5000);
         }
     }
     startConnection();
     ```


