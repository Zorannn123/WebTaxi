import React, { useState, useEffect } from "react";
import { verifyDriver, getDrivers, denyDriver } from "../../services/userService";
import { useNavigate } from "react-router-dom";

export const Drivers = () => {
    const [drivers, setDrivers] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const [verifyMessage, setVerifyMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchDrivers = async () => {
            try {
                const data = await getDrivers();
                setDrivers(data);
            } catch (error) {
                setErrorMessage('Failed to load drivers.');
            }
        };

        fetchDrivers();
    }, []);

    const handleVerify = async (driverId) => {
        try {
            await verifyDriver(driverId);
            setVerifyMessage(`Driver ${driverId} verified successfully!`);

            const data = await getDrivers();
            setDrivers(data);
        } catch (error) {
            setVerifyMessage(`Failed to verify driver ${driverId}.`);
        }
    };

    const handleDeny = async (driverId) => {
        try {
            await denyDriver(driverId);
            setVerifyMessage(`Driver ${driverId} denied successfully!`);

            const data = await getDrivers();
            setDrivers(data);
        } catch (error) {
            setVerifyMessage(`Failed to deny driver ${driverId}.`);
        }
    };

    const HandleBack = () => {
        navigate("/");
    }

    return (
        <div>
            <h1>Drivers</h1>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            {verifyMessage && <p style={{ color: 'green' }}>{verifyMessage}</p>}
            {drivers.length > 0 ? (
                <>
                    <table border={1}>
                        <thead>
                            <tr>
                                <th>First name</th>
                                <th>Last name</th>
                                <th>Username</th>
                                <th>Email</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {drivers.map((driver) => (
                                <tr key={driver.email}>
                                    <td>{driver.firstName}</td>
                                    <td>{driver.lastName}</td>
                                    <td>{driver.userName}</td>
                                    <td>{driver.email}</td>
                                    <td>
                                        {driver.verifyStatus === "Approved" ? "Verified" : "Not Verified"}
                                    </td>
                                    <td>
                                        {driver.verifyStatus === "OnHold" && (
                                            <>
                                                <button onClick={() => handleVerify(driver.email)}>Approve</button>
                                                <button onClick={() => handleDeny(driver.email)}>Deny</button>
                                            </>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <br />
                    <button onClick={HandleBack}>Back</button>
                </>

            ) : (
                <p>No drivers available.</p>
            )}
        </div>
    );
};
