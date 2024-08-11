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

