
import { createBrowserRouter } from "react-router";
import LoginPage from "../../features/Auth/LoginPage";
import VideoDetailPage from "../../features/Videos/details/videoDetailPage";
import App from "../../app/layout/App";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";
import VideoUploadPage from "../../features/Videos/VideoUploadPage";

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
                path: 'videos',
                element: <VideoDashboard />
            },
            {
                path: 'videos/upload',
                element: <VideoUploadPage />
            },
            {
                path: 'video/:id',
                element: <VideoDetailPage />
            },

        ]
    }
])