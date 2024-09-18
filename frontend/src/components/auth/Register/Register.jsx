import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { registerUser } from '../../../services/userService';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Box from "@mui/material/Box";
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';

export const Register = () => {
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

        if (email.trim() === '' || password.trim() === '' || username.trim() === '') {
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
            const result = await registerUser(userData);
            if (result) {
                setSuccessMessage('Registration successful!');
                setErrorMessage('');
                window.alert('Registration successful!');
                navigate('/login');
            } else {
                setErrorMessage('Registration failed. Email might already be in use.');
            }
        } catch (error) {
            setErrorMessage('Registration failed. Please try again.');
            console.error('Registration error:', error);
        }
    };

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100vh',
                textAlign: 'center',
                padding: 2,
                backgroundColor: "#f8f9fa",
            }}
        >
            <Typography variant="h4" component="h1" sx={{ marginBottom: '30px', fontFamily: "Roboto" }}>
                Register
            </Typography>

            <form onSubmit={handleSubmit} style={{ width: '100%', maxWidth: '500px' }}>
                <TextField
                    label="Username"
                    variant="outlined"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="Email"
                    variant="outlined"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="Password"
                    variant="outlined"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="First Name"
                    variant="outlined"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="Last Name"
                    variant="outlined"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="Date of Birth"
                    type="date"
                    variant="outlined"
                    InputLabelProps={{ shrink: true }}
                    value={dateOfBirth}
                    onChange={(e) => setDateOfBirth(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <TextField
                    label="Address"
                    variant="outlined"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    sx={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                <FormControl fullWidth sx={{ marginBottom: '16px', width: "80%" }}>
                    <InputLabel>Role</InputLabel>
                    <Select
                        value={role}
                        onChange={(e) => setRole(e.target.value)}
                        label="Role"
                    >
                        <MenuItem value="" disabled>Select Role</MenuItem>
                        <MenuItem value="Driver">Driver</MenuItem>
                        <MenuItem value="User">User</MenuItem>
                        <MenuItem value="Admin">Admin</MenuItem>
                    </Select>
                </FormControl>
                <br />
                <input
                    type="file"
                    onChange={handleFileChange}
                    style={{ marginBottom: '16px', width: "80%" }}
                />
                <br />
                {errorMessage && (
                    <Typography variant="body2" color="error" sx={{ marginBottom: '16px' }}>
                        {errorMessage}
                    </Typography>
                )}
                {successMessage && (
                    <Typography variant="body2" color="success" sx={{ marginBottom: '16px' }}>
                        {successMessage}
                    </Typography>
                )}
                <Button
                    type="submit"
                    variant="outlined"
                    sx={{ marginBottom: '16px', backgroundColor: '#f7e32f', color: 'black', width: '80%' }}
                >
                    Register
                </Button>
            </form>

            <Box
                sx={{
                    marginTop: '16px',
                    fontSize: '16px',
                }}
            >
                Already have an account? <a href='/login' style={{ textDecoration: 'none', color: '#f7e32f', textDecoration: 'underline' }}>Login</a>
            </Box>
        </Box>
    );
};
