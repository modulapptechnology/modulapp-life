import type { ApiFieldErrors, ApiProblem } from '@life/types';

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function parseFieldErrors(value: unknown): ApiFieldErrors | undefined {
  if (!isRecord(value)) {
    return undefined;
  }

  const errors: ApiFieldErrors = {};
  for (const [field, messages] of Object.entries(value)) {
    if (Array.isArray(messages)) {
      errors[field] = messages.filter((message) => typeof message === 'string');
    }
  }
  return errors;
}

export function parseProblem(text: string): ApiProblem {
  let body: unknown;
  try {
    body = JSON.parse(text);
  } catch {
    return {};
  }

  if (!isRecord(body)) {
    return {};
  }

  const problem: ApiProblem = {};
  if (typeof body.title === 'string') {
    problem.title = body.title;
  }
  if (typeof body.detail === 'string') {
    problem.detail = body.detail;
  }
  const errors = parseFieldErrors(body.errors);
  if (errors) {
    problem.errors = errors;
  }
  return problem;
}
