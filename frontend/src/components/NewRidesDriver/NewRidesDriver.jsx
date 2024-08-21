import React, { useState, useEffect } from "react";
import { getOnHoldOrders, acceptOrder } from "../../services/orderService";
import { useNavigate } from 'react-router-dom';

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
            console.log(result)
            if (result) {
                setOrders(orders.filter(order => order.id !== orderId));
                setBusy(true);
                window.alert('You accept the order successfully!');
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
            <>
                <p>You are busy with an order.</p>
                <button onClick={HandleBack}>Back</button>
            </>
        )
    }

    return (
        <div>
            <h1>On-Hold Orders</h1>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            {orders.length > 0 ? (
                <>
                    <table border={1}>
                        <thead>
                            <tr>
                                <th>Start Address</th>
                                <th>Arrive Address</th>
                                <th>Distance</th>
                                <th>Duration</th>
                                <th>Price</th>
                                <th>User</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {orders.map((order) => (
                                <tr key={order.id}>
                                    <td>{order.startAddress}</td>
                                    <td>{order.arriveAddress}</td>
                                    <td>{order.distance}km</td>
                                    <td>{order.duration}min</td>
                                    <td>{order.price}din</td>
                                    <td>{order.userId}</td>
                                    <td>
                                        <button onClick={() => handleAccept(order.id)}>Accept</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <button onClick={HandleBack}>Back</button>
                </>
            ) : (
                <>
                    <p>No on-hold orders available.</p>
                    <button onClick={HandleBack}>Back</button>
                </>
            )}
        </div>
    );
};