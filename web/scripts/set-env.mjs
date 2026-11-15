// Запускается на Render перед сборкой: берёт адрес API из переменной API_URL
// и записывает его в src/environments/environment.ts.
// Локально (без API_URL) ничего не меняет — остаётся http://localhost:5000.
import { writeFileSync } from 'node:fs';

const apiUrl = (process.env.API_URL ?? '').trim().replace(/\/+$/, '');

if (!apiUrl) {
  console.log('[set-env] API_URL не задан — environment.ts не меняю');
  process.exit(0);
}

writeFileSync(
  'src/environments/environment.ts',
  `export const environment = {\n  apiUrl: '${apiUrl}',\n};\n`,
);
console.log(`[set-env] apiUrl = ${apiUrl}`);
