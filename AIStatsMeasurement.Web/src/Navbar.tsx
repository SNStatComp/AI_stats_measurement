import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import "./Navbar.css";

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
    window.location.href = "/login";
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        <h2 className="logo">LLM Statistics Monitoring</h2>

        <div className="nav-links">
          <Link to="/" className={location.pathname === "/" ? "active" : ""}>
            Analytics
          </Link>

          <Link to="/run" className={location.pathname === "/run" ? "active" : ""}>
            Run Single Prompt
          </Link>

          <Link to="/run-multiple" className={location.pathname === "/run-multiple" ? "active" : ""}>
            Run Multiple Prompts
          </Link>

          <Link to="/create-prompt" className={location.pathname === "/create-prompt" ? "active" : ""}>
            Create Prompt
          </Link>

          <Link to="/model-responses" className={location.pathname === "/model-responses" ? "active" : ""}>
            Model Responses
          </Link>

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
            <Link to="/login" className={location.pathname === "/login" ? "active" : ""}>
              Login
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
}

export default Navbar;