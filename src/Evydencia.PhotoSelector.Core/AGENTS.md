# Core AGENTS.md

Core e dominio puro.

Pode conter:

- sessao;
- photo item/status;
- navegacao;
- delete state machine;
- undo;
- contadores;
- reconciliacao;
- interfaces de dominio.

Nao pode referenciar:

- WinUI;
- Windows App SDK;
- WIC;
- Win2D;
- SQLite;
- Serilog;
- Shell;
- HTTP;
- filesystem concreto quando uma interface resolver.

Toda alteracao em Core precisa ter teste unitario em `tests/Evydencia.PhotoSelector.Core.Tests` quando esse projeto existir.

Regras de review:

- delete/undo devem ser deterministas;
- contadores nao podem depender da UI;
- `PhotoStatus` deve suportar `PendingDelete`, `Deleted`, `PendingRestore`, `Restored`, `Missing` e `DeleteFailed`;
- Core nao decide detalhes de WIC, cache em disco, Explorer ou banco.
