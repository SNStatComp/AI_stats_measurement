import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import "./Navbar.css";
import statCompLogo from "./assets/StatComp.png";

type User = {
  name: string | null;
  email: string | null;
  roles: string[];
};

function Navbar() {
  const location = useLocation();
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!token) {
      setUser(null);
      return;
    }

    fetch("/api/auth/me", {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
      .then(res => {
        if (!res.ok) throw new Error("Not logged in");
        return res.json();
      })
      .then(data => setUser(data))
      .catch(() => {
        localStorage.removeItem("token");
        setUser(null);
      });
  }, []);

  const logout = () => {
    localStorage.removeItem("token");
    setUser(null);
    window.location.href = "/";
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        <div className="logo-container">
          <img src={statCompLogo} alt="StatComp Logo" className="logo-image" />
          <h2 className="logo">LLM Statistics Monitoring</h2>
        </div>
        <div className="nav-links">
          <Link to="/" className={location.pathname === "/" ? "active" : ""}>
            Analytics
          </Link>

          {user && (
            <>
              <Link to="/run" className={location.pathname === "/run" ? "active" : ""}>
                Run Single Prompt
              </Link>

              <Link
                to="/run-multiple"
                className={location.pathname === "/run-multiple" ? "active" : ""}
              >
                Run Multiple Prompts
              </Link>

              <Link
                to="/create-prompt"
                className={location.pathname === "/create-prompt" ? "active" : ""}
              >
                Create Prompt
              </Link>

              <Link
                to="/admin"
                className={location.pathname === "/admin" ? "active" : ""}
              >
                Admin
              </Link>
            </>
          )}

          <Link
            to="/model-responses"
            className={location.pathname === "/model-responses" ? "active" : ""}
          >
            Tableview
          </Link>

          <div className="auth-section">
            {user ? (
              <>
                <span className="logged-in-user">
                  {user.email ?? user.name}
                </span>

                <button className="logout-button" onClick={logout}>
                  Logout
                </button>
              </>
            ) : (
              <Link
                to="/login"
                className={location.pathname === "/login" ? "active" : ""}
              >
                Login
              </Link>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;