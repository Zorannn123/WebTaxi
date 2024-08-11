import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../../../services/userService'; // Adjust the path to where your service is located

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
            const data = await login(email, password);
            console.log(email);
            console.log(password);
            window.alert('Login successful!');
            // You can store the token or any other response data here if needed
            // localStorage.setItem('token', data.token);
            navigate('/');
        } catch (error) {
            setErrorMessage('Login failed. Please check your credentials.');
            console.error('Login error:', error);
        }
    };

    return (
        <div>
            <h1>Login</h1>
            <form onSubmit={handleSubmit}>
                <label>Email:</label><br />
                <input
                    type="text"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                /><br />
                <label>Password:</label><br />
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                /><br />
                {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
                <button type="submit">Login</button>
            </form>
            <div>
                Don't have an account?
                <a href='/register'>Sign up</a>
            </div>
        </div>
    );
};
