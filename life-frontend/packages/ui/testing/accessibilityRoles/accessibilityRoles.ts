type RenderedNode = {
  props?: Record<string, unknown>;
  children?: (RenderedNode | string)[] | null;
};

// Roles de accesibilidad de todo el árbol dibujado, incluidos los de vistas que Testing Library
// no considera accesibles (View con accessibilityRole sin accessible).
export function collectRoles(tree: unknown): string[] {
  const roles: string[] = [];
  const visit = (node: RenderedNode | string | null | undefined) => {
    if (!node || typeof node === 'string') return;
    const role = node.props?.accessibilityRole;
    if (typeof role === 'string') roles.push(role);
    node.children?.forEach(visit);
  };
  (Array.isArray(tree) ? tree : [tree]).forEach(visit);

  return roles;
}
