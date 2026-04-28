import { BrowserRouter, Routes, Route } from "react-router-dom";
import Navbar from "./Navbar.tsx";
import Analytics from "./Analytics.tsx";
import RunSinglePrompt from "./RunSinglePrompt.tsx";
import RunMultiplePrompts from "./RunMultiplePrompts.tsx";
import CreatePrompt from "./CreatePrompt.tsx";
import LoginPage from "./LoginPage.tsx";
import ModelResponsesPage from "./ModelresponsesPage.tsx";

function App() {
  //const token = localStorage.getItem("token");

  // if (!token) {
  //   return <LoginPage />;
  // }

  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<Analytics />} />
        <Route path="/run" element={<RunSinglePrompt />} />
        <Route path="/run-multiple" element={<RunMultiplePrompts />} />
        <Route path="/create-prompt" element={<CreatePrompt />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/model-responses" element={<ModelResponsesPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;