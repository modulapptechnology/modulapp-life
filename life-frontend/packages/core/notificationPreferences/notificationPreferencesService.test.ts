import {
  channelsForPlatform,
  createMockNotificationPreferencesService,
} from './notificationPreferencesService';

describe('notificationPreferencesService', () => {
  it('ofrece push e in-app en mobile', () => {
    const platform = 'mobile';

    const channels = channelsForPlatform(platform);

    expect(channels).toEqual(['push', 'inApp']);
  });

  it('ofrece correo e in-app en web', () => {
    const platform = 'web';

    const channels = channelsForPlatform(platform);

    expect(channels).toEqual(['email', 'inApp']);
  });

  it('el mock lista todos los tipos en todos los canales, activados', async () => {
    const service = createMockNotificationPreferencesService(0);

    const preferences = await service.list();

    expect(preferences).toHaveLength(9);
    expect(preferences.every((preference) => preference.enabled)).toBe(true);
  });

  it('el mock recuerda lo guardado en la siguiente lectura', async () => {
    const service = createMockNotificationPreferencesService(0);
    await service.save({ type: 'dailySummary', channel: 'email', enabled: false });

    const preferences = await service.list();

    expect(
      preferences.find((item) => item.type === 'dailySummary' && item.channel === 'email')?.enabled,
    ).toBe(false);
  });
});
