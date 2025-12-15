
import { createBrowserRouter } from "react-router";
import App from "../layout/App";
import Login from "@mui/icons-material/Login";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";

export const router =createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
           
            {
                path: 'login',
                element: <Login />
            },
            {
                path: '',
                element: <VideoDashboard />
            },
            
        ]
    }
])