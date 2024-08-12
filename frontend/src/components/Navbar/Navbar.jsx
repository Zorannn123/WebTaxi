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
                <a href="/yourProfile">Your profile</a>
            </div>
            <div>
                <a href="/editProfile">Yowur profile</a>
            </div>
        </nav>
    );
};
