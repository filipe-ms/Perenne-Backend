
<h1 align="center">Este projeto está em desenvolvimento</h1>
<h3 align="center">A versão desta branch é estável e está pronta para avaliação</h3>

<h4 align="center">[ Ver também: <a href="https://github.com/andgabx/Perenne-Frontend">Perenne-Frontend</a> ]</h4>

---
<h4 align="center">**Informação para os avaliadores**</h4>

A branch mais atual (e usada no deploy) é a [Prod](https://github.com/filipe-ms/Perenne-Backend/tree/prod).

Por favor, visitem e testem nosso site [aqui](https://perenne-gray.vercel.app/)!

Informações sobre o deploy:
- Frontend está hospedado no Vercel (https://vercel.com/).
- Backend e banco de dados estão hospedados no Render (https://render.com/).

Obs: As instruções de configuração abaixo podem não estar consistentes devido a alterações recentes em nosso CI/CD. Podem ser necessárias mudanças no `program.cs`.


---


# Perenne | Backend

Perenne é uma plataforma de comunidade modular e escalável para gestão de grupos, mensagens em tempo real, feed de notícias, eventos e notificações.

## Funcionalidades prontas para uso:

- Chat em tempo real público e privado com histórico persistente.
- Feed de postagens públicas por grupo.
- Moderação de usuários: silenciar, banir, expulsar e bloquear.
- Autenticação e gerenciamento de usuários (registro, login, recuperação de senha, perfis).
- Criação de grupos com papéis (admin, moderador, membro) e controle de permissões.
- Chat em tempo real por grupo com WebSockets (SignalR).

## No futuro, planejamos expandir o Perenne com:

- Reações a mensagens e enquetes com votos anônimos ou múltiplos.
- Criação de eventos com edição, inscrição e lembretes.
- Sistema de notificações (menções, alertas e lembretes configuráveis).
- Postagens multimídia com upload de arquivos (imagens, vídeos, documentos).
- Upload de arquivos como documentos de texto, imagens e PDF diretamente no chat.
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

## Configuração | Instalação

Obs: A configuração dependerá do ambiente de deploy. Segue aqui um tutorial para rodar a API localmente **para fins de teste.**.

1. **Clone o repositório**
```
git clone https://github.com/filipe-ms/Perenne-Backend
```

2. **Substitua `Program.cs`, por este:**
```
TODO: Fazer upload de um program.cs específico para testes locais e disponibilizar link aqui.
```

3. **Configure o ```appsettings.json``` com a string de conexão do banco de dados nesta linha.**
```
"DefaultConnection": "Host=localhost;Port=5432;Database=database;Username=postgres;Password=Password1234@"
```

4. **Restaure pacotes e compile o projeto**
```
dotnet restore
dotnet build
```

5. **Faça a migração do banco de dados**
```
dotnet ef migrations add [nome_da_migração]
dotnet ef database update
```

6. **Execute a aplicação**
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

