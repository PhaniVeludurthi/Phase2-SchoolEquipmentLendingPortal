import { createContext, useContext, useEffect, useState } from 'react';
import { Profile } from '../types';
import { login as apiLogin, register as apiRegister, me as apiMe, logout as apiLogout } from '../services/authService';

interface AuthContextType {
  profile: Profile | null;
  loading: boolean;
  signIn: (email: string, password: string) => Promise<{ error: Error | null }>;
  signUp: (email: string, password: string, fullName: string, role: 'student' | 'staff' | 'admin') => Promise<{ error: Error | null }>;
  signOut: () => Promise<void>;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchMe = async () => {
    try {
      const res = await apiMe();
      setProfile(res);
    } catch {
      setProfile(null);
    }
  };

  const refreshProfile = async () => {
    await fetchMe();
  };

  useEffect(() => {
    (async () => {
      await fetchMe();
      setLoading(false);
    })();
  }, []);

  const signIn = async (email: string, password: string) => {
    try {
      const res = await apiLogin(email, password);
      setProfile(res.user);
      return { error: null };
    } catch (error) {
      return { error: error as Error };
    }
  };

  const signUp = async (email: string, password: string, fullName: string, role: 'student' | 'staff' | 'admin') => {
    try {
      await apiRegister({ email, password, fullName, role });
      return { error: null };
    } catch (error) {
      return { error: error as Error };
    }
  };

  const signOut = async () => {
    apiLogout();
    setProfile(null);
  };

  return (
    <AuthContext.Provider value={{ profile, loading, signIn, signUp, signOut, refreshProfile }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
