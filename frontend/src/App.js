import { Login } from './components/auth/Login/Login';
import { Register } from './components/auth/Register/Register';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';
import { EditProfile } from './components/EditProfile/EditProfile';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/user/login" element={<Login />} />
        <Route path="/user/register" element={<Register />} />
        <Route path="/user/editProfile" element={<EditProfile />} />
      </Routes>
    </Router>
  );
}

export default App;
