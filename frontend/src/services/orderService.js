import apiClient from "../components/AxiosClient/AxiosClient";

export const getEstimateOrder = async (id) => {
    try {
        const response = await apiClient.get(`/order/estimateOrder?orderId=${id}`)
        return response.data
    } catch (error) {
        throw error
    }
};

export const confirmOrder = async (id) => {
    try {
        const response = await apiClient.post(`/order/confirmOrder?orderId=${id}`)
        return response.data
    } catch (error) {
        throw error
    }
};

export const deleteOrder = async (id) => {
    try {
        const response = await apiClient.post(`/order/deleteOrder?orderId=${id}`)
        return response.data
    } catch (error) {
        throw error
    }
};

export const getOnHoldOrders = async () => {
    try {
        const response = await apiClient.get('/order/onHoldOrders')
        return response.data
    } catch (error) {
        throw error
    }
};

export const acceptOrder = async (id) => {
    try {
        const response = await apiClient.post(`/order/acceptOrder?orderId=${id}`)
        return response.data
    } catch (error) {
        throw error
    }
};