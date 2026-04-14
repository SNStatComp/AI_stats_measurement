import { BrowserRouter, Routes, Route } from "react-router-dom";
import Navbar from "./Navbar.tsx";
import Analytics from "./Analytics.tsx";
import RunSinglePrompt from "./RunSinglePrompt.tsx";
import RunMultiplePrompts from "./RunMultiplePrompts.tsx";
import CreatePrompt from "./CreatePrompt.tsx";

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<Analytics />} />
        <Route path="/run" element={<RunSinglePrompt />} />
        <Route path="/run-multiple" element={<RunMultiplePrompts />} />
        <Route path="/create-prompt" element={<CreatePrompt />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;