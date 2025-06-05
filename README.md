
<h1 align="center">⚠️ EM DESENVOLVIMENTO ⚠️</h1>

<h4 align="center">[ Ver também: <a href="https://github.com/andgabx/Perenne-Frontend">Perenne-Frontend</a> ]</h4>

---
Informação para os avaliadores:
A branch mais atual (e usada no deploy) é a [Prod](https://github.com/filipe-ms/Perenne-Backend/tree/prod).
Por favor, visitem e testem [aqui](https://perenne-gray.vercel.app/)!

Informações sobre o deploy:
- Frontend está hospedado no Vercel (https://vercel.com/).
- Backend e banco de dados estão hospedados no Render (https://render.com/).

Obs: As instruções abaixo podem não estar consistentes devido ao nosso CI/CD. Podem ser necessárias mudanças no `program.cs`.
Obs²: Algumas funcionalidades não foram implementadas (como upload de arquivos) pois só temos direito a 256mb de armazenamento.
---


# Perenne – Backend

Perenne é uma plataforma de comunidade modular e escalável para gestão de grupos, mensagens em tempo real, feed de notícias, eventos e notificações.

## Funcionalidades principais

- Chat em tempo real público e privado com histórico persistente.
- Feed de postagens públicas por grupo.
- Moderação de usuários: silenciar, banir, expulsar e bloquear.
- Autenticação e gerenciamento de usuários (registro, login, recuperação de senha, perfis).
- Criação de grupos com papéis (admin, moderador, membro) e controle de permissões.
- Chat em tempo real por grupo com WebSockets (SignalR).

## Funcionalidades planejadas

- Reações a mensagens e enquetes com votos anônimos ou múltiplos.
- Criação de eventos com edição, inscrição e lembretes.
- Sistema de notificações (menções, alertas e lembretes configuráveis).
- Postagens multimídia com upload de arquivos (imagens, vídeos, documentos).
- Sistema de tags, filtros por assunto e organização temática.
- Histórico de ações e logs de moderação por grupo.
- Gamificação com quizzes, conquistas, níveis e badges.

## Arquitetura e Tecnologias

- **.NET 9.0**: framework principal da aplicação.
- **Entity Framework Core**: ORM para abstração e persistência de dados.
- **PostgreSQL**: banco de dados relacional.
- **SignalR**: para comunicação em tempo real via WebSockets.
- **Autenticação JWT**: segura, com suporte a roles e claims.
- **OpenAPI**: documentação automática das APIs.
- **CORS configurável**: suporte a ambientes com múltiplos frontends.

## Pré-requisitos

Antes de começar, você precisará ter instalado:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Banco de dados PostgreSQL (com string de conexão válida).

## Instalação

1. **Clone o repositório**
```
git clone https://github.com/filipe-ms/Perenne-Backend
```

2. **Configure o ```appsettings.json``` com a string de conexão do banco de dados.**

3. **Restaure pacotes e compile o projeto**
```
dotnet restore
dotnet build
```

4. **Execute a aplicação**
```
dotnet run
```

## Colaboradores
- [Filipe](https://github.com/filipe-ms/)
- [Débora](https://github.com/DeboraCASouza/)
- [Yuri](https://github.com/yuricavalcanti06/)
- [Gabriel](https://github.com/andgabx/)
- [Rafael](https://github.com/rafael-zzz/)
- [Raphael](https://github.com/rafatito03/)
- [Marcelo](https://github.com/marceloh090/)

