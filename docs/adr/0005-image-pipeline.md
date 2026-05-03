# ADR 0005 - Image pipeline

## Status

Proposed

## Context

A V1 precisa exibir JPEGs com qualidade alta e navegacao fluida, sem decodificar imagens gigantes desnecessariamente e sem bloquear `Delete`.

## Decision

V1 usa WIC/WinUI pipeline adequado para JPEG, com decode no tamanho de exibicao.

Regras:

- calcular `DisplayContext`;
- aplicar DPI/rasterization scale;
- aplicar EXIF orientation antes do fit;
- usar fit-contain;
- adicionar margem de qualidade de 1.15x a 1.35x;
- nao usar full-res decode no fit normal;
- liberar streams apos decode;
- abrir leitura com `FileShare.ReadWrite | FileShare.Delete`;
- cancelar decodes obsoletos.

Win2D/Direct2D fica como evolucao V1.5/V2 atras de abstracao.

## Consequences

- Melhor uso de memoria em JPEGs grandes.
- Delete nao deve falhar por handle preso pelo viewer.
- Zoom 100% futuro pode pedir full-res explicitamente.

## Validation

- Foto 24 MP em monitor 1080p/4K nao decodifica full-res no fit.
- EXIF orientation e respeitado.
- Segurar seta nao acumula fila obsoleta.
- Foto exibida pode ser movida imediatamente.
