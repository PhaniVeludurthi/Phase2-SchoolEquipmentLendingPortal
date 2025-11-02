import { useState } from "react";
import { useAuth } from "../../contexts/AuthContext";
import { Navigation } from "./Navigation";
import { EquipmentDashboard } from "../Dashboard/EquipmentDashboard";
import { RequestsManagement } from "../Borrowing/RequestsManagement";
import { EquipmentManagement } from "../Equipment/EquipmentManagement";
import { ToastContainer } from "react-toastify";

export function MainApp() {
  const { profile } = useAuth();
  const [activeTab, setActiveTab] = useState("dashboard");

  return (
    <>
      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop={true}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="light"
      />

      <div className="min-h-screen bg-gray-50">
        <Navigation activeTab={activeTab} onTabChange={setActiveTab} />

        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {activeTab === "dashboard" && <EquipmentDashboard />}
          {activeTab === "requests" && <RequestsManagement />}
          {activeTab === "management" && profile?.role === "admin" && (
            <EquipmentManagement />
          )}
        </main>
      </div>
    </>
  );
}
