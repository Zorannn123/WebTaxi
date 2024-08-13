import React, { useState, useEffect } from "react";
import { Navigate, Outlet } from 'react-router-dom';

export const ProtectedRoute = () => {

    const token = localStorage.getItem('authToken');
    console.log(token);
    if (token) {
        return <Outlet />
    } else {
        return <Navigate to='/' />
    }
};