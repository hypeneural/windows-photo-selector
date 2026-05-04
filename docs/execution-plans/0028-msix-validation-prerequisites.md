# Execucao 0028 - Pre-requisitos verificaveis para MSIX single-instance

## Objetivo

Fechar a ambiguidade operacional restante da fatia MSIX: detectar se o certificado dev esta confiavel no store correto, se o pacote esta assinado/instalado, e se o smoke de single-instance pode rodar. Esta fatia nao implementa `ShellExtension`/`IExplorerCommand`; ela prepara a validacao empacotada exigida antes do menu moderno.

## Arquivos que serao lidos

- `AGENTS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/context-menu.md`
- `docs/execution-progress.md`
- `docs/context-menu.md`
- `tools/package-msix-dev.ps1`
- `tools/install-msix-dev.ps1`
- `tools/trust-msix-dev-cert-admin.ps1`
- `tools/smoke-msix-single-instance.ps1`

Documentacao oficial consultada:

- Microsoft Learn, `MSIX troubleshooting guide`: https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide
- Microsoft Learn, `Create a certificate for package signing`: https://learn.microsoft.com/en-gb/windows/msix/package/create-certificate-package-signing
- Microsoft Learn, `Settings for developers`: https://learn.microsoft.com/en-us/windows/apps/get-started/developer-mode-features-and-debugging
- Microsoft Learn, `Create a single-instanced WinUI app with C#`: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance

## Camada afetada

- Tools
- Docs
- Packaging

## Restricoes da tarefa

- V1 continua offline.
- Sem API Laravel.
- Sem PDV.
- Sem upload.
- Sem RAW.
- Sem Electron/WebView.
- Sem implementar ShellExtension ainda.

## Plano em passos pequenos

1. Ajustar o script de confianca dev para priorizar `LocalMachine\TrustedPeople`, alinhado a documentacao oficial de MSIX.
2. Manter `LocalMachine\Root` como opcao explicita, nao padrao.
3. Criar script de diagnostico de pre-requisitos MSIX dev.
4. Validar sintaxe dos scripts.
5. Rodar o diagnostico no ambiente atual e registrar o bloqueio concreto.
6. Rodar build/testes para garantir que nada quebrou.

## Testes necessarios

- Parser PowerShell dos scripts alterados/criados.
- `tools/check-msix-dev-prereqs.ps1`.
- `tools/format.ps1`.
- `tools/build.ps1`.
- `tools/test.ps1`.

## Criterio de aceite

- O diagnostico informa assinatura, stores de certificado, instalacao do pacote e permissao de administrador.
- O script de trust nao adiciona certificado em `LocalMachine\Root` por padrao.
- `F0-19` continua pendente se o pacote nao estiver instalado e o smoke nao passar.

## Riscos

- Instalar certificado em stores de maquina exige PowerShell elevado.
- Um MSIX self-signed em ambiente real deve ser substituido por certificado confiavel de produto.
- O smoke instalado nao pode rodar enquanto `Add-AppxPackage` falhar.

