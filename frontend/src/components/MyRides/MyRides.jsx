import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getPreviousOrdersDriver } from "../../services/orderService";

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
        <div>
            <h1>My Rides</h1>
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
                                <th>Date</th>
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
                                    <td>{order.startingTime}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <button onClick={HandleBack}>Back</button>
                </>
            ) : (
                <>
                    <p>No previous rides available.</p>
                    <button onClick={HandleBack}>Back</button>
                </>
            )}
        </div>
    );
};