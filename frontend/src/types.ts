export interface Profile {
  id: string;
  email: string;
  fullName: string;
  role: 'student' | 'staff' | 'admin';
  createdAt?: string;
  updatedAt?: string;
}

export interface Equipment {
  id: string;
  name: string;
  category: 'sports' | 'lab' | 'camera' | 'instrument' | 'project' | 'other';
  description?: string;
  condition: 'excellent' | 'good' | 'fair' | 'poor';
  quantity: number;
  availableQuantity: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface BorrowingRequest {
  id: string;
  userId: string;
  equipmentId: string;
  quantity: number;
  status: 'pending' | 'approved' | 'rejected' | 'issued' | 'returned';
  requestedAt: string;
  approvedAt: string | null;
  approvedBy: string | null;
  issuedAt: string | null;
  dueDate: string | null;
  returnedAt: string | null;
  notes?: string;
  adminNotes?: string;
  equipment?: Equipment;
  user?: Profile;
  approver?: Profile;
}

