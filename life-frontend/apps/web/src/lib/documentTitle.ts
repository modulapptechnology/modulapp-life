import { APP_NAME } from '@life/constants';
import { buildTitle } from '@life/utils';

export function documentTitle(section?: string): string {
  return buildTitle(APP_NAME, section);
}
