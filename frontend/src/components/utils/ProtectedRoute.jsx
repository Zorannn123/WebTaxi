import React from "react";
import { Navigate, Outlet } from 'react-router-dom';

export const ProtectedRoute = ({ isLoggedIn }) => {
    return (
        <div>
            {isLoggedIn ? <Outlet /> : <Navigate to='/' />}
        </div>
    )
};