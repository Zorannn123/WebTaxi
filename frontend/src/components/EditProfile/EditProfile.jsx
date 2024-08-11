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
    useEffect(() => {
        const fetchUserProfile = async () => {
            try {
                const userProfile = await getUserProfile();
                setUsername(userProfile.username);
                setEmail(userProfile.email);
                setFirstName(userProfile.firstName);
                setLastName(userProfile.lastName);
                setDateOfBirth(userProfile.dateOfBirth);
                setAddress(userProfile.address);
                setRole(userProfile.role);
                setImage(userProfile.image);
            } catch (error) {
                setErrorMessage('Failed to load user profile.');
            }
        };

        fetchUserProfile();
    }, []);


    const handleFileChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64Image = reader.result.split(',')[1];
                setImage(base64Image);
            };
            reader.readAsDataURL(file);
        }
    };


    const handleSubmit = async (e) => {
        e.preventDefault();

        if (email.trim() === '' || username.trim() === '') {
            setErrorMessage('Please fill in all required fields.');
            return;
        }


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
            if (result) {
                setSuccessMessage('Profile updated successfully!');
                setErrorMessage('');
                navigate('/profile');
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
                <label>Email:</label><br />
                <input
                    type="text"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled
                /><br />
                <label>Password:</label><br />
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Enter new password if you want to change it"
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
                <label>Role:</label><br />
                <select
                    value={role}
                    onChange={(e) => setRole(e.target.value)}
                    disabled
                >
                    <option value="Driver">Driver</option>
                    <option value="User">User</option>
                </select><br />
                <label>Image:</label><br />
                <input
                    type="file"
                    onChange={handleFileChange}
                /><br />
                {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
                {successMessage && <p style={{ color: 'green' }}>{successMessage}</p>}
                <button type="submit">Update Profile</button>
            </form>
        </div>
    );
};
