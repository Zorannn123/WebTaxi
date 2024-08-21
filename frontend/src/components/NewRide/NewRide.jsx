import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { createNewRide } from "../../services/userService";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import "@fontsource/roboto";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";


export const NewRide = () => {
    const [startAddress, setStartAddress] = useState('');
    const [arriveAddress, setArriveAddress] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const savedOrderId = localStorage.getItem('orderId');
        if (savedOrderId) {
            navigate(`/confirmOrder/${savedOrderId}`);
        }
    }, [navigate]);

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (startAddress.trim() === '' || arriveAddress.trim() === '') {
            setErrorMessage('Please enter both start and arrive addresses.');
            return;
        }

        try {
            const data = await createNewRide(startAddress, arriveAddress);
            console.log(data.id);
            if (data && data.id) {
                localStorage.setItem('orderId', data.id);
                navigate(`/confirmOrder/${data.id}`);
            }
        } catch (error) {
            setErrorMessage('Ride creation failed. Please try again.');
            console.error('Ride creation error:', error);
        }
    };

    const HandleBack = () => {
        navigate('/');
    }

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
                backgroundColor: "#f8f9fa"
            }}
        >
            <form onSubmit={handleSubmit}>
                <Typography variant="h4" component="h1" sx={{ marginBottom: '30px', fontFamily: "Roboto" }}>
                    Create New Ride
                </Typography>
                <TextField
                    label="Start Address:"
                    variant="outlined"
                    value={startAddress}
                    onChange={(e) => setStartAddress(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <TextField
                    label="Arrive Address:"
                    variant="outlined"
                    value={arriveAddress}
                    onChange={(e) => setArriveAddress(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                {errorMessage && (
                    <Typography variant="body2" color="error" sx={{ marginBottom: '16px' }}>
                        {errorMessage}
                    </Typography>
                )}
                <Box>
                    <Button
                        type="submit"
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black', marginRight: '20px' }}
                    >
                        Order a ride
                    </Button>
                    <Button
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={HandleBack}
                    >
                        Back
                    </Button>
                </Box>
            </form>
        </Box>
    );
};