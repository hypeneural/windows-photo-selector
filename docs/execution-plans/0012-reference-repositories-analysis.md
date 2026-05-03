# Execucao 0012 - Analise de repositorios de referencia

## Objetivo

Analisar os repositorios de referencia baixados em `C:\Users\Usuario\Desktop\ideias` e registrar, sem copiar codigo, quais ideias devem orientar as proximas fatias do viewer, principalmente F2-03 Mostrar primeira foto no viewer.

## Arquivos a ler

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `docs/execution-progress.md`
- Repositorios em `C:\Users\Usuario\Desktop\ideias`

## Camada afetada

- Docs

## Restricoes da tarefa

- Nao copiar codigo de terceiros.
- Nao introduzir dependencias.
- Nao implementar API, PDV, upload, RAW, Electron, WebView ou IA na V1.
- Preservar o desenho V1: WinUI 3 + Windows App SDK, JPEG-only, local-first/offline.
- Manter Delete/Undo planejado: mover para `_deletadas_evydencia`, estados explicitos, journal JSONL e `Ctrl+Z`.

## Plano em passos pequenos

1. Confirmar quais repositorios existem localmente.
2. Ler README/licenca e pontos tecnicos relevantes de cada projeto.
3. Comparar as ideias com o estado atual do repositorio.
4. Separar ideias uteis, ideias fora de escopo e riscos de licenca.
5. Mapear classes e servicos nossos impactados.
6. Escrever a documentacao principal.
7. Fazer validacao leve de conteudo e caminhos.

## Testes necessarios

- Nao ha teste automatizado necessario, pois a entrega e somente documentacao.

## Criterio de aceite

- A doc lista as referencias analisadas e ausentes.
- A doc separa o que vale usar como ideia do que nao serve para a V1.
- A doc aponta riscos de licenca.
- A doc mapeia impactos nas camadas/classes do nosso repositorio.
- A doc traz recomendacoes objetivas para F2-03.

## Riscos

- `Windows-classic-samples / WicViewerD2D` e `cull` nao estavam presentes localmente na pasta de ideias; foram tratados como referencias externas oficiais/primarias, sem analise de codigo local.
- Repositorios com GPL/LGPL devem continuar apenas como inspiracao conceitual, salvo revisao juridica explicita.

## Comandos a rodar

```powershell
Get-ChildItem C:\Users\Usuario\Desktop\ideias
Get-Content <arquivos README/licenca relevantes>
Select-String <padroes tecnicos relevantes>
```

## Resultado

- Criada a documentacao `docs/reference-repositories-analysis.md`.
- Complemento aprofundado registrado em `docs/reference-repositories-analysis.md` com ideias aceitas de FlyPhotos, ImageGlass, qimgv, Oculante e JPEGView: fila de decode, navigation burst, cache por janela, cache admission, guard de EXIF orientation dupla, taxonomia de erros, signature probe, atalhos por comando e `FileMoveService` rico.
- Plano mestre atualizado em `docs/evydencia-escolher-fotos-plano-implementacao.md` com as melhorias que entram na execucao futura.
- Nenhum codigo de produto foi alterado.
- Nenhuma dependencia foi adicionada.
