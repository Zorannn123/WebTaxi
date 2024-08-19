import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getAllOrders } from "../../services/orderService";

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
        <div>
            <h1>All Rides</h1>
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
                                <th>Driver</th>
                                <th>User</th>
                                <th>Order State</th>
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
                                    <td>{order.driverId}</td>
                                    <td>{order.userId}</td>
                                    <td>{order.status}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <button onClick={HandleBack}>Back</button>
                </>
            ) : (
                <p>No previous rides available.</p>
            )}
        </div>
    );
};