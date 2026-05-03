# ShellExtension AGENTS.md

ShellExtension e Launcher devem ser minimos.

Podem:

- receber caminho do Explorer;
- normalizar caminho;
- validar se e pasta;
- chamar launcher/app principal;
- registrar erro minimo de ativacao quando seguro.

Nao podem:

- escanear pasta;
- decodificar imagem;
- mover/deletar/restaurar arquivos;
- escrever journal de sessao;
- carregar pipeline de Imaging;
- carregar regras pesadas de Core;
- abrir UI propria pesada.

Context menu:

- dev fallback pode usar HKCU classico;
- produto Windows 11 deve validar MSIX/package identity + `IExplorerCommand`;
- tratar clique sobre pasta e fundo de pasta;
- tratar multiplos caminhos explicitamente.

Single-instance:

- Explorer context menu so esta completo quando ativacao redireciona para a instancia principal sem abrir multiplas janelas acidentais.
