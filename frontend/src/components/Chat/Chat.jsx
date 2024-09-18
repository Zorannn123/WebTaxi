import React, { useEffect, useRef, useState } from "react";
import { jwtDecode } from "jwt-decode";
import { getEstimateOrder } from "../../services/orderService";
import { useNavigate } from "react-router-dom";
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Box from "@mui/material/Box";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";

export const MessageChat = () => {
    const [messages, setMessages] = useState([]);
    const [message, setMessage] = useState('');
    const [role, setRole] = useState('');
    const [name, setName] = useState('');
    const [order, setOrder] = useState(null);
    const navigate = useNavigate();
    const [error, setError] = useState(null);
    const messagesEndRef = useRef(null);
    const ws = useRef(null);
    const token = localStorage.getItem('authToken');
    const orderId = localStorage.getItem('orderId');
    const socketUrl = `ws://localhost:8813/ws?token=${token}&orderId=${orderId}`;
    const decodedToken = jwtDecode(token);

    useEffect(() => {
        if (token) {

            setName(decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
            setRole(decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
            console.log('Name:', decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);

        };
        if (orderId) {
            console.log(orderId)
            const fetchOrder = async () => {
                try {
                    const orderData = await getEstimateOrder(orderId);
                    console.log(orderData);
                    setOrder(orderData);
                } catch (error) {
                    setError('Failed to fetch order details.');
                    console.error('Error fetching order:', error);
                }
            };
            fetchOrder();
        }
    }, []);

    useEffect(() => {
        ws.current = new WebSocket(socketUrl);

        ws.current.onopen = () => {
            console.log('WebSocket connected');
        };

        ws.current.onmessage = (event) => {
            try {
                const receivedMessages = JSON.parse(event.data);
                console.log(receivedMessages)
                if (Array.isArray(receivedMessages)) {
                    // If receivedMessages is an array, treat it as previous messages
                    setMessages(receivedMessages);
                } else {
                    // Otherwise, treat it as a single message
                    setMessages((prevMessages) => [...prevMessages, receivedMessages]);
                }
            } catch (error) {
                console.error('Error parsing WebSocket message:', error);
            }
        };

        ws.current.onclose = () => {
            console.log('WebSocket disconnected');
        };

        return () => {
            if (ws.current) {
                ws.current.close();
            }
        };
    }, []);

    useEffect(() => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages]);

    const handleSendMessage = async () => {
        if (ws.current && message.trim()) {
            const userRole = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
            let messageDto;

            if (userRole === "User") {
                messageDto = {
                    SenderId: order.userId,
                    ReceiverId: order.driverId,
                    Message: message,
                    SentAt: new Date().toISOString(),
                };
            } else if (userRole === "Driver") {
                messageDto = {
                    SenderId: order.driverId,
                    ReceiverId: order.userId,
                    Message: message,
                    SentAt: new Date().toISOString(),
                };
            } else {
                console.error("Unauthorized role");
                return;
            }

            // Send message via WebSocket
            ws.current.send(JSON.stringify(messageDto));
            setMessage(''); // Clear input field
            setMessages((prevMessages) => [...prevMessages, messageDto]); // Update messages
        } else {
            setError('Message cannot be empty.');
        }
    };

    const handleBackButtonClick = () => {
        navigate("/");
    };

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'flex-start',
                minHeight: '100vh',
                backgroundColor: '#f8f9fa',
                padding: 2
            }}
        >
            <Typography variant="h4" component="h1" sx={{ marginBottom: '30px', fontFamily: "Roboto" }}>
                Chat
            </Typography>

            <Box
                sx={{
                    width: '100%',
                    maxWidth: '300px',
                    marginBottom: '20px',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center'
                }}
            >
                <TextField
                    label="Type a message"
                    variant="outlined"
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    sx={{ marginBottom: '16px', width: '100%' }}
                />
                <Button
                    onClick={handleSendMessage}
                    variant="contained"
                    sx={{ marginBottom: '16px', backgroundColor: '#f7e32f', color: 'black', width: '100%' }}
                >
                    Send
                </Button>

                {role === "Driver" && (
                    <Button
                        onClick={handleBackButtonClick}
                        variant="outlined"
                        sx={{ backgroundColor: '#f8f9fa', color: 'black', width: '100%' }}
                    >
                        Exit Chat
                    </Button>
                )}
            </Box>

            <Box sx={{ width: '100%', maxWidth: '300px', backgroundColor: '#fff', padding: 2, borderRadius: '8px', boxShadow: '0 0 10px rgba(0, 0, 0, 0.1)' }}>
                <Typography variant="h5" component="h2" sx={{ marginBottom: '16px' }}>
                    Messages
                </Typography>
                <List sx={{ maxHeight: '200px', overflowY: 'auto' }}>
                    {messages
                        .slice() // Create a copy of the messages array
                        .sort((a, b) => new Date(a.SentAt) - new Date(b.SentAt)) // Sort by SentAt time
                        .map((msg, index) => (
                            <ListItem key={index}>
                                <ListItemText
                                    primary={<strong>{msg.SenderId.split('@')[0]}:</strong>}
                                    secondary={
                                        <>
                                            {msg.Message}
                                            <em style={{ marginLeft: '8px' }}>
                                                ({new Date(msg.SentAt).toLocaleTimeString()})
                                            </em>
                                        </>
                                    }
                                />
                            </ListItem>
                        ))}
                    <div ref={messagesEndRef} />
                </List>
            </Box>
        </Box>
    );
};
