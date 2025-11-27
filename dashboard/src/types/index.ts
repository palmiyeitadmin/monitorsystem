export * from './common';
export * from './api';
export * from './host';
export * from './check';
export * from './incident';
export * from './customer';
export * from './notification';

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
  phoneNumber?: string;
  theme?: string;
  avatarUrl?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}
