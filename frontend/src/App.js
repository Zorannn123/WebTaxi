import { Login } from './components/auth/Login/Login';
import { Register } from './components/auth/Register/Register';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { EditProfile } from './components/EditProfile/EditProfile';
import { Dashboard } from './components/Dashboard/Dashboard';
import { UserProfile } from './components/UserProfile/UserProfile';
import { ProtectedRoute } from './components/utils/ProtectedRoute';
import { NewRide } from './components/NewRide/NewRide';
import { ConfirmOrder } from './components/ConfirmRide/ConfirmRide';
import { NewRides } from './components/NewRidesDriver/NewRidesDriver';
import React from 'react';
import './App.css';
import { PreviousRides } from './components/PreviousRides/PreviousRides';
import { MyRides } from './components/MyRides/MyRides';
import { AllRides } from './components/AllRides/AllRides';
import { Logout } from './components/auth/Logout/Logout';
import { Drivers } from './components/AllDrivers/AllDrivers';
import { DriverRating } from './components/DriverRating/DriverRating';
import { MessageChat } from './components/Chat/Chat';

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
          <Route path="/newRide" element={<NewRide />} />
          <Route path="/confirmOrder/:id" element={<ConfirmOrder />} />
          <Route path="/newRides" element={<NewRides />} />
          <Route path="/previousRides" element={<PreviousRides />} />
          <Route path="/myRides" element={<MyRides />} />
          <Route path="/allRides" element={<AllRides />} />
          <Route path="/logout" element={<Logout />} />
          <Route path="/allDrivers" element={<Drivers />} />
          <Route path="/driverRating" element={<DriverRating />} />
          <Route path="/chat" element={<MessageChat />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
