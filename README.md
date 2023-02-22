# PB: Desafio backend Pleno (2022)

## Sobre
O seu objetivo é fazer uma WebApi desenvolvida em .net 6.0 para registros de clientes que deverão informar o nome completo e e-mail para receber informações.

### Funcionalidades

- [x] Criar usuário
- [x] Consultar usuário(s)
- [x] Editar usuário
- [x] Deletar usuário

### Tecnologias e padrões

- ASP.NET API com .NET 6
- CQS com MediatR
- FluentValidation
- Entity Framework Core 6
- XUnit, FluentAssertions, Moq
- Swagger

### Arquitetura

Essa API foi criada usando e estilo arquitetural [Vertical slice](https://jimmybogard.com/vertical-slice-architecture/). 


### Pontos de Melhorias

- Adicionar regras de negócio que faltaram
- Reutilizar lógica para mockar DbSets
- Padronizar erros de API com Problem Details
- Adcionar filtro global para tratar exceptions não tratadas
- Melhorar documentação swagger
- Configurar API com Docker
- Refatorar Nome e Email para Value Object
- Criar testes de integração