import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEstimateOrder, confirmOrder, deleteOrder } from "../../services/orderService";
import Countdown from 'react-countdown';
import { rateRide } from "../../services/userService";
import Box from "@mui/material/Box";
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import "@fontsource/roboto";

export const ConfirmOrder = () => {
    const { id } = useParams();
    const [order, setOrder] = useState(null);
    const [showInput, setShowInput] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');
    const [rating, setRating] = useState("");
    const navigate = useNavigate();

    useEffect(() => {
        const fetchCurrentOrder = async () => {
            try {
                const data = await getEstimateOrder(id);
                setOrder(data);
            } catch (error) {
                setErrorMessage('Failed to fetch order!:(');
                console.error('Error fetching order: ', error);
            }
        };
        fetchCurrentOrder();
    }, [id]);

    const handleAccept = async () => {
        try {
            const success = await confirmOrder(id);
            if (success) {
                window.alert('Order accepted successfully!');
                window.location.reload()
            } else {
                setErrorMessage('Failed to accept the order.');
            }
        } catch (error) {
            setErrorMessage('Error occurred while accepting the order.');
            console.error('Error accepting order: ', error);
        }
    };



    const handleDecline = async () => {
        try {
            const success = await deleteOrder(id);
            if (success) {
                window.alert('Order declined successfully!');
                localStorage.removeItem("orderId");
                navigate('/newRide');
            } else {
                setErrorMessage('Failed to decline the order.');
            }
        } catch (error) {
            setErrorMessage('Error occurred while declining the order.');
            console.error('Error declining order: ', error);
        }
    };

    const renderer = ({ hours, minutes, seconds, completed }) => {
        if (completed) {
            const delayDuration = 1000;

            setTimeout(() => {
                window.location.reload();
            }, delayDuration);
        } else {
            // Render a countdown
            return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
        }
    };

    const HandleRate = () => {
        setShowInput(true);
    };

    const handleRatingChange = (e) => {
        setRating(e.target.value);
    };

    const handleSubmitRating = async () => {
        try {
            const result = await rateRide(order.id, parseInt(rating), order.driverId)
            console.log(result);
            setShowInput(false);
            setRating("");
            localStorage.removeItem("orderId");
            window.alert("Your rating successfully recorded!");
            navigate("/");
        } catch (error) {
            console.error("Error submiting rating: ", error);
        }
    };

    //const clearLocalOrder = () => {
    //    return localStorage.removeItem("orderId");
    //}

    if (!order) {
        return <div>Loading...</div>;
    }

    let date = new Date(order.startingTime);
    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '90vh',
                textAlign: 'center',
                padding: 2,
                backgroundColor: "#f8f9fa"
            }}
        >
            <Typography variant="h4" gutterBottom
                sx={{ marginBottom: '30px', fontFamily: "Roboto" }}
            >
                Your Order
            </Typography>

            {errorMessage && (
                <Typography variant="body1" color="error">
                    {errorMessage}
                </Typography>
            )}

            <Paper sx={{ padding: '20px', marginBottom: '20px', width: '20%' }}>
                <Typography variant="body1" sx={{ marginBottom: "10px" }} ><strong>Start Address:</strong> {order.startAddress}</Typography>
                <Typography variant="body1" sx={{ marginBottom: "10px" }}><strong>Arrive Address:</strong> {order.arriveAddress}</Typography>
                <Typography variant="body1" sx={{ marginBottom: "10px" }}><strong>Distance:</strong> {order.distance}km</Typography>
                <Typography variant="body1" sx={{ marginBottom: "10px" }}><strong>Price:</strong> {order.price}din</Typography>
                <Typography variant="body1" sx={{ marginBottom: "10px" }}><strong>Ride duration:</strong> {order.duration}min</Typography>
                <Typography variant="body1" sx={{ marginBottom: "10px" }}><strong>Scheduled pickup:</strong> {order.scheduledPickup}min</Typography>
            </Paper>

            {order.status === "OnHold" && (
                <Box>
                    <Button
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black', marginRight: '20px' }}
                        onClick={handleAccept}>
                        Accept
                    </Button>
                    <Button
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={handleDecline}>
                        Decline
                    </Button>
                </Box>
            )}

            {order.status === "ConfirmedByUser" && (
                <Typography variant="h6" sx={{ fontFamily: "Roboto" }}>
                    Waiting for driver to accept
                </Typography>
            )}

            {order.status === "WaitingForPickup" && (
                <Box sx={{ marginTop: '20px' }}>
                    <Typography variant="h6" sx={{ fontFamily: "Roboto" }}>
                        Waiting for pickup
                    </Typography>
                    <Countdown
                        date={date.getTime() + (order.scheduledPickup * 1000)}
                        renderer={renderer}
                    />
                </Box>
            )}

            {order.status === "InProgress" && (
                <Box sx={{ marginTop: '20px' }}>
                    <Typography variant="h6" sx={{ fontFamily: "Roboto" }}>
                        Waiting for ride to finish
                    </Typography>
                    <Countdown
                        date={date.getTime() + ((order.scheduledPickup + order.duration) * 1000)}
                        renderer={renderer}
                    />
                </Box>
            )}

            {order.status === "Finished" && (
                <Box>
                    <Typography variant="h6" sx={{ marginTop: '5px', fontFamily: "Roboto" }}>
                        Ride is finished
                    </Typography>
                    <Box sx={{ marginTop: '20px' }}>
                        {!showInput && (
                            <Button
                                variant="outlined"
                                sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black' }}
                                onClick={HandleRate}>
                                Rate the ride
                            </Button>
                        )}
                        {showInput && (
                            <Box sx={{ marginTop: '20px' }}>
                                <Typography variant="body1" gutterBottom>
                                    Rate the ride (1-5)
                                </Typography>
                                <TextField
                                    type="number"
                                    value={rating}
                                    onChange={handleRatingChange}
                                    placeholder="Enter your rating"
                                    inputProps={{ min: 1, max: 5 }}
                                    fullWidth
                                    variant="outlined"
                                    sx={{ marginBottom: '20px' }}
                                />
                                <Button
                                    variant="outlined"
                                    sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black' }}
                                    onClick={handleSubmitRating}>
                                    Submit Rating
                                </Button>
                            </Box>
                        )}
                    </Box>
                </Box>
            )}
        </Box>
    );
};