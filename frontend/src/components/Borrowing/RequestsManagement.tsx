import { useState, useEffect } from "react";
import { BorrowingRequest } from "../../types";
import { useAuth } from "../../contexts/AuthContext";
import {
  CheckCircle,
  XCircle,
  Package as PackageIcon,
  Clock,
} from "lucide-react";
import { listRequests, updateRequest } from "../../services/requestsService";
import { toast } from "react-toastify";

export function RequestsManagement() {
  const { profile } = useAuth();
  const [requests, setRequests] = useState<BorrowingRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<
    "all" | "pending" | "approved" | "issued"
  >("all");
  const isStaffOrAdmin = profile?.role === "staff" || profile?.role === "admin";
  const [updatingRequestId, setUpdatingRequestId] = useState<string | null>(
    null
  );

  useEffect(() => {
    fetchRequests();
  }, []);

  const fetchRequests = async () => {
    setLoading(true);
    try {
      const data = await listRequests();
      setRequests(data);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load requests";
      toast.error(message);
      console.error("Error fetching requests:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (
    requestId: string,
    status: string,
    adminNotes?: string
  ) => {
    const updateData: any = { status };
    // Prevent multiple clicks
    if (updatingRequestId === requestId) {
      return;
    }
    setUpdatingRequestId(requestId);
    if (status === "approved") {
      updateData.approvedAt = new Date().toISOString();
      updateData.approvedBy = profile?.id;
    }

    if (status === "issued") {
      updateData.issuedAt = new Date().toISOString();
      const dueDate = new Date();
      dueDate.setDate(dueDate.getDate() + 7);
      updateData.dueDate = dueDate.toISOString();
    }

    if (status === "returned") {
      updateData.returnedAt = new Date().toISOString();
    }

    if (adminNotes) {
      updateData.adminNotes = adminNotes;
    }
    try {
      await updateRequest(requestId, updateData);
      const successMessages: Record<string, string> = {
        approved: "✅ Request approved successfully",
        rejected: "❌ Request rejected",
        issued: "📦 Equipment marked as issued",
        returned: "✅ Equipment marked as returned",
        cancelled: "🚫 Request cancelled",
      };
      toast.success(successMessages[status] || "Request updated successfully");

      // Refresh the requests list
      await fetchRequests();
    } catch (err) {
      // Handle different error types
      let errorMessage = "An error occurred. Please try again!";

      if (err instanceof Error) {
        errorMessage = err.message;

        // Parse specific API error messages
        if (errorMessage.includes("Insufficient equipment quantity")) {
          errorMessage =
            "⚠️ Not enough equipment available to approve this request";
        } else if (errorMessage.includes("Invalid status transition")) {
          errorMessage =
            "⚠️ Cannot perform this action on current request status";
        } else if (errorMessage.includes("Equipment not found")) {
          errorMessage = "⚠️ Equipment no longer exists";
        } else if (errorMessage.includes("Request not found")) {
          errorMessage = "⚠️ Request not found";
        } else if (errorMessage.includes("409")) {
          errorMessage =
            "⚠️ This request was just updated. Please refresh and try again";
        } else if (
          errorMessage.includes("Network") ||
          errorMessage.includes("fetch")
        ) {
          errorMessage = "🌐 Network error. Please check your connection";
        }
      }

      toast.error(errorMessage);
      console.error("Error updating request:", err);
    } finally {
      setUpdatingRequestId(null);
    }
  };

   const handleReject = (requestId: string) => {
    const notes = prompt('Please provide a reason for rejection:');
    
    // User cancelled the prompt
    if (notes === null) {
      return;
    }
    
    // User submitted empty reason
    if (notes.trim() === '') {
      toast.warning('Please provide a rejection reason');
      return;
    }
    
    handleStatusUpdate(requestId, 'rejected', notes);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "pending":
        return "bg-yellow-100 text-yellow-800";
      case "approved":
        return "bg-green-100 text-green-800";
      case "rejected":
        return "bg-red-100 text-red-800";
      case "issued":
        return "bg-blue-100 text-blue-800";
      case "returned":
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const filteredRequests = requests.filter(
    (req) =>
      filter === "all" ||
      req.status.toLocaleLowerCase() === filter.toLocaleLowerCase()
  );

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
        <h2 className="text-2xl font-bold text-gray-800">Borrowing Requests</h2>
        <p className="text-gray-600 mt-1">
          {isStaffOrAdmin
            ? "Review and manage all borrowing requests"
            : "View your borrowing history"}
        </p>
      </div>

      <div className="flex gap-2 flex-wrap">
        <button
          onClick={() => setFilter("all")}
          className={`px-4 py-2 rounded-md transition-colors ${
            filter === "all"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          All
        </button>
        <button
          onClick={() => setFilter("pending")}
          className={`px-4 py-2 rounded-md transition-colors ${
            filter === "pending"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          Pending
        </button>
        <button
          onClick={() => setFilter("approved")}
          className={`px-4 py-2 rounded-md transition-colors ${
            filter === "approved"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          Approved
        </button>
        <button
          onClick={() => setFilter("issued")}
          className={`px-4 py-2 rounded-md transition-colors ${
            filter === "issued"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          Issued
        </button>
      </div>

      <div className="space-y-4">
        {filteredRequests.map((request) => (
          <div key={request.id} className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-start justify-between mb-4">
              <div className="flex items-start gap-4 flex-1">
                <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                  <PackageIcon className="w-6 h-6 text-blue-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-semibold text-gray-800 text-lg">
                    {request.equipment?.name}
                  </h3>
                  <p className="text-sm text-gray-600 mt-1">
                    {isStaffOrAdmin
                      ? `Requested by: ${request.user?.fullName} (${request.user?.email})`
                      : `Approved by: ${
                          request.approvedBy
                            ? request.approver?.fullName
                            : "N/A"
                        }`}
                  </p>
                  <p className="text-sm text-gray-500">
                    Quantity: {request.quantity}
                  </p>
                  {request.notes && (
                    <p className="text-sm text-gray-600 mt-2">
                      <span className="font-medium">Notes:</span>{" "}
                      {request.notes}
                    </p>
                  )}
                  {request.adminNotes && (
                    <p className="text-sm text-gray-600 mt-1">
                      <span className="font-medium">Admin Notes:</span>{" "}
                      {request.adminNotes}
                    </p>
                  )}
                </div>
              </div>
              <span
                className={`px-3 py-1 rounded-full text-xs font-medium capitalize ${getStatusColor(
                  request.status
                )}`}
              >
                {request.status}
              </span>
            </div>

            <div className="flex items-center gap-6 text-sm text-gray-600 mb-4">
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4" />
                <span>
                  Requested:{" "}
                  {new Date(request.requestedAt).toLocaleDateString()}
                </span>
              </div>
              {request.dueDate &&
                request.status.toLocaleLowerCase() === "issued" && (
                  <div className="flex items-center gap-2">
                    <Clock className="w-4 h-4" />
                    <span>
                      Due: {new Date(request.dueDate).toLocaleDateString()}
                    </span>
                  </div>
                )}
            </div>

            {isStaffOrAdmin &&
              request.status.toLocaleLowerCase() === "pending" && (
                <div className="flex gap-2">
                  <button
                    onClick={() => handleStatusUpdate(request.id, "approved")}
                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors"
                  >
                    <CheckCircle className="w-4 h-4" />
                    Approve
                  </button>
                  <button
                    onClick={() => {
                      const notes = prompt("Reason for rejection:");
                      if (notes !== null) {
                        handleStatusUpdate(request.id, "rejected", notes);
                      }
                    }}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors"
                  >
                    <XCircle className="w-4 h-4" />
                    Reject
                  </button>
                </div>
              )}

            {isStaffOrAdmin &&
              request.status.toLocaleLowerCase() === "approved" && (
                <button
                  onClick={() => handleStatusUpdate(request.id, "issued")}
                  className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                >
                  <PackageIcon className="w-4 h-4" />
                  Mark as Issued
                </button>
              )}

            {isStaffOrAdmin &&
              request.status.toLocaleLowerCase() === "issued" && (
                <button
                  onClick={() => handleStatusUpdate(request.id, "returned")}
                  className="flex items-center gap-2 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 transition-colors"
                >
                  <CheckCircle className="w-4 h-4" />
                  Mark as Returned
                </button>
              )}
          </div>
        ))}

        {filteredRequests.length === 0 && (
          <div className="text-center py-12 bg-white rounded-lg">
            <PackageIcon className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-600 mb-2">
              No requests found
            </h3>
            <p className="text-gray-500">
              {filter === "all"
                ? "No borrowing requests yet"
                : `No ${filter} requests`}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
