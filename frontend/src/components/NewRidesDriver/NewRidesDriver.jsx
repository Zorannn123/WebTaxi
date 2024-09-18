import React, { useState, useEffect } from "react";
import { getOnHoldOrders, acceptOrder } from "../../services/orderService";
import { useNavigate } from 'react-router-dom';
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

export const NewRides = () => {
    const [orders, setOrders] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        const fetchOnHoldOrders = async () => {
            try {
                const data = await getOnHoldOrders();
                setOrders(data);
                console.log(data);
            } catch (error) {
                setErrorMessage('Failed to fetch on-hold orders.');
                console.error('Error fetching on-hold orders: ', error);
            }
        };
        fetchOnHoldOrders();
    }, []);

    const handleAccept = async (orderId) => {
        try {
            const result = await acceptOrder(orderId);
            localStorage.setItem('orderId', orderId);
            console.log(result)
            if (result) {
                setOrders(orders.filter(order => order.id !== orderId));
                setBusy(true);
                navigate('/chat');
            }
        } catch (error) {
            setErrorMessage('Failed to accept order.');
            console.error('Error accepting order: ', error);
        }
    };

    const HandleBack = () => {
        navigate("/");
    }

    if (busy) {
        return (
            <Box
                sx={{
                    display: 'flex',
                    minHeight: '100vh',
                    flexDirection: 'column',
                    alignItems: 'flex-start',
                    padding: '20px',
                    backgroundColor: "#f8f9fa"
                }}>
                <Typography variant="h4" gutterBottom
                    sx={{ marginBottom: '20px', fontFamily: "Roboto", marginTop: '30px' }}>
                    You are busy with an order.
                </Typography>
                <Button
                    variant="outlined"
                    sx={{ marginTop: '20px', backgroundColor: 'black', color: '#f7e32f' }}
                    onClick={HandleBack}
                >
                    Back
                </Button>
            </Box>
        )
    }

    return (
        <Box
            sx={{
                display: 'flex',
                minHeight: '100vh',
                flexDirection: 'column',
                alignItems: 'flex-start',
                padding: '20px',
                backgroundColor: "#f8f9fa"
            }}
        >
            <Typography variant="h4" gutterBottom
                sx={{ marginBottom: '20px', fontFamily: "Roboto", marginTop: '30px' }}>
                On-Hold Orders
            </Typography>

            {errorMessage && (
                <Typography variant="body1" color="error">
                    {errorMessage}
                </Typography>
            )}

            {orders.length > 0 ? (
                <>
                    <TableContainer component={Paper} sx={{ maxHeight: '500px', marginTop: '20px' }}>
                        <Table stickyHeader aria-label="on-hold orders table">
                            <TableHead>
                                <TableRow>
                                    <TableCell>Start Address</TableCell>
                                    <TableCell>Arrive Address</TableCell>
                                    <TableCell>Distance</TableCell>
                                    <TableCell>Duration</TableCell>
                                    <TableCell>Away from</TableCell>
                                    <TableCell>Price</TableCell>
                                    <TableCell>User</TableCell>
                                    <TableCell>Actions</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {orders.map((order) => (
                                    <TableRow key={order.id}>
                                        <TableCell>{order.startAddress}</TableCell>
                                        <TableCell>{order.arriveAddress}</TableCell>
                                        <TableCell>{order.distance}km</TableCell>
                                        <TableCell>{order.duration}min</TableCell>
                                        <TableCell>{order.scheduledPickup}min</TableCell>
                                        <TableCell>{order.price}din</TableCell>
                                        <TableCell>{order.userId}</TableCell>
                                        <TableCell>
                                            <Button
                                                variant="outlined"
                                                sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black' }}
                                                onClick={() => handleAccept(order.id)}
                                            >
                                                Accept
                                            </Button>
                                        </TableCell>
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
                    <Typography variant="h6" gutterBottom
                        sx={{ marginBottom: '20px', fontFamily: "Roboto", marginTop: '30px' }}>
                        No on-hold orders available.
                    </Typography>
                    <Button
                        variant="outlined"
                        sx={{ marginTop: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={HandleBack}
                    >
                        Back
                    </Button>
                </>
            )}
        </Box>
    );
};