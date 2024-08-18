import React, { useState, useEffect } from "react";
import { getUserProfile } from "../../services/userService";
import { useNavigate } from "react-router-dom";

export const UserProfile = () => {
    const [username, setUsername] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [address, setAddress] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [image, setImage] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const fetchUserProfile = async () => {
        try {
            const token = localStorage.getItem('testToken');
            const response = await getUserProfile(token);
            const userProfile = response;
            console.log(response);
            if (userProfile) {
                setUsername(userProfile.userName);
                setFirstName(userProfile.firstName);
                setLastName(userProfile.lastName);
                setAddress(userProfile.address);
                setImage(userProfile.image);
                setDateOfBirth(userProfile.dateOfBirth);
            }
            else {
                setErrorMessage('Error fetching user profile:');
            }
        } catch (error) {
            console.error('Error fetching user profile:', error);
        }
    };

    useEffect(() => {
        fetchUserProfile();
    }, []);

    const handleEditClick = () => {
        navigate("/editProfile");
    }
    const HandleBack = () => {
        navigate("/");
    }

    return (
        <div>
            <div>
                <div>
                    <h1>YOUR PROFILE</h1>
                    <label>Username: {username}</label><br />
                    <label>First name: {firstName}</label><br />
                    <label>Last name: {lastName}</label><br />
                    <label>Address: {address}</label><br />
                    <label>DateOfBirth: {dateOfBirth}</label><br />
                    <div>
                        <img src={image} alt="PPic" />
                    </div>
                    <br />

                    <button onClick={(e) => {
                        e.preventDefault();
                        handleEditClick();
                    }}>Edit Profile</button>
                    {errorMessage && <p style={{ color: 'red', textAlign: 'center', marginBottom: 20 }} > {errorMessage}</p>}
                    <button onClick={HandleBack}>Back</button>
                </div>
            </div>
        </div>
    );
};
