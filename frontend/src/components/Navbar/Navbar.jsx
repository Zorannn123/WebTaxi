import React from "react";
import styles from './Navbar.module.css';
import { jwtDecode } from "jwt-decode";
import { getBusyStatus, getIsBlockedStatus } from "../../services/userService";
import { useState, useEffect } from "react";

export const Navbar = () => {
    const [role, setRole] = useState('');
    const [name, setName] = useState('');
    const [isBusy, setIsBusy] = useState(false);
    const [isBlocked, setIsBlocked] = useState(false);

    useEffect(() => {
        const token = localStorage.getItem('authToken');

        if (token) {
            const fetchStatuses = async () => {
                try {
                    // Decode the JWT token
                    const decodedToken = jwtDecode(token);
                    setRole(decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
                    setName(decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
                    console.log('Role:', decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
                    console.log('Name:', decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);

                    // Fetch both statuses concurrently
                    const [busyStatus, blockedStatus] = await Promise.all([
                        getBusyStatus(),
                        getIsBlockedStatus()
                    ]);

                    setIsBusy(busyStatus);
                    setIsBlocked(blockedStatus);
                    console.log('Busy Status:', busyStatus);
                    console.log('Blocked Status:', blockedStatus);
                } catch (error) {
                    console.error('Failed to fetch statuses: ', error);
                }
            };

            fetchStatuses();
        }
    }, []);


    return (
        <nav className={styles.navbar}>
            {!role && (
                <>
                    <div>
                        <a href="/login">Login</a>
                    </div>
                    <div>
                        <a href="/register">Register</a>
                    </div>
                </>
            )}

            {role && (
                <>
                    <div>
                        <a href="/yourProfile">Your Profile</a>
                    </div>

                    {role === "User" && (
                        <>
                            {!isBusy && (<div>
                                <a href="/newRide">New Ride</a>
                            </div>)}
                            <div>
                                <a href="/previousRides">Previous Rides</a>
                            </div>
                        </>
                    )}

                    {role === "Driver" && (
                        <>
                            <div>
                                <a href="/myRides">My Rides</a>
                            </div>
                            {!isBusy && !isBlocked && (<div>
                                <a href="/newRides">New Rides</a>
                            </div>)}
                        </>
                    )}

                    {role === "Admin" && (
                        <>
                            <div>
                                <a href="/allRides">All Rides</a>
                            </div>
                            <div>
                                <a href="/allDrivers">All Drivers</a>
                            </div>
                            <div>
                                <a href="/driverRating">Driver Rating</a>
                            </div>
                        </>
                    )}
                    <div>
                        <a href="/logout">Logout</a>
                    </div>
                </>
            )}
        </nav>
    );
};
