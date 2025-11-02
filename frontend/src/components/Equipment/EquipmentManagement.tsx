import { useState, useEffect } from 'react';
import { Equipment } from '../../types';
import { EquipmentForm } from './EquipmentForm';
import { Plus, Edit, Trash2, Package } from 'lucide-react';
import { createEquipment, deleteEquipment, listEquipment, updateEquipment } from '../../services/equipmentService';

export function EquipmentManagement() {
  const [equipment, setEquipment] = useState<Equipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingEquipment, setEditingEquipment] = useState<Equipment | undefined>();

  useEffect(() => {
    fetchEquipment();
  }, []);

  const fetchEquipment = async () => {
    setLoading(true);
    try {
      const data = await listEquipment();
      setEquipment(data);
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = async (data: Partial<Equipment>) => {
    await createEquipment(data);
    await fetchEquipment();
  };

  const handleUpdate = async (data: Partial<Equipment>) => {
    if (!editingEquipment) return;
    await updateEquipment(editingEquipment.id, data);
    await fetchEquipment();
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this equipment?')) return;
    try {
      await deleteEquipment(id);
      await fetchEquipment();
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Cannot delete equipment');
    }
  };

  const openEditForm = (eq: Equipment) => {
    setEditingEquipment(eq);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingEquipment(undefined);
  };

  const getConditionColor = (condition: Equipment['condition']) => {
    switch (condition) {
      case 'excellent': return 'bg-green-100 text-green-800';
      case 'good': return 'bg-blue-100 text-blue-800';
      case 'fair': return 'bg-yellow-100 text-yellow-800';
      case 'poor': return 'bg-red-100 text-red-800';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-800">Equipment Management</h2>
          <p className="text-gray-600 mt-1">Add, edit, or remove equipment from inventory</p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors"
        >
          <Plus className="w-5 h-5" />
          Add Equipment
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {equipment.map((item) => (
          <div key={item.id} className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow">
            <div className="p-6">
              <div className="flex items-start justify-between mb-4">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                    <Package className="w-6 h-6 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-800">{item.name}</h3>
                    <span className="text-xs text-gray-500 capitalize">{item.category}</span>
                  </div>
                </div>
              </div>

              {item.description && (
                <p className="text-sm text-gray-600 mb-3 line-clamp-2">{item.description}</p>
              )}

              <div className="space-y-2">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Condition:</span>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${getConditionColor(item.condition)}`}>
                    {item.condition}
                  </span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Available:</span>
                  <span className="font-semibold text-gray-800">
                    {item.availableQuantity} / {item.quantity}
                  </span>
                </div>
              </div>

              <div className="flex gap-2 mt-4 pt-4 border-t">
                <button
                  onClick={() => openEditForm(item)}
                  className="flex-1 flex items-center justify-center gap-2 px-3 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition-colors"
                >
                  <Edit className="w-4 h-4" />
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(item.id)}
                  className="flex-1 flex items-center justify-center gap-2 px-3 py-2 bg-red-100 text-red-700 rounded-md hover:bg-red-200 transition-colors"
                >
                  <Trash2 className="w-4 h-4" />
                  Delete
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {equipment.length === 0 && (
        <div className="text-center py-12">
          <Package className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-600 mb-2">No equipment yet</h3>
          <p className="text-gray-500">Add your first equipment item to get started</p>
        </div>
      )}

      {showForm && (
        <EquipmentForm
          equipment={editingEquipment}
          onSubmit={editingEquipment ? handleUpdate : handleAdd}
          onClose={closeForm}
        />
      )}
    </div>
  );
}
