import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserProfile, updateUserProfile } from '../../services/userService';

export const EditProfile = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [address, setAddress] = useState('');
    const [role, setRole] = useState('');
    const [image, setImage] = useState(null);
    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const navigate = useNavigate();

    //TODO:
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
                setEmail(userProfile.email);
                setPassword(userProfile.password);
                setRole(userProfile.role);
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


    const handleFileChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64Image = reader.result.split(',')[1];
                setImage(base64Image);
                console.log(image);
            };
            reader.readAsDataURL(file);
        }
    };


    const handleSubmit = async (e) => {
        e.preventDefault();

        const userData = {
            username,
            email,
            password,
            firstName,
            lastName,
            dateOfBirth,
            address,
            role,
            image
        };

        try {
            const result = await updateUserProfile(userData);
            console.log(result);
            if (result) {
                setSuccessMessage('Profile updated successfully!');
                setErrorMessage('');
                navigate('/yourProfile');
            } else {
                setErrorMessage('Profile update failed.');
            }
        } catch (error) {
            setErrorMessage('Profile update failed. Please try again.');
            console.error('Profile update error:', error);
        }
    };

    return (
        <div>
            <h1>Edit Profile</h1>
            <form onSubmit={handleSubmit}>
                <label>Username:</label><br />
                <input
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                /><br />
                <label>First Name:</label><br />
                <input
                    type="text"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                /><br />
                <label>Last Name:</label><br />
                <input
                    type="text"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                /><br />
                <label>Date of Birth:</label><br />
                <input
                    type="date"
                    value={dateOfBirth}
                    onChange={(e) => setDateOfBirth(e.target.value)}
                /><br />
                <label>Address:</label><br />
                <input
                    type="text"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                /><br />
                <label>Image:</label><br />
                <input
                    type="file"
                    onChange={handleFileChange}
                />
                {image && <img src={image} alt='uploaded' />}
                <br />
                {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
                {successMessage && <p style={{ color: 'green' }}>{successMessage}</p>}
                <button type="submit">Update Profile</button>
            </form>
        </div>
    );
};
