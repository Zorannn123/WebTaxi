import React, { useState, useEffect } from "react";
import { blockDriver, getDriverRating, getDrivers, unBlockDriver } from "../../services/userService";
import { useNavigate } from "react-router-dom";

export const DriverRating = () => {
    const [drivers, setDrivers] = useState([]);
    const [ratings, setRatings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchDrivers = async () => {
            setLoading(true);
            try {
                const driverData = await getDrivers();
                setDrivers(driverData);
                console.log(driverData)

                // Create an array of promises to fetch ratings for each driver
                const ratingPromises = driverData.map(async (driver) => {
                    try {
                        const rating = await getDriverRating(driver.email);
                        return {
                            email: driver.email,
                            rating,
                            isBlocked: driver.isBlocked
                        };

                    } catch (error) {
                        console.error(`Error fetching rating for ${driver.email}:`, error);
                        return null;
                    }
                });

                // Wait for all rating promises to resolve
                const ratingData = await Promise.all(ratingPromises);

                // Filter out drivers with a rating of 0 and null values
                const filteredRatings = ratingData.filter(driver => driver && driver.rating > 0);
                setRatings(filteredRatings);
            } catch (error) {
                setErrorMessage('Failed to load drivers.');
            } finally {
                setLoading(false);
            }
        };

        fetchDrivers();
    }, [])


    const handleBack = () => {
        navigate("/");
    };

    const handleBlockDriver = async (email) => {
        try {
            await blockDriver(email);
            window.location.reload();
        } catch (error) {
            console.error(`Failed to block driver ${email}: `, error);
        }
    };

    const handleUnBlockDriver = async (email) => {
        try {
            await unBlockDriver(email);
            window.location.reload();
        } catch (error) {
            console.error(`Failed to unblock driver ${email}: `, error);
        }
    };

    return (
        <div>
            <h1>Driver Ratings</h1>
            <table border="1" cellPadding="10">
                <thead>
                    <tr>
                        <th>Driver Email</th>
                        <th>Average Rating</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {ratings.length > 0 ? (
                        ratings.map((driver, index) => (
                            <tr key={index}>
                                <td>{driver.email}</td>
                                <td>{driver.rating}</td>
                                <td>
                                    {driver.isBlocked ? (
                                        <button onClick={() => handleUnBlockDriver(driver.email, true)}>
                                            Unblock
                                        </button>
                                    ) : (
                                        <button onClick={() => handleBlockDriver(driver.email, false)}>
                                            Block
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))
                    ) : (
                        <tr>
                            <td colSpan="2">No ratings available</td>
                        </tr>
                    )}
                </tbody>
            </table>
            <br />
            <button onClick={handleBack}>Back</button>
        </div>
    );
};
