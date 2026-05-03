# Benchmarks AGENTS.md

Benchmarks medem budgets de performance V1.

Use BenchmarkDotNet quando o projeto existir.

Medir:

- scan 50, 500 e 2.000 JPEGs;
- decode preview de JPEG grande;
- navegacao cached;
- navegacao uncached;
- delete/undo em volume;
- journal append;
- cache hit/miss;
- memoria sob navegacao sustentada.

Regras:

- nao misturar benchmark com teste unitario;
- salvar resultados em `artifacts/performance` ou local documentado;
- comparar baseline quando houver;
- nao declarar melhoria de performance sem medida ou justificativa tecnica direta.
