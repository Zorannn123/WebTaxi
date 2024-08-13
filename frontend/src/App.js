import { Login } from './components/auth/Login/Login';
import { Register } from './components/auth/Register/Register';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { EditProfile } from './components/EditProfile/EditProfile';
import { Dashboard } from './components/Dashboard/Dashboard';
import { UserProfile } from './components/UserProfile/UserProfile';
import React from 'react';
import './App.css';
import { ProtectedRoute } from './components/utils/ProtectedRoute';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/editProfile" element={<EditProfile />} />
        <Route element={<ProtectedRoute />} >
          <Route path="/yourProfile" element={<UserProfile />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
