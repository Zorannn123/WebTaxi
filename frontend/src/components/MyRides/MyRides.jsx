import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getPreviousOrdersDriver } from "../../services/orderService";
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import "@fontsource/roboto";

export const MyRides = () => {
    const [orders, setOrders] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchMyPreviousOrders = async () => {
            try {
                const data = await getPreviousOrdersDriver();
                setOrders(data);
            } catch (error) {
                setErrorMessage('Failed to fetch previous rides.');
                console.error('Error fetching previous rides: ', error);
            }
        };
        fetchMyPreviousOrders();
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
            <Typography variant="h4" component="h1" sx={{ marginBottom: '20px', fontFamily: "Roboto", marginTop: '30px' }}>
                My Rides
            </Typography>

            {errorMessage && (
                <Typography variant="body1" color="error">
                    {errorMessage}
                </Typography>
            )}

            {orders.length > 0 ? (
                <>
                    <TableContainer component={Paper} sx={{ maxHeight: '500px', marginTop: '20px' }}>
                        <Table stickyHeader aria-label="rides table">
                            <TableHead>
                                <TableRow>
                                    <TableCell>Start Address</TableCell>
                                    <TableCell>Arrive Address</TableCell>
                                    <TableCell>Distance</TableCell>
                                    <TableCell>Duration</TableCell>
                                    <TableCell>Price</TableCell>
                                    <TableCell>Date</TableCell>
                                    <TableCell>User</TableCell>
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
                                        <TableCell>{order.userId}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    <Button
                        variant="outlined"
                        sx={{ marginTop: '20px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={HandleBack}
                    >
                        Back
                    </Button>
                </>
            ) : (
                <>
                    <Typography variant="body1" sx={{ marginTop: '20px' }}>
                        No previous rides available.
                    </Typography>
                    <Button
                        variant="outlined"
                        sx={{ marginTop: '20px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={HandleBack}
                    >
                        Back
                    </Button>
                </>
            )}
        </Box>
    );
};