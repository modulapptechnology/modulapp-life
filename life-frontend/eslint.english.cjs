// Regla compartida de "código en inglés" (igual que el backend): los identificadores
// llevan solo letras ASCII, dígitos, _ y $. Bloquea tildes y ñ, que delatan nombres en español.
module.exports = {
  rules: {
    'id-match': [
      'error',
      '^[A-Za-z0-9_$]+$',
      { properties: true, classFields: true, onlyDeclarations: false, ignoreDestructuring: false },
    ],
  },
};
