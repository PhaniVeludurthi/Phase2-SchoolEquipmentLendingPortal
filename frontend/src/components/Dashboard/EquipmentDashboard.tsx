import { useState, useEffect } from 'react';
import { Equipment } from '../../types';
import { useAuth } from '../../contexts/AuthContext';
import { RequestForm } from '../Borrowing/RequestForm';
import { Search, Package, Filter } from 'lucide-react';
import { createBorrowRequest, listEquipment } from '../../services/equipmentService';

export function EquipmentDashboard() {
  const { profile } = useAuth();
  const [equipment, setEquipment] = useState<Equipment[]>([]);
  const [filteredEquipment, setFilteredEquipment] = useState<Equipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');
  const [availabilityFilter, setAvailabilityFilter] = useState<'all' | 'available' | 'unavailable'>('all');
  const [showRequestForm, setShowRequestForm] = useState(false);
  const [selectedEquipment, setSelectedEquipment] = useState<Equipment | null>(null);

  useEffect(() => {
    fetchEquipment();
  }, []);

  useEffect(() => {
    filterEquipment();
  }, [equipment, searchQuery, categoryFilter, availabilityFilter]);

  const fetchEquipment = async () => {
    setLoading(true);
    try {
      const data = await listEquipment();
      setEquipment(data);
    } finally {
      setLoading(false);
    }
  };

  const filterEquipment = () => {
    let filtered = equipment;

    if (searchQuery) {
      filtered = filtered.filter(
        (item) =>
          item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
          item.description?.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    if (categoryFilter !== 'all') {
      filtered = filtered.filter((item) => item.category === categoryFilter);
    }

    if (availabilityFilter === 'available') {
      filtered = filtered.filter((item) => item.availableQuantity > 0);
    } else if (availabilityFilter === 'unavailable') {
      filtered = filtered.filter((item) => item.availableQuantity === 0);
    }

    setFilteredEquipment(filtered);
  };

  const handleRequestEquipment = (item: Equipment) => {
    setSelectedEquipment(item);
    setShowRequestForm(true);
  };

  const handleSubmitRequest = async (equipmentId: string, quantity: number, notes: string) => {
    await createBorrowRequest({ equipmentId, quantity, notes });
    await fetchEquipment();
  };

  const getConditionColor = (condition: Equipment['condition']) => {
    switch (condition) {
      case 'excellent': return 'bg-green-100 text-green-800';
      case 'good': return 'bg-blue-100 text-blue-800';
      case 'fair': return 'bg-yellow-100 text-yellow-800';
      case 'poor': return 'bg-red-100 text-red-800';
    }
  };

  const categories = ['all', ...Array.from(new Set(equipment.map((item) => item.category)))];

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-gray-800">Available Equipment</h2>
        <p className="text-gray-600 mt-1">Browse and request equipment for borrowing</p>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div className="flex flex-col lg:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search equipment..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <div className="flex gap-4">
            <div className="relative">
              <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <select
                value={categoryFilter}
                onChange={(e) => setCategoryFilter(e.target.value)}
                className="pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 appearance-none bg-white"
              >
                {categories.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat === 'all' ? 'All Categories' : cat.charAt(0).toUpperCase() + cat.slice(1)}
                  </option>
                ))}
              </select>
            </div>

            <select
              value={availabilityFilter}
              onChange={(e) => setAvailabilityFilter(e.target.value as 'all' | 'available' | 'unavailable')}
              className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All Items</option>
              <option value="available">Available</option>
              <option value="unavailable">Unavailable</option>
            </select>
          </div>
        </div>

        <div className="text-sm text-gray-600">
          Showing {filteredEquipment.length} of {equipment.length} items
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredEquipment.map((item) => (
          <div
            key={item.id}
            className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow"
          >
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
                <p className="text-sm text-gray-600 mb-4 line-clamp-2">{item.description}</p>
              )}

              <div className="space-y-2 mb-4">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Condition:</span>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${getConditionColor(item.condition)}`}>
                    {item.condition}
                  </span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Available:</span>
                  <span className={`font-semibold ${item.availableQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {item.availableQuantity} / {item.quantity}
                  </span>
                </div>
              </div>

              <button
                onClick={() => handleRequestEquipment(item)}
                disabled={item.availableQuantity === 0}
                className="w-full py-2 px-4 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                {item.availableQuantity === 0 ? 'Unavailable' : 'Request to Borrow'}
              </button>
            </div>
          </div>
        ))}
      </div>

      {filteredEquipment.length === 0 && (
        <div className="text-center py-12 bg-white rounded-lg">
          <Package className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-600 mb-2">No equipment found</h3>
          <p className="text-gray-500">Try adjusting your search or filters</p>
        </div>
      )}

      {showRequestForm && selectedEquipment && (
        <RequestForm
          equipment={selectedEquipment}
          onSubmit={handleSubmitRequest}
          onClose={() => {
            setShowRequestForm(false);
            setSelectedEquipment(null);
          }}
        />
      )}
    </div>
  );
}
