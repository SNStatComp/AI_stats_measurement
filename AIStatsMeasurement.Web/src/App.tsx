import { BrowserRouter, Routes, Route } from "react-router-dom";
import Navbar from "./Navbar.tsx";
import Analytics from "./Analytics.tsx";
import RunSinglePrompt from "./RunSinglePrompt.tsx";

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<Analytics />} />
        <Route path="/run" element={<RunSinglePrompt />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;