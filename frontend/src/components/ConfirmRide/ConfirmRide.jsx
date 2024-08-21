import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEstimateOrder, confirmOrder, deleteOrder } from "../../services/orderService";
import Countdown from 'react-countdown';
import { rateRide } from "../../services/userService";

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
                window.location.reload()
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
            {order.status === "OnHold" && <div>
                <button onClick={handleAccept}>Accept</button>
                <button onClick={handleDecline}>Decline</button>
            </div>}

            {order.status === "ConfirmedByUser" && <div>
                Waiting for driver to accept
            </div>}

            {order.status === "WaitingForPickup" && <div>
                Waiting for pickup
                <Countdown
                    date={date.getTime() + (order.scheduledPickup * 1000)}
                    renderer={renderer}
                />
            </div>}
            {order.status === "InProgress" && <div>
                Waiting for ride to finish
                <Countdown
                    date={date.getTime() + ((order.scheduledPickup + order.duration) * 1000)}
                    renderer={renderer}
                />
            </div>}

            {order.status === "Finished" && (<div>
                Order is finished

                <br />
                <br />
                {!showInput && (
                    <button onClick={HandleRate}>Rate ride</button>
                )}
                {showInput && (
                    <div>
                        <label>Rate a ride(1-5)</label>
                        <br />
                        <input
                            type="number"
                            value={rating}
                            onChange={handleRatingChange}
                            placeholder="Enter your rating"
                            min="1"
                            max="5"
                        />
                        <button onClick={handleSubmitRating}>
                            Submit Rating
                        </button>
                    </div>
                )}
            </div>)}

        </div>
    );
};