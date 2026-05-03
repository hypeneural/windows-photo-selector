# Application AGENTS.md

Application e a camada de casos de uso.

Pode conter:

- `OpenSessionUseCase`;
- `NavigateNextPhotoUseCase`;
- `NavigatePreviousPhotoUseCase`;
- `DeleteCurrentPhotoUseCase`;
- `UndoLastDeleteUseCase`;
- `RecoverSessionUseCase`;
- `BuildLocalSelectionSummaryUseCase`;
- `PrepareViewerImageUseCase`;
- abstracoes para filesystem, journal, preview, settings e performance.

Nao pode referenciar:

- WinUI;
- Windows App SDK UI;
- XAML;
- `Window`;
- `Page`;
- controles visuais;
- cliente HTTP real na V1.

Regras:

- orquestre `Core`, `Imaging`, `Storage` e `Infrastructure` por interfaces;
- mantenha ViewModels finos;
- nao implemente IO concreto aqui;
- teste fluxos reais em `tests/Evydencia.PhotoSelector.Application.Tests`.
