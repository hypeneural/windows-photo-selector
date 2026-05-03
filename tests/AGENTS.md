# Tests AGENTS.md

Testes devem proteger comportamento, nao detalhes acidentais.

Organizacao esperada:

- `Evydencia.PhotoSelector.Core.Tests`;
- `Evydencia.PhotoSelector.Imaging.Tests`;
- `Evydencia.PhotoSelector.Storage.Tests`;
- `Evydencia.PhotoSelector.IntegrationTests`;
- `Evydencia.PhotoSelector.UiSmokeTests`.

Regras:

- Core deve ter unit tests para toda regra nova;
- Imaging deve testar EXIF, target sizing, cache e file handle;
- Storage deve testar journal, settings e file move/restore;
- IntegrationTests devem usar pastas temporarias;
- testes nao devem depender de fotos reais de clientes;
- dados de teste devem ser gerados ou sinteticos;
- falhas de IO devem ser testadas quando a tarefa tocar delete/undo.

Use `.\tools\test.ps1` quando a solution existir.
