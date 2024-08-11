import { Login } from './components/auth/Login/Login';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/user/login" element={<Login />} />
      </Routes>
    </Router>
  );
}

export default App;
