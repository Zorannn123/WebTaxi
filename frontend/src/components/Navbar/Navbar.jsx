import React from "react";
import styles from './Navbar.module.css';
import { Link } from 'react-router-dom';

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
                <Link to='/yourProfile'>
                    Your Profile
                </Link>
            </div>
        </nav>
    );
};
