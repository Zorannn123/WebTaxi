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
//TODO:prebaciti u orderService.js
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

export const verifyDriver = async (driverId) => {
    try {
        const response = await apiClient.post(`/administrator/approveVerification?email=${driverId}`,
            null,
            {
                timeout: 30000
            })
        return response.data
    } catch (error) {
        console.error('Error verifying driver:', error);
        throw error;
    }
};

export const denyDriver = async (driverId) => {
    try {
        const response = await apiClient.post(`/administrator/denyVerification?email=${driverId}`,
            null,
            {
                timeout: 30000
            })
        return response.data
    } catch (error) {
        console.error('Error verifying driver:', error);
        throw error;
    }
};

export const getDrivers = async () => {
    try {
        const response = await apiClient.get('/administrator/drivers');
        return response.data;
    } catch (error) {
        console.error('Error fetching drivers:', error);
        throw error;
    }
};

export const rateRide = async (orderId, rate, driverId) => {
    try {
        const response = await apiClient.post('rate/rateRide', {
            orderId,
            rate,
            driverId
        }, {
            timeout: 30000
        });
        return response.data;
    } catch (error) {
        console.error('Error rating a ride: ', error);
        throw error;
    }
};

export const getDriverRating = async (driverId) => {
    try {
        const response = await apiClient.get(`rate/getRating?driverId=${driverId}`, {
            timeout: 30000
        });
        return response.data;
    } catch (error) {
        console.error('Error fetching driver rating: ', error);
        throw error;
    }
};

export const blockDriver = async (driverId) => {
    try {
        const response = await apiClient.post(`administrator/blockDriver?email=${driverId}`);
        return response.data;
    } catch (error) {
        console.error('Error blocking driver: ', error);
        throw error;
    }
};

export const unBlockDriver = async (driverId) => {
    try {
        const response = await apiClient.post(`administrator/unBlockDriver?email=${driverId}`);
        return response.data;
    } catch (error) {
        console.error('Error blocking driver: ', error);
        throw error;
    }
};

export const getBusyStatus = async () => {
    try {
        const response = await apiClient.get("user/busyStatus");
        return response.data;
    } catch (error) {
        console.error('Error blocking driver: ', error);
        throw error;
    }
};

export const getIsBlockedStatus = async () => {
    try {
        const response = await apiClient.get("user/isBlocked");
        return response.data;
    } catch (error) {
        console.error('Error blocking driver: ', error);
        throw error;
    }
};

