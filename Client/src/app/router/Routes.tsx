
import { createBrowserRouter } from "react-router";
import LoginPage from "../../features/Auth/LoginPage";
import SigninPage from "../../features/Auth/SigninPage";
import ConfirmEmailPage from "../../features/Auth/ConfirmEmailPage";
import CheckEmailPage from "../../features/Auth/CheckEmailPage";
import ForgotPasswordPage from "../../features/Auth/ForgotPasswordPage";
import ResetPasswordPage from "../../features/Auth/ResetPasswordPage";
import GitHubCallbackPage from "../../features/Auth/GitHubCallbackPage";
import VideoDetailPage from "../../features/Videos/details/videoDetailPage";
import App from "../../app/layout/App";
import VideoDashboard from "../../features/Videos/dashboard/VideoDashboard";
import VideoUploadPage from "../../features/Videos/VideoUploadPage";
import RequireAuth from "../shared/components/RequireAuth.tsx";
import RequireGuest from "../shared/components/RequireGuest.tsx";
import UserProfilePage from "../../features/userProfile/UserProfilePage";
import EditProfilePage from "../../features/userProfile/EditProfilePage";

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [

            {
                path: 'login',
                element: (
                    <RequireGuest>
                        <LoginPage />
                    </RequireGuest>
                )
            },
            {
                path: 'signup',
                element: (
                    <RequireGuest>
                        <SigninPage />
                    </RequireGuest>
                )
            },
            {
                path: 'confirm-email',
                element: (
                    <RequireGuest>
                        <ConfirmEmailPage />
                    </RequireGuest>
                )
            },
            {
                path: 'check-email',
                element: (
                    <RequireGuest>
                        <CheckEmailPage />
                    </RequireGuest>
                )
            },
            {
                path: 'forgot-password',
                element: (
                    <RequireGuest>
                        <ForgotPasswordPage />
                    </RequireGuest>
                )
            },
            {
                path: 'reset-password',
                element: (
                    <RequireGuest>
                        <ResetPasswordPage />
                    </RequireGuest>
                )
            },
            {
                path: 'auth/callback',
                element: <GitHubCallbackPage />
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
            {
                path: 'user/:id/edit',
                element: (
                    <RequireAuth>
                        <EditProfilePage />
                    </RequireAuth>
                )
            },
        ]
    }
])