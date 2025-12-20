
import { createBrowserRouter } from "react-router";
import LoginPage from "../../features/Auth/LoginPage";
import SigninPage from "../../features/Auth/SigninPage";
import VideoDetailPage from "../../features/Videos/details/videoDetailPage";
import App from "../../app/layout/App";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";
import VideoUploadPage from "../../features/Videos/VideoUploadPage";
import RequireAuth from "../shared/components/RequireAuth.tsx";
import UserProfilePage from "../../features/userProfile/UserProfilepage";

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
                path: 'signup',
                element: <SigninPage />
            },
           
            {
                path: 'videos',
                element: <VideoDashboard />
            },
            {
                path: 'videos/upload',
                element: (
                    <RequireAuth>
                        <VideoUploadPage />
                    </RequireAuth>
                )
            },
            {
                path: 'video/:id',
                element: <VideoDetailPage />
            },
            {
                path: 'user/:id',
                element: <UserProfilePage />
            },
        ]
    }
])