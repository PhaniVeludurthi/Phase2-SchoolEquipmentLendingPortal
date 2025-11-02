import { BorrowingRequest } from '../types';
import { apiClient } from './apiClient';

export async function listRequests(): Promise<BorrowingRequest[]> {
  return apiClient.get('/api/requests');
}

export async function updateRequest(
  id: string,
  update: Partial<
    Pick<
      BorrowingRequest,
      | "status"
      | "approvedAt"
      | "approvedBy"
      | "issuedAt"
      | "dueDate"
      | "returnedAt"
      | "adminNotes"
    >
  >
): Promise<BorrowingRequest> {
  return apiClient.put(`/api/requests/${id}`, update);
}