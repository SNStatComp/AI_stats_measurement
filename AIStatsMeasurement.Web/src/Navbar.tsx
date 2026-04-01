import { Link, useLocation } from "react-router-dom";
import "./Navbar.css";

function Navbar() {
  const location = useLocation();

  return (
    <nav className="navbar">
        <div className="navbar-inner">
      <h2 className="logo">LLM Statistics Monitoring</h2>

      <div className="nav-links">
        <Link
          to="/"
          className={location.pathname === "/" ? "active" : ""}
        >
          Analytics
        </Link>

        <Link
          to="/run"
          className={location.pathname === "/run" ? "active" : ""}
        >
          Run Single Prompt
        </Link>
      </div>
      </div>
    </nav>
  );
}

export default Navbar;