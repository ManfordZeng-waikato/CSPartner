
import { createBrowserRouter } from "react-router";
import App from "../layout/App";
import LoginPage from "../../features/Auth/LoginPage";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";

export const router =createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
           
            {
                path: 'login',
                element: <LoginPage />
            },
            {
                path: '',
                element: <VideoDashboard />
            },
            
        ]
    }
])