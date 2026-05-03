# ADR 0002 - Delete mode

## Status

Proposed

## Context

O fluxo real e selecao por exclusao. O cliente pressiona `Delete` nas fotos que nao quer, mas perda irreversivel por engano precisa ser evitada.

## Decision

Na V1, `Delete` deve mover a foto para `_deletadas_evydencia` dentro da pasta da sessao. A foto deve sair imediatamente da navegacao ativa, e o move deve acontecer com estado intermediario recuperavel.

Estados obrigatorios:

- `Active`
- `PendingDelete`
- `Deleted`
- `PendingRestore`
- `Restored`
- `Missing`
- `DeleteFailed`

Exclusao permanente fica fora do padrao e deve ser protegida por configuracao avancada futura.

## Consequences

- `Ctrl+Z` restaura movendo o arquivo de volta.
- Journal JSONL registra delete/restore.
- Falha de move nao pode corromper contadores.
- Colisoes de nome precisam de politica segura.

## Validation

- Delete no inicio, meio e fim da lista.
- Delete seguido de Ctrl+Z.
- Falha de move vira `DeleteFailed`.
- Arquivo exibido pode ser movido sem IOException causada pelo proprio app.
