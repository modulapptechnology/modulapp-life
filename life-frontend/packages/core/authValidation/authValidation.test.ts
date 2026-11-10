import {
  hasErrors,
  validateForgotForm,
  validateLoginForm,
  validateRegisterForm,
  validateResetForm,
  validateVerifyForm,
} from './authValidation';

describe('validateLoginForm', () => {
  it('pide correo y contraseña cuando están vacíos', () => {
    const valores = { email: '  ', password: '' };

    const errores = validateLoginForm(valores);

    expect(errores).toEqual({
      email: 'auth.validation.emailRequired',
      password: 'auth.validation.passwordRequired',
    });
  });

  it('acepta un correo y una contraseña válidos', () => {
    const valores = { email: 'ana@correo.com', password: 'cualquiera' };

    const errores = validateLoginForm(valores);

    expect(hasErrors(errores)).toBe(false);
  });
});

describe('validateRegisterForm', () => {
  it('rechaza una contraseña sin mayúscula, minúscula o número', () => {
    const base = { email: 'ana@correo.com', confirmPassword: '' };

    const errores = [
      validateRegisterForm({ ...base, password: 'abcdefghij1', confirmPassword: 'abcdefghij1' }),
      validateRegisterForm({ ...base, password: 'ABCDEFGHIJ1', confirmPassword: 'ABCDEFGHIJ1' }),
      validateRegisterForm({ ...base, password: 'Abcdefghijk', confirmPassword: 'Abcdefghijk' }),
    ].map((resultado) => resultado.password);

    expect(errores).toEqual([
      'auth.validation.passwordUpper',
      'auth.validation.passwordLower',
      'auth.validation.passwordDigit',
    ]);
  });

  it('rechaza una confirmación distinta y una contraseña corta', () => {
    const valores = { email: 'ana@correo.com', password: 'Corta1', confirmPassword: 'Otra1' };

    const errores = validateRegisterForm(valores);

    expect(errores).toEqual({
      password: 'auth.validation.passwordTooShort',
      confirmPassword: 'auth.validation.passwordMismatch',
    });
  });

  it('acepta datos válidos', () => {
    const valores = {
      email: 'ana@correo.com',
      password: 'ClaveSegura1',
      confirmPassword: 'ClaveSegura1',
    };

    const errores = validateRegisterForm(valores);

    expect(hasErrors(errores)).toBe(false);
  });
});

describe('validateForgotForm', () => {
  it('rechaza un correo con formato inválido', () => {
    const valores = { email: 'sin-arroba' };

    const errores = validateForgotForm(valores);

    expect(errores).toEqual({ email: 'auth.validation.emailInvalid' });
  });
});

describe('validateVerifyForm', () => {
  it('pide el código cuando está vacío y rechaza uno que no tiene 6 dígitos', () => {
    const base = { email: 'ana@correo.com' };

    const errores = ['', '12345', '12345a', '1234567'].map(
      (code) => validateVerifyForm({ ...base, code }).code,
    );

    expect(errores).toEqual([
      'auth.validation.codeRequired',
      'auth.validation.codeInvalid',
      'auth.validation.codeInvalid',
      'auth.validation.codeInvalid',
    ]);
  });

  it('acepta un correo válido con un código de 6 dígitos', () => {
    const valores = { email: 'ana@correo.com', code: '123456' };

    const errores = validateVerifyForm(valores);

    expect(hasErrors(errores)).toBe(false);
  });
});

describe('validateResetForm', () => {
  it('exige la política de contraseña y que ambas coincidan', () => {
    const valores = { password: 'ClaveSegura1', confirmPassword: 'ClaveSegura2' };

    const errores = validateResetForm(valores);

    expect(errores).toEqual({ confirmPassword: 'auth.validation.passwordMismatch' });
  });
});
