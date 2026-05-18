import { useState } from "react";
import "./AdminPage.css";

export default function AdminPage() {
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const handleRecalculate = async () => {
    setLoading(true);
    setMessage("");
    setError("");

    try {
          const token = localStorage.getItem("token");

          const response = await fetch(
      "https://ai-stats-measurement.lab.sspcloud.fr/api/llm/recalculate",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify([])
      }
    );

      if (!response.ok) {
        throw new Error("Failed to recalculate prompts");
      }

      setMessage("Recalculation started successfully.");
    } catch (err) {
      setError("Failed to recalculate prompts.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="admin-page">
      <div className="admin-card">
        <h1>Admin Panel</h1>
        <p>
          Recalculate all prompts and model statistics.
        </p>

        <button
          onClick={handleRecalculate}
          disabled={loading}
          className="recalculate-button"
        >
          {loading ? "Recalculating..." : "Recalculate All Prompts"}
        </button>

        {message && <p className="success-message">{message}</p>}
        {error && <p className="error-message">{error}</p>}
      </div>
    </div>
  );
}