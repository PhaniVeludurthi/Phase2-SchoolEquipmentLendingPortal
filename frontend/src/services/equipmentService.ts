import { Equipment } from '../types';
import { apiClient } from './apiClient';

export async function listEquipment(): Promise<Equipment[]> {
  return apiClient.get('/api/equipment');
}

export async function createEquipment(data: Partial<Equipment>): Promise<Equipment> {
  return apiClient.post('/api/equipment', data);
}

export async function updateEquipment(id: string, data: Partial<Equipment>): Promise<Equipment> {
  return apiClient.put(`/api/equipment/${id}`, data);
}

export async function deleteEquipment(id: string): Promise<{ success: true }> {
  return apiClient.del(`/api/equipment/${id}`);
}

export async function createBorrowRequest(payload: { equipmentId: string; quantity: number; notes: string }): Promise<{ success: true }>
{
  return apiClient.post('/api/requests', {
    equipmentId: payload.equipmentId,
    quantity: payload.quantity,
    notes: payload.notes,
  });
}

