import { Login } from './components/auth/Login/Login';
import { Register } from './components/auth/Register/Register';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { EditProfile } from './components/EditProfile/EditProfile';
import { Dashboard } from './components/Dashboard/Dashboard';
import { UserProfile } from './components/UserProfile/UserProfile';
import React, { useState } from 'react';
import './App.css';

function App() {
  const [isLogged, setIsLogged] = useState(false);
  console.log(isLogged);
  const logState = () => {
    if (!isLogged) {
      setIsLogged(true);
    }
  };

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/login" element={<Login logState={logState} />} />
        <Route path="/register" element={<Register />} />
        <Route path="/editProfile" element={<EditProfile />} />
        {isLogged && <Route path="/yourProfile" element={<UserProfile />} />}
      </Routes>
    </Router>
  );
}

export default App;
