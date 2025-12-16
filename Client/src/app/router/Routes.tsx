
import { createBrowserRouter } from "react-router";
import LoginPage from "../../features/Auth/LoginPage";
import VideoDetail from "../../features/Videos/details/videoDetail";
import App from "../../app/layout/App";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";

export const router = createBrowserRouter([
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
            {
                path: 'video/:id',
                element: <VideoDetail />
            },

        ]
    }
])