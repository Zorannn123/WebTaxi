import React, { useState, useEffect } from "react";
import { verifyDriver, getDrivers, denyDriver } from "../../services/userService";
import { useNavigate } from "react-router-dom";
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Button from '@mui/material/Button';
import Box from "@mui/material/Box";

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
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'left',
                padding: '20px',
            }}
        >
            <h1>Drivers</h1>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
            {verifyMessage && <p style={{ color: 'green' }}>{verifyMessage}</p>}
            {drivers.length > 0 ? (
                <>
                    <TableContainer component={Paper} sx={{ margin: 'auto', maxHeight: '700px' }}>
                        <Table sx={{ minWidth: 500 }} aria-label="drivers table">
                            <TableHead>
                                <TableRow>
                                    <TableCell>First name</TableCell>
                                    <TableCell>Last name</TableCell>
                                    <TableCell>Username</TableCell>
                                    <TableCell>Email</TableCell>
                                    <TableCell>Status</TableCell>
                                    <TableCell>Actions</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {drivers.map((driver) => (
                                    <TableRow key={driver.email}>
                                        <TableCell component="th" scope="row">
                                            {driver.firstName}
                                        </TableCell>
                                        <TableCell>{driver.lastName}</TableCell>
                                        <TableCell>{driver.userName}</TableCell>
                                        <TableCell>{driver.email}</TableCell>
                                        <TableCell>
                                            {driver.verifyStatus === "Approved" ? "Verified" : "Not Verified"}
                                        </TableCell>
                                        <TableCell>
                                            {driver.verifyStatus === "OnHold" && (
                                                <>
                                                    <Button
                                                        variant="contained"
                                                        color="primary"
                                                        onClick={() => handleVerify(driver.email)}
                                                        sx={{ marginRight: '5px' }}
                                                    >
                                                        Approve
                                                    </Button>
                                                    <Button
                                                        variant="contained"
                                                        color="secondary"
                                                        onClick={() => handleDeny(driver.email)}
                                                    >
                                                        Deny
                                                    </Button>
                                                </>
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    <br />
                    <Button variant="contained" sx={{ width: "30px" }} onClick={HandleBack}>Back</Button>
                </>
            ) : (
                <p>No drivers available.</p>
            )}
        </Box>
    );
};
