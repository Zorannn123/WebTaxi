import React from "react";
import styles from './Navbar.module.css';
import { jwtDecode } from "jwt-decode";

export const Navbar = () => {
    const token = localStorage.getItem('authToken');
    let role = null;
    if (token) {
        const decodedToken = jwtDecode(token);
        role = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
        console.log(role);
    }

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
                            <div>
                                <a href="/newRide">New Ride</a>
                            </div>
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
                            <div>
                                <a href="/newRides">New Rides</a>
                            </div>
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
