import axios from 'axios';

/**
 * The single axios instance used for all backend calls.
 * Base URL: VITE_API_URL from .env, or the backend's default dev port.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5266',
  timeout: 60_000,
});

/**
 * Turns any thrown error into a message we can show to the user.
 * The backend returns plain-text explanations for 400/401/503/500 —
 * surface those directly when present.
 */
export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data: unknown = error.response?.data;
    if (typeof data === 'string' && data.trim().length > 0) return data;
    if (error.response) return `The server answered with status ${error.response.status}.`;
    return 'Cannot reach the backend. Is it running? (dotnet run in the project root)';
  }
  return 'Something unexpected went wrong.';
}
