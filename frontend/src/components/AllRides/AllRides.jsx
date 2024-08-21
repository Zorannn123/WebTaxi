import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getAllOrders } from "../../services/orderService";
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import Box from "@mui/material/Box";

export const AllRides = () => {
    const [orders, setOrders] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchAllOrders = async () => {
            try {
                const data = await getAllOrders();
                console.log(data);
                setOrders(data);
            } catch (error) {
                setErrorMessage('Failed to fetch previous rides.');
                console.error('Error fetching previous rides: ', error);
            }
        };
        fetchAllOrders();
    }, []);

    const HandleBack = () => {
        navigate("/");
    }

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'left',
                padding: '20px',
            }}
        >
            <h1>All Rides</h1>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            {orders.length > 0 ? (
                <>
                    <TableContainer component={Paper} sx={{ margin: 'auto', maxHeight: '700px' }}>
                        <Table sx={{ minWidth: 500 }} aria-label="rides table">
                            <TableHead>
                                <TableRow>
                                    <TableCell>Start Address</TableCell>
                                    <TableCell>Arrive Address</TableCell>
                                    <TableCell>Distance</TableCell>
                                    <TableCell>Duration</TableCell>
                                    <TableCell>Price</TableCell>
                                    <TableCell>Date</TableCell>
                                    <TableCell>Driver</TableCell>
                                    <TableCell>User</TableCell>
                                    <TableCell>Order State</TableCell>
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
                                        <TableCell>{order.userId}</TableCell>
                                        <TableCell>{order.status}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    <br />
                    <Button variant="contained" sx={{ width: "30px" }} onClick={HandleBack}>Back</Button>
                </>
            ) : (
                <p>No previous rides available.</p>
            )}
        </Box>
    );
};
