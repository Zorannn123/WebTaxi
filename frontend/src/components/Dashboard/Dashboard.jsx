import React from "react";
import { Navbar } from "../Navbar/Navbar";
import dashboardImage from "../assets/taxiImg.png";

export const Dashboard = () => {

    return (
        <>
            <Navbar />
            <div style={{ display: "flex" }}>
                <img src={dashboardImage} alt="Dashboard" style={{ width: '100%', height: '100%', marginTop: "50px" }} />
            </div>
        </>
    );
};