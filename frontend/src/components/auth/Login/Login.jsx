import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../../../services/userService';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Box from "@mui/material/Box";



export const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (email.trim() === '' || password.trim() === '') {
            setErrorMessage('Please enter both username and password.');
            return;
        }

        try {
            localStorage.clear();
            const data = await login(email, password);

            if (data) {
                localStorage.setItem('authToken', data);
                navigate('/');
            } else {
                console.error("Token not found in the response");
                setErrorMessage("Token not found in the response");
            }
        } catch (error) {
            setErrorMessage('Login failed. Please check your credentials.');
            console.error('Login error:', error);
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
                marginTop: '-100px',
                padding: 2,
                backgroundColor: "#f8f9fa",
            }}
        >
            <Typography variant="h4" component="h1" sx={{ marginBottom: '30px', fontFamily: "Roboto" }}>
                Login
            </Typography>

            <form onSubmit={handleSubmit} style={{ width: '100%', maxWidth: '400px' }}>
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
                {errorMessage && (
                    <Typography variant="body2" color="error" sx={{ marginBottom: '16px' }}>
                        {errorMessage}
                    </Typography>
                )}
                <Button
                    type="submit"
                    variant="outlined"
                    sx={{ marginBottom: '16px', backgroundColor: '#f7e32f', color: 'black', width: '80%' }}
                >
                    Login
                </Button>
            </form>

            <Box
                sx={{
                    marginTop: '16px',
                    fontSize: '16px',
                }}
            >
                Don't have an account? <a href='/register' style={{ textDecoration: 'none', color: '#f7e32f', textDecoration: 'underline' }}>Sign up</a>
            </Box>
        </Box>
    );
};
