# Imaging AGENTS.md

Imaging cuida de JPEG, EXIF, decode target, cache e prefetch.

Regras obrigatorias:

- nunca decodificar full-res no modo fit normal;
- usar `DisplayContext` ou entrada equivalente de tamanho/DPI;
- aplicar EXIF orientation antes de calcular fit;
- cancelar decode obsoleto;
- nao manter `FileStream` aberto depois do decode;
- abrir JPEG com `FileShare.ReadWrite | FileShare.Delete`;
- nao decidir estado `Deleted` ou `Restored`;
- thumbnail/cache nao pode atrasar a primeira imagem.

Toda alteracao de decode/cache precisa ter teste em `tests/Evydencia.PhotoSelector.Imaging.Tests` quando esse projeto existir.

Qualquer PR que exiba imagem deve explicar:

- onde target decode size e calculado;
- como DPI/rasterization scale entra;
- por que full-res decode nao e usado;
- como tarefas antigas sao canceladas.
