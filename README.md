#⚠️EM DESENVOLVIMENTO⚠️

# Perenne – Backend 

Plataforma de comunidade modular e escalável para gestão de grupos, mensagens em tempo real, feed interativo, eventos e notificações.

## 📖 Sobre

O **Perenne** é uma plataforma de comunidade construída em arquitetura modular e escalável, que oferece:

- Interações em grupos e troca de mensagens em tempo real.
- Feed de notícias personalizado para cada grupo.
- Discussões e fóruns internos.
- Gerenciamento de eventos e notificações.

## ✨ Funcionalidades

- **Autenticação e Gerenciamento de Usuários**: registro, login, recuperação de senha e perfis.
- **Grupos e Permissões**: criação de grupos, atribuição de papéis (admin, moderador, membro) e controle de acesso.
- **Mensagens em Tempo Real**: chat por grupo usando WebSockets.
- **Feed de Notícias**: postagens públicas do administrador/gestor.
- **Eventos**: criação, edição, inscrições e lembretes automáticos.

## 🏗️ Arquitetura e Tecnologias

- **.NET 9.0**: framework principal.
- **Entity Framework Core**: ORM para persistência de dados.
- **SignalR**: comunicação em tempo real via WebSockets.
- **Swagger / OpenAPI**: documentação automática das APIs.

## ⚙️ Pré-requisitos

Antes de começar, você precisará ter instalado em sua máquina:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Banco de dados suportado pelo Entity Framework.

---

## 🚀 Instalação

1. **Clone o repositório**
```
git clone https://github.com/filipe-ms/Perenne-Backend
```
2. **Preencha as chaves de conexão com o banco de dados no ```appsettings.json```**.

3. **Restaure pacotes e build**
```
dotnet restore
dotnet build
```

4. **Rodar com dotnet run**
```
dotnet run
```

## Colaboradores

- [Débora](https://github.com/DeboraCASouza/)
- [Yuri](https://github.com/yuricavalcanti06/)
- [Gabriel](https://github.com/andgabx/)
- [Rafael](https://github.com/rafael-zzz/)
- [Raphael](https://github.com/rafatito03/)
- [Marcelo](https://github.com/marceloh090/)

