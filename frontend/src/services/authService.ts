import { Profile } from '../types';
import { apiClient, setAuthToken } from './apiClient';

interface LoginResponse {
  token: string;
  user: Profile;
}

interface RegisterPayload {
  email: string;
  password: string;
  fullName: string;
  role: 'student' | 'staff' | 'admin';
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const res = await apiClient.post<LoginResponse>('/api/auth/login', { email, password });
  setAuthToken(res.token);
  return res;
}

export async function register(payload: RegisterPayload): Promise<{ success: true }>
{
  await apiClient.post('/api/auth/register', payload);
  return { success: true };
}

export async function me(): Promise<Profile>
{
  return apiClient.get('/api/profile/me');
}

export function logout() {
  setAuthToken(null);
}

