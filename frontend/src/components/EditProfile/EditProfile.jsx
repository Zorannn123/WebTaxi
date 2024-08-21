import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserProfile, updateUserProfile } from '../../services/userService';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import "@fontsource/roboto";

export const EditProfile = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [address, setAddress] = useState('');
    const [role, setRole] = useState('');
    const [image, setImage] = useState(null);
    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const navigate = useNavigate();

    //TODO:
    const fetchUserProfile = async () => {

        console.log("localStorage.getItem('userImg')", localStorage.getItem('userImg'))

        try {
            const token = localStorage.getItem('authToken');
            const response = await getUserProfile(token);
            const userProfile = response;
            console.log(response);
            if (userProfile) {
                setUsername(userProfile.userName);
                setFirstName(userProfile.firstName);
                setLastName(userProfile.lastName);
                setEmail(userProfile.email);
                setPassword(userProfile.password);
                setRole(userProfile.role);
                setAddress(userProfile.address);
                convertImageToBase64FromSrc(localStorage.getItem('userImg'), (base64Image => {
                    console.log("base64Image", base64Image)
                    setImage(base64Image);
                }))

                setDateOfBirth(userProfile.dateOfBirth);
            }
            else {
                setErrorMessage('Error fetching user profile:');
            }
        } catch (error) {
            console.error('Error fetching user profile:', error);
        }

        console.log("image", image)
    };

    useEffect(() => {
        fetchUserProfile();
    }, []);


    const handleFileChange = (e) => {
        console.log("uso")
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64Image = reader.result.split(',')[1];
                setImage(base64Image);
            };
            reader.readAsDataURL(file);
        }
    };


    const handleSubmit = async (e) => {
        e.preventDefault();

        const userData = {
            username,
            email,
            password,
            firstName,
            lastName,
            dateOfBirth,
            address,
            role,
            image: image || localStorage.getItem('userImg')
        };
        console.log(userData)

        try {
            const result = await updateUserProfile(userData);
            console.log(result);
            if (result) {
                setSuccessMessage('Profile updated successfully!');
                setErrorMessage('');
                window.alert("Profile updated successfully!");
                navigate('/yourProfile');
            } else {
                setErrorMessage('Profile update failed.');
            }
        } catch (error) {
            setErrorMessage('Profile update failed. Please try again.');
            console.error('Profile update error:', error);
        }
    };
    const HandleBack = () => {
        navigate("/yourProfile");
    }

    const convertImageToBase64FromSrc = (imageUrl, callback) => {
        const img = new Image();
        img.crossOrigin = 'Anonymous'; // This might help with CORS issues, but it's not always effective
        img.onload = () => {
            // Create a canvas to draw the image
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');

            // Set canvas dimensions to image dimensions
            canvas.width = img.width;
            canvas.height = img.height;

            // Draw the image on canvas
            ctx.drawImage(img, 0, 0);

            // Get the Base64 string from the canvas
            const base64Image = canvas.toDataURL('image/jpeg');

            // Optionally, strip the MIME type from the result
            const base64Data = base64Image.split(',')[1];

            callback(base64Data);
        };

        img.onerror = (error) => {
            console.error('Error loading image:', error);
        };

        img.src = imageUrl;
    };


    const convertImageToBase64 = async (imageUrl) => {
        try {
            // Fetch the image from the URL
            const response = await fetch(imageUrl);
            if (!response.ok) {
                throw new Error('Failed to fetch the image.');
            }

            // Convert the response to a Blob
            const blob = await response.blob();

            // Convert the Blob to a Base64 string
            const reader = new FileReader();
            return new Promise((resolve, reject) => {
                reader.onloadend = () => {
                    // `reader.result` contains the Base64 string
                    resolve(reader.result.split(',')[1]); // Remove the data URL prefix
                };
                reader.onerror = () => reject(new Error('Failed to read the image.'));
                reader.readAsDataURL(blob);
            });
        } catch (error) {
            console.error('Error converting image to Base64:', error);
        }
    };

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100vh',
                textAlign: 'center',
                marginTop: '-40px',
                padding: 2,
                backgroundColor: "#f8f9fa"
            }}
        >
            <Typography variant="h4" component="h1" sx={{ marginBottom: '30px', fontFamily: "Roboto" }}>
                Edit Profile
            </Typography>

            <form onSubmit={handleSubmit}>
                <TextField
                    label="Username"
                    variant="outlined"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <TextField
                    label="First Name"
                    variant="outlined"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <TextField
                    label="Last Name"
                    variant="outlined"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <TextField
                    label="Date of Birth"
                    type="date"
                    variant="outlined"

                    InputLabelProps={{ shrink: true }}
                    value={dateOfBirth}
                    onChange={(e) => setDateOfBirth(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <TextField
                    label="Address"
                    variant="outlined"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    sx={{ marginBottom: '16px', width: "300px" }}
                />
                <br />
                <input
                    type="file"
                    onChange={handleFileChange}
                    style={{ marginBottom: '16px' }}
                />
                <br />
                {image && <img src={`data:image/jpeg;base64,${image}`} alt="uploaded" style={{ marginBottom: '16px', maxWidth: '200px' }} />}
                <br />
                {errorMessage && (
                    <Typography variant="body2" color="error" sx={{ marginBottom: '16px' }}>
                        {errorMessage}
                    </Typography>
                )}
                {successMessage && (
                    <Typography variant="body2" color="success" sx={{ marginBottom: '16px' }}>
                        {successMessage}
                    </Typography>
                )}

                <Box>
                    <Button
                        type="submit"
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: '#f7e32f', color: 'black', marginRight: '20px' }}
                    >
                        Update Profile
                    </Button>
                    <Button
                        variant="outlined"
                        sx={{ marginBottom: '8px', backgroundColor: 'black', color: '#f7e32f' }}
                        onClick={HandleBack}
                    >
                        Back
                    </Button>
                </Box>
            </form>
        </Box>
    );
};
