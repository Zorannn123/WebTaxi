import axios from "axios";

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

export const login = async (email, password) => {
    try {
        const response = axios.post(`${API_BASE_URL}/user/login`, {
            email,
            password
        });
        return (await response).data;
    } catch (error) {
        throw error;
    }
}

export const registerUser = async (userData) => {
    try {
        const response = await axios.post(`${API_BASE_URL}/user/register`, userData, {
            headers: {
                'Content-Type': 'application/json',
            }
        });
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Fetch user profile
export const getUserProfile = async () => {
    try {
        const response = await axios.get(`${API_BASE_URL}/user/profile`);
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Update user profile
export const updateUserProfile = async (userData) => {
    try {
        const response = await axios.put(`${API_BASE_URL}/user/profile`, userData);
        return response.data;
    } catch (error) {
        throw error;
    }
};


