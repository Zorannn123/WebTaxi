import React, { useState, useEffect } from "react";
import { getPreviousOrdersUser } from "../../services/userService";
import { useNavigate } from 'react-router-dom';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Box from "@mui/material/Box";
import "@fontsource/roboto";

export const PreviousRides = () => {
    const [orders, setOrders] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchPreviousOrders = async () => {
            try {
                const data = await getPreviousOrdersUser();
                setOrders(data);
            } catch (error) {
                setErrorMessage('Failed to fetch previous rides.');
                console.error('Error fetching previous rides: ', error);
            }
        };
        fetchPreviousOrders();
    }, []);

    const HandleBack = () => {
        navigate("/");
    }

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'flex-start',
                padding: '20px',
                minHeight: '100vh',
                backgroundColor: "#f8f9fa"
            }}
        >
            <Typography variant="h4" component="h1" sx={{ marginBottom: '20px', fontFamily: "Roboto", marginTop: '60px' }}>
                Previous Rides
            </Typography>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            {orders.length > 0 ? (
                <>
                    <TableContainer component={Paper} sx={{ maxHeight: '500px', marginTop: '20px' }}>
                        <Table sx={{ minWidth: 500 }} aria-label="previous rides table">
                            <TableHead>
                                <TableRow>
                                    <TableCell>Start Address</TableCell>
                                    <TableCell>Arrive Address</TableCell>
                                    <TableCell>Distance</TableCell>
                                    <TableCell>Duration</TableCell>
                                    <TableCell>Price</TableCell>
                                    <TableCell>Date</TableCell>
                                    <TableCell>Driver</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {orders.map((order) => (
                                    <TableRow key={order.id}>
                                        <TableCell>{order.startAddress}</TableCell>
                                        <TableCell>{order.arriveAddress}</TableCell>
                                        <TableCell>{order.distance}km</TableCell>
                                        <TableCell>{order.duration}min</TableCell>
                                        <TableCell>{order.price}din</TableCell>
                                        <TableCell>{order.startingTime}</TableCell>
                                        <TableCell>{order.driverId}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    <Box sx={{ marginTop: '20px' }}>
                        <Button
                            variant="outlined"
                            sx={{ marginBottom: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                            onClick={HandleBack}>
                            Back
                        </Button>
                    </Box>
                </>
            ) : (
                <>
                    <Typography variant="h6" sx={{ fontFamily: "Roboto" }}>
                        No previous rides available.
                    </Typography>
                    <Box sx={{ marginTop: '20px' }}>
                        <Button
                            variant="outlined"
                            sx={{ marginBottom: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                            onClick={HandleBack}>
                            Back
                        </Button>
                    </Box>
                </>
            )}
        </Box>
    );
};