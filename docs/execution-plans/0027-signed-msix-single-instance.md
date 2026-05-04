# Execucao 0027 - MSIX assinado dev e validacao de single-instance empacotado

## Objetivo

Validar o `single-instance` com app realmente instalado/registrado por MSIX assinado de desenvolvimento. A fatia deve resolver ou documentar tecnicamente o bloqueio F0-19 antes de iniciar `ShellExtension`/`IExplorerCommand` moderno.

Esta fatia nao implementa API, PDV, upload, RAW, cache pesado nem ShellExtension. O foco e empacotamento, assinatura, instalacao e smoke de duas ativacoes reais.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/context-menu.md`
- `docs/execution-progress.md`
- `docs/context-menu.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `src/Evydencia.PhotoSelector.App/Evydencia.PhotoSelector.App.csproj`
- `src/Evydencia.PhotoSelector.App/Package.appxmanifest`
- `src/Evydencia.PhotoSelector.App/Activation/AppInstanceCoordinator.cs`
- `tools/*.ps1`

Documentacao oficial consultada:

- Microsoft Learn, `Package your app using single-project MSIX`: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix
- Microsoft Learn, `Windows App SDK deployment guide for framework-dependent packaged apps`: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-packaged-apps
- Microsoft Learn, `Sign your MSIX package: end-to-end guide`: https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide
- Microsoft Learn, `MSIX troubleshooting guide`: https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide
- Microsoft Learn, `Create a single-instanced WinUI app with C#`: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance
- Microsoft Learn, `Integrate a packaged desktop app with File Explorer`: https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/integrate-packaged-app-with-file-explorer
- Microsoft Learn, `IExplorerCommand interface`: https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-iexplorercommand

## Camada afetada

- App
- Packaging
- Tools
- Docs

## Restricoes da tarefa

- V1 continua offline.
- Sem API Laravel.
- Sem PDV.
- Sem upload.
- Sem RAW.
- Sem Electron/WebView.
- Sem bloquear UI thread.
- Sem quebrar separacao de camadas.
- Sem excluir permanentemente por padrao.
- Nao implementar ShellExtension ainda.

## Plano em passos pequenos

1. Criar certificado dev em `CurrentUser\My` e exportar `.cer`/`.pfx` em `packaging/certificates-dev`.
2. Confiar o `.cer` no `CurrentUser\TrustedPeople` para instalacao local.
3. Criar script de empacotamento dev MSIX assinavel/repetivel.
4. Gerar pacote MSIX x64 em `artifacts/packages`.
5. Instalar o MSIX assinado com `Add-AppxPackage`.
6. Executar smoke de duas ativacoes reais e medir processos vivos.
7. Atualizar docs/progresso com resultado ou bloqueio.

## Testes necessarios

- `tools/format.ps1`.
- `tools/build.ps1`.
- `tools/build.ps1 -Platform x64`.
- `tools/test.ps1`.
- Smoke MSIX:
  - pacote instalado;
  - app inicia sem `REGDB_E_CLASSNOTREG`;
  - segunda ativacao nao cria segunda instancia viva;
  - app e encerrado no final.

## Criterio de aceite

- MSIX dev assinado e gerado ou bloqueio documentado com erro concreto.
- Certificado dev fica documentado e removivel/renovavel.
- App instalado inicia com package identity.
- Duas ativacoes nao deixam duas instancias principais vivas.
- F0-19 e marcado como concluido apenas se o smoke empacotado passar.

## Riscos

- Geracao/importacao de certificado pode exigir permissao fora do sandbox. Mitigacao: usar `CurrentUser` e documentar se houver bloqueio.
- Build MSIX pode exigir ferramentas SDK/SignTool. Mitigacao: localizar SignTool no Windows SDK instalado ou registrar ausencia.
- `Add-AppxPackage` pode continuar bloqueado por politica local. Mitigacao: registrar ActivityId/erro e manter F0-19 pendente.
- Smoke pode abrir UI e precisar encerrar processo. Mitigacao: script encerra processos do app ao final.

## Comandos a rodar

```powershell
.\tools\format.ps1
.\tools\build.ps1
.\tools\build.ps1 -Platform x64
.\tools\test.ps1
```

## Saida esperada no final

- Arquivos alterados.
- Camada alterada.
- Testes adicionados/atualizados.
- Comandos executados.
- Resultado do MSIX/smoke.
- Riscos restantes.
- Desvios do plano ou de `AGENTS.md`, se houver.

## Resultado parcial

- `tools/package-msix-dev.ps1` criado para gerar MSIX dev assinado com certificado `CN=EvydenciaDev`.
- O pacote foi gerado em `artifacts/packages/Evydencia.PhotoSelector.App_1.0.0.0_x64_Test/Evydencia.PhotoSelector.App_1.0.0.0_x64.msix`.
- O script agora importa o PFX em `CurrentUser\My` e assina por `PackageCertificateThumbprint`, evitando warnings de importacao de PFX durante o publish.
- A assinatura passou em `Get-AuthenticodeSignature` depois de confiar o certificado em `CurrentUser\Root`.
- `Add-AppxPackage` ainda falhou com `0x800B0109` porque a raiz do certificado nao esta confiavel para o provedor AppX no nivel exigido pelo Windows.
- `tools/trust-msix-dev-cert-admin.ps1` foi criado para confiar o certificado em `LocalMachine\Root` e `LocalMachine\TrustedPeople` via PowerShell elevado.
- `tools/install-msix-dev.ps1` foi criado para instalar/reinstalar o MSIX e explicar o caminho de correcao quando ocorrer `0x800B0109`.
- `tools/smoke-msix-single-instance.ps1` foi criado para validar duas ativacoes reais do app instalado.
- `tools/smoke-msix-single-instance.ps1` falha corretamente quando o pacote ainda nao esta instalado.

## Decisao

`F0-19` permanece pendente ate o pacote instalar e o smoke `tools/smoke-msix-single-instance.ps1` passar. O projeto `ShellExtension`/`IExplorerCommand` moderno nao deve ser iniciado antes dessa validacao.
