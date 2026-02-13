import axios from 'axios';

// Use relative URLs so Nginx can proxy to backend
// In development, VITE_API_URL can be set to override
const API_BASE_URL = import.meta.env.VITE_API_URL || '';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface ShortenUrlRequest {
  url: string;
}

export const shortenUrl = async (url: string): Promise<string> => {
  const response = await api.post<string>('/shorten', { url });
  const code = response.data.replace(/^"|"$/g, '');
  // Build the URL using the current frontend domain
  const baseUrl = window.location.origin;
  return `${baseUrl}/${code}`;
};

export default api;
