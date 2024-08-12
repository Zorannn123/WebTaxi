import React, { useState, useEffect } from "react";
import axios from "axios";
import { getUserProfile } from "../../services/userService";

export const UserProfile = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [address, setAddress] = useState('');
    const [image, setImage] = useState('');

    const fetchUserProfile = async () => {
        try {
            const token = localStorage.getItem('userToken');
            const response = getUserProfile();
            const userProfile = response.data;

            setUsername(userProfile.username);
            setPassword(userProfile.password);
            setFirstName(userProfile.firstName);
            setLastName(userProfile.lastName);
            setAddress(userProfile.address);
            setImage(userProfile.image);
        } catch (error) {
            console.error('Error fetching user profile:', error);
        }
    };

    useEffect(() => {
        fetchUserProfile();
    }, []);

    return (
        <>
            <div>
                <h2>WELCOME TO YOUR PROFILE</h2>
            </div>
            <div>
                <a href="/yourProfile">Edit profile</a>
            </div>
        </>
    );
};
