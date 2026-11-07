import { API_DEFAULT_TIMEOUT_MS } from '@life/constants';
import type { ApiResult } from '@life/types';
import { parseProblem } from '../parseProblem/parseProblem';

export type ApiClientConfig = {
  baseUrl: string;
  getToken?: () => string | undefined;
  refreshSession?: () => Promise<boolean>;
  onSessionExpired?: () => void;
  timeoutMs?: number;
  fetchFn?: typeof fetch;
  createCorrelationId?: () => string;
};

export type ApiRequestOptions = {
  body?: unknown;
  headers?: Record<string, string>;
  signal?: AbortSignal;
  timeoutMs?: number;
};

export type ApiClient = {
  get: <T>(path: string, options?: ApiRequestOptions) => Promise<ApiResult<T>>;
  post: <T>(path: string, options?: ApiRequestOptions) => Promise<ApiResult<T>>;
  put: <T>(path: string, options?: ApiRequestOptions) => Promise<ApiResult<T>>;
  patch: <T>(path: string, options?: ApiRequestOptions) => Promise<ApiResult<T>>;
  delete: <T>(path: string, options?: ApiRequestOptions) => Promise<ApiResult<T>>;
};

export const CORRELATION_HEADER = 'X-Correlation-Id';

function defaultCorrelationId(): string {
  if (typeof globalThis.crypto?.randomUUID === 'function') {
    return globalThis.crypto.randomUUID();
  }
  return `${Date.now().toString(16)}-${Math.random().toString(16).slice(2)}`;
}

function joinUrl(baseUrl: string, path: string): string {
  return `${baseUrl.replace(/\/+$/, '')}/${path.replace(/^\/+/, '')}`;
}

function mapResponse<T>(status: number, text: string): ApiResult<T> {
  if (status >= 200 && status < 300) {
    try {
      // Un cuerpo vacío (204) se entrega como null.
      return { kind: 'success', status, data: JSON.parse(text.length > 0 ? text : 'null') };
    } catch {
      return { kind: 'unexpected', status };
    }
  }

  switch (status) {
    case 400:
      return { kind: 'badRequest', status, problem: parseProblem(text) };
    case 401:
      return { kind: 'unauthorized', status };
    case 403:
      return { kind: 'forbidden', status, problem: parseProblem(text) };
    case 404:
      return { kind: 'notFound', status, problem: parseProblem(text) };
    case 409:
      return { kind: 'conflict', status, problem: parseProblem(text) };
    default:
      return status >= 500 ? { kind: 'serverError', status } : { kind: 'unexpected', status };
  }
}

export function createApiClient(config: ApiClientConfig): ApiClient {
  if (config.baseUrl.trim().length === 0) {
    throw new Error('The API base URL is not configured.');
  }

  const fetchFn = config.fetchFn ?? fetch;
  const createCorrelationId = config.createCorrelationId ?? defaultCorrelationId;
  let refreshing: Promise<boolean> | null = null;

  // Las peticiones que reciben 401 a la vez comparten una sola renovación.
  function refreshOnce(): Promise<boolean> {
    if (!config.refreshSession) {
      return Promise.resolve(false);
    }
    if (!refreshing) {
      refreshing = config.refreshSession().then(
        (renewed) => renewed,
        () => false,
      );
      void refreshing.finally(() => {
        refreshing = null;
      });
    }
    return refreshing;
  }

  async function send<T>(
    method: string,
    path: string,
    correlationId: string,
    options: ApiRequestOptions,
  ): Promise<ApiResult<T>> {
    if (options.signal?.aborted) {
      return { kind: 'cancelled' };
    }

    const controller = new AbortController();
    let timedOut = false;
    const timer = setTimeout(
      () => {
        timedOut = true;
        controller.abort();
      },
      options.timeoutMs ?? config.timeoutMs ?? API_DEFAULT_TIMEOUT_MS,
    );
    const abortFromCaller = () => controller.abort();
    options.signal?.addEventListener('abort', abortFromCaller, { once: true });

    const headers: Record<string, string> = {
      Accept: 'application/json',
      [CORRELATION_HEADER]: correlationId,
      ...options.headers,
    };
    const token = config.getToken?.();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
    if (options.body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }

    try {
      const response = await fetchFn(joinUrl(config.baseUrl, path), {
        method,
        headers,
        body: options.body === undefined ? undefined : JSON.stringify(options.body),
        signal: controller.signal,
      });
      const text = await response.text();
      return mapResponse<T>(response.status, text);
    } catch {
      if (timedOut) {
        return { kind: 'timeout' };
      }
      if (options.signal?.aborted) {
        return { kind: 'cancelled' };
      }
      return { kind: 'networkError' };
    } finally {
      clearTimeout(timer);
      options.signal?.removeEventListener('abort', abortFromCaller);
    }
  }

  async function request<T>(
    method: string,
    path: string,
    options: ApiRequestOptions = {},
  ): Promise<ApiResult<T>> {
    const correlationId = createCorrelationId();
    let result = await send<T>(method, path, correlationId, options);

    if (result.kind === 'unauthorized') {
      if (await refreshOnce()) {
        result = await send<T>(method, path, correlationId, options);
      }
      if (result.kind === 'unauthorized') {
        config.onSessionExpired?.();
      }
    }
    return result;
  }

  return {
    get: (path, options) => request('GET', path, options),
    post: (path, options) => request('POST', path, options),
    put: (path, options) => request('PUT', path, options),
    patch: (path, options) => request('PATCH', path, options),
    delete: (path, options) => request('DELETE', path, options),
  };
}
