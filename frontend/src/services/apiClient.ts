import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

function getAuthToken(): string | null {
  try {
    return localStorage.getItem('auth_token');
  } catch {
    return null;
  }
}

type ApiResponseCamel<T> = {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
  statusCode: number;
};

type ApiResponse<T> = ApiResponseCamel<T>;

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: false,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers = config.headers ?? {};
    (config.headers as any).Authorization = `Bearer ${token}`;
  }
  return config;
});

async function request<T>(method: AxiosRequestConfig['method'], path: string, body?: unknown): Promise<T> {
  const response = await api.request<ApiResponse<T>>({ method, url: path, data: body, validateStatus: () => true });
    // Handle non-success HTTP codes
    if (response.status >= 400) {
      const apiError = response.data as any;
      const message =
        apiError?.message ??
        apiError?.Message ??
        'An unexpected error occurred';
      throw new Error(message);
    }
  const wrapped = response.data as ApiResponse<T>;

  const success = (wrapped as any).success ?? (wrapped as any).Success;
  const message = (wrapped as any).message ?? (wrapped as any).Message ?? 'Request failed';
  const errors: string[] | undefined = (wrapped as any).errors ?? (wrapped as any).Errors;
  const data: T = (wrapped as any).data ?? (wrapped as any).Data;

  if (success === false) {
    console.log(message, errors);
    const errorMessage = errors?.[0] ?? message ?? 'Request failed';
    throw new Error(errorMessage);
  }

  return data;
}

export const apiClient = {
  get: <T>(path: string) => request<T>('GET', path),
  post: <T>(path: string, body?: unknown) => request<T>('POST', path, body ?? {}),
  put: <T>(path: string, body?: unknown) => request<T>('PUT', path, body ?? {}),
  del: <T>(path: string) => request<T>('DELETE', path),
};

export function setAuthToken(token: string | null) {
  try {
    if (token) localStorage.setItem('auth_token', token);
    else localStorage.removeItem('auth_token');
  } catch {
    // ignore
  }
}

