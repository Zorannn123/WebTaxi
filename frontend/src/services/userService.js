import apiClient from "../components/AxiosClient/AxiosClient";

export const login = async (email, password) => {
    try {
        const response = await apiClient.post('/auth/login', {
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
        const response = await apiClient.post('/auth/register', userData);
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Fetch user profile
export const getUserProfile = async (token) => {
    try {
        const response = await apiClient.get('/user/currentProfile');
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Update user profile
export const updateUserProfile = async (userData) => {
    try {
        const response = await apiClient.post('user/editProfile', userData);
        return response.data;
    } catch (error) {
        throw error;
    }
};

export const createNewRide = async (startAddress, ArriveAddress) => {
    try {
        const response = await apiClient.post('/order/createNew', {
            startAddress,
            ArriveAddress
        })
        return response.data;
    } catch (error) {
        throw error;
    }
}

export const getPreviousOrdersUser = async () => {
    try {
        const response = await apiClient.get('order/previousOrders')
        return response.data
    } catch (error) {
        throw error;
    }
}


