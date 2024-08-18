import React from "react";
import { Navigate, Outlet } from 'react-router-dom';

export const ProtectedRoute = () => {

    const token = localStorage.getItem('authToken');
    if (token) {
        return <Outlet />
    } else {
        return <Navigate to='/' />
    }
};