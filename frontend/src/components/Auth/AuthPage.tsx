import { useState } from 'react';
import { LoginForm } from './LoginForm';
import { SignupForm } from './SignupForm';
import { Package } from 'lucide-react';

export function AuthPage() {
  const [showLogin, setShowLogin] = useState(true);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100 flex items-center justify-center p-4">
      <div className="w-full max-w-6xl flex flex-col lg:flex-row items-center gap-12">
        <div className="flex-1 text-center lg:text-left">
          <div className="flex items-center justify-center lg:justify-start mb-4">
            <Package className="w-16 h-16 text-blue-600" />
          </div>
          <h1 className="text-4xl lg:text-5xl font-bold text-gray-800 mb-4">
            Equipment Lending Portal
          </h1>
          <p className="text-lg text-gray-600 mb-6">
            Manage and track school equipment loans efficiently. Request sports kits, lab equipment,
            cameras, musical instruments, and more.
          </p>
          <div className="space-y-3 text-left">
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-blue-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Easy borrowing requests for students and staff</p>
            </div>
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-blue-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Streamlined approval workflow for administrators</p>
            </div>
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-blue-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Real-time equipment availability tracking</p>
            </div>
          </div>
        </div>

        <div className="flex-1 flex justify-center">
          {showLogin ? (
            <LoginForm onSwitchToSignup={() => setShowLogin(false)} />
          ) : (
            <SignupForm onSwitchToLogin={() => setShowLogin(true)} />
          )}
        </div>
      </div>
    </div>
  );
}
