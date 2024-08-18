import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createNewRide } from "../../../services/userService";

export const NewRide = () => {
    const [startAddress, setStartAddress] = useState('');
    const [arriveAddress, setArriveAddress] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (startAddress.trim() === '' || arriveAddress.trim() === '') {
            setErrorMessage('Please enter both start and arrive addresses.');
            return;
        }

        try {
            const data = await createNewRide(startAddress, arriveAddress);
            console.log(data.id);
            if (data) {
                window.alert('Ride created successfully!');
                navigate(`/confirmOrder/${data.id}`);
            }
        } catch (error) {
            setErrorMessage('Ride creation failed. Please try again.');
            console.error('Ride creation error:', error);
        }
    };

    const HandleBack = () => {
        navigate('/');
    }

    return (
        <div>
            <h1>Create New Ride</h1>
            <form onSubmit={handleSubmit}>
                <label>Start Address:</label><br />
                <input
                    type="text"
                    value={startAddress}
                    onChange={(e) => setStartAddress(e.target.value)}
                /><br />
                <label>Arrive Address:</label><br />
                <input
                    type="text"
                    value={arriveAddress}
                    onChange={(e) => setArriveAddress(e.target.value)}
                /><br />
                {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
                <button type="submit">Order a ride</button>
            </form>
            <button onClick={HandleBack}>Back</button>
        </div>
    );
};