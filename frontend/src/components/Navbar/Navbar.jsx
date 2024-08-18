import React from "react";
import styles from './Navbar.module.css';

export const Navbar = () => {
    return (
        <nav className={styles.navbar}>
            <div>
                <a href="/login">Login</a>
            </div>
            <div>
                <a href="/register">Register</a>
            </div>
            <div>
                <a href="/yourProfile">Your Profile</a>
            </div>
            <div>
                <a href="/newRide">New Ride</a>
            </div>
            <div>
                <a href="/previousRides">Previous Rides</a>
            </div>
            <div>
                <a href="/newRides">New Rides</a>
            </div>
        </nav>
    );
};
