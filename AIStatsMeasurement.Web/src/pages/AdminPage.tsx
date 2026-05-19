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
    <h1 className="admin-title">Admin Panel</h1>

    <div className="admin-grid">
      <div className="admin-card">
        <h2>Recalculate Model Responses</h2>

        <p>
          Trigger a full recalculation of all model responses using the
          current parsing, scoring and evaluation logic.
        </p>

        <button
          onClick={handleRecalculate}
          disabled={loading}
          className="recalculate-button"
        >
          {loading
            ? "Recalculating..."
            : "Recalculate All Model Responses"}
        </button>

        {message && (
          <p className="success-message">{message}</p>
        )}

        {error && (
          <p className="error-message">{error}</p>
        )}
      </div>

      <div className="admin-card">
        <h2>Export Data</h2>

        <p>
          Export all model responses, prompts, parsed responses
          and sources into a transferable dataset.
        </p>

        <button
          onClick={() =>
            alert("Export functionality not implemented yet")
          }
          className="recalculate-button"
        >
          Export All Data
        </button>
      </div>

      <div className="admin-card">
        <h2>Import Data</h2>

        <p>
          Import a previously exported dataset into the system.
        </p>

        <button
          onClick={() =>
            alert("Import functionality not implemented yet")
          }
          className="recalculate-button"
        >
          Import Data
        </button>
      </div>
    </div>
  </div>
);
}