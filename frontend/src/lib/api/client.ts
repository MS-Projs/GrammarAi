import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: `${API_URL}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

// ── Request interceptor: attach access token from Zustand persisted store ────
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (typeof window !== 'undefined') {
    try {
      const raw = localStorage.getItem('grammarai-auth');
      const token = raw ? (JSON.parse(raw) as { state?: { accessToken?: string } }).state?.accessToken : null;
      if (token) config.headers.Authorization = `Bearer ${token}`;
    } catch {
      // corrupt storage — ignore
    }
  }
  return config;
});

// ── Response interceptor: auto-refresh on 401 ────────────────────────────────
let isRefreshing = false;
let pendingQueue: Array<{ resolve: (v: string) => void; reject: (e: unknown) => void }> = [];

function processQueue(error: unknown, token: string | null) {
  pendingQueue.forEach(({ resolve, reject }) => (error ? reject(error) : resolve(token!)));
  pendingQueue = [];
}

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject });
      }).then((token) => {
        original.headers.Authorization = `Bearer ${token}`;
        return apiClient(original);
      });
    }
    original._retry = true;
    isRefreshing = true;
    try {
      const raw = localStorage.getItem('grammarai-auth');
      const refreshToken = raw ? (JSON.parse(raw) as { state?: { refreshToken?: string } }).state?.refreshToken : null;
      if (!refreshToken) throw new Error('No refresh token');
      const { data } = await axios.post(`${API_URL}/api/v1/auth/refresh`, { refreshToken });
      // Update Zustand persisted store with new tokens
      const stored = raw ? JSON.parse(raw) as { state?: Record<string, unknown> } : { state: {} };
      stored.state = { ...stored.state, accessToken: data.accessToken, refreshToken: data.refreshToken };
      localStorage.setItem('grammarai-auth', JSON.stringify(stored));
      apiClient.defaults.headers.common.Authorization = `Bearer ${data.accessToken}`;
      processQueue(null, data.accessToken);
      original.headers.Authorization = `Bearer ${data.accessToken}`;
      return apiClient(original);
    } catch (refreshError) {
      processQueue(refreshError, null);
      localStorage.removeItem('grammarai-auth');
      if (typeof document !== 'undefined') document.cookie = 'grammarai_auth=; path=/; max-age=0';
      if (typeof window !== 'undefined') window.location.href = '/login';
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

export default apiClient;
