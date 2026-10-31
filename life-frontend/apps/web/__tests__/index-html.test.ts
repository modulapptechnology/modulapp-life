import { readFileSync } from 'fs';
import { join } from 'path';
import { APP_NAME } from '@life/constants';

describe('index.html de la web', () => {
  it('el título de la pestaña es el nombre de la aplicación', () => {
    const html = readFileSync(join(__dirname, '..', 'index.html'), 'utf8');

    const titulo = /<title>(.*?)<\/title>/.exec(html)?.[1];

    expect(titulo).toBe(APP_NAME);
  });
});
