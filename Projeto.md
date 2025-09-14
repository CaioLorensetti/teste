🎯 Objetivo
Criar uma API simples para gestão de solicitações de antecipação de valores, com foco em:

- Clareza de código, Clean Code

- Boas práticas de engenharia, DDD e Design Patterns

- Testes automatizados, testes de unidade

- Estrutura pensada para evolução

📚 Contexto
A LastLink permite que criadores recebam suas receitas por meio da plataforma. Para apoiar o fluxo de caixa, oferecemos a opção de antecipação de recebíveis: o criador pode solicitar a liberação antecipada de parte de seus valores futuros, mediante uma taxa.

Você foi convidado a desenvolver um serviço de antecipação que será consumido por um sistema interno. Esse serviço precisa expor uma API REST para gerenciamento dessas solicitações.

🧱 Requisitos
📌 Funcionalidades da API
Criar uma solicitação de antecipação

Input: creator_id, valor_solicitado, data_solicitacao

Aplicar taxa de 5% sobre o valor solicitado

Retornar: valor_liquido, status (default = "pendente")

Listar solicitações por creator_id

Aprovar ou recusar uma solicitação

Atualizar o status para "aprovada" ou "recusada"

(Opcional) Expor endpoint para simulação sem criar a solicitação

(GET com query params)

🔎 Regras de Negócio
O valor solicitado deve ser maior que R$100,00

Um creator não pode ter mais de uma solicitação pendente ao mesmo tempo

A taxa de antecipação é fixa: 5% sobre o valor bruto

Toda solicitação deve iniciar com status "pendente"

🧪 O que esperamos ver
Código limpo, organizado e coeso

Testes automatizados (unitários ou de integração)

Modelagem bem estruturada (ex: separação de domínio e controller)

Versionamento da API (ex: /api/v1/...)

Um README com instruções claras de como rodar o projeto localmente

🌐 (Opcional) Frontend
Se quiser demonstrar habilidades fullstack, você pode entregar uma interface simples em Angular ou React que permita:

Listar as solicitações existentes

Criar uma nova solicitação (formulário)

Aprovar ou recusar uma solicitação

🧰 Stack Sugerida
Backend: C# / .NET Core (ou stack de sua preferência)

Banco de dados: Em memória (ex: SQLite, ou mocks)

Testes: Framework de sua escolha

