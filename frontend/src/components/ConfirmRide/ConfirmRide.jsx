import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEstimateOrder, confirmOrder, deleteOrder } from "../../services/orderService";


export const ConfirmOrder = () => {
    const { id } = useParams();
    const [order, setOrder] = useState(null);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchCurrentOrder = async () => {
            try {
                const data = await getEstimateOrder(id);
                setOrder(data);
                console.log(data);
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
                navigate('/');
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
                navigate('/');
            } else {
                setErrorMessage('Failed to decline the order.');
            }
        } catch (error) {
            setErrorMessage('Error occurred while declining the order.');
            console.error('Error declining order: ', error);
        }
    };


    if (!order) {
        return <div>Loading...</div>;
    }

    return (
        <div>
            <h1>Confirm Order</h1>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            <div>
                <p><strong>Start Address:</strong> {order.startAddress}</p>
                <p><strong>Arrive Address:</strong> {order.arriveAddress}</p>
                <p><strong>Distance:</strong> {order.distance}km</p>
                <p><strong>Price:</strong> {order.price}din</p>
                <p><strong>Ride duration:</strong> {order.duration}min</p>
                <p><strong>Scheduled pickup:</strong> {order.scheduledPickup}min</p>
            </div>
            <button onClick={handleAccept}>Accept</button>
            <button onClick={handleDecline}>Decline</button>
        </div>
    );
};