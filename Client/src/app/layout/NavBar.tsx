import {
  AppBar,
  Avatar,
  Box,
  Container,
  Toolbar,
  Button,
  Stack,
  Tooltip,
  CircularProgress
} from "@mui/material";
import MenuItemLink from "../shared/components/MenuItemLink";
import { useLogout } from "../../features/hooks/useAccount";
import { useAuthSession } from "../../features/hooks/useAuthSession";
import { useUserProfile } from "../../features/hooks/useUserProfile";
import { useNavigate, useLocation } from "react-router";
import { getAvatarUrl } from "../../lib/utils/avatar";
import { useVideos } from "../../features/hooks/useVideos";

export default function Navbar() {
    const logout = useLogout();
    const navigate = useNavigate();
    const location = useLocation();
    const { session } = useAuthSession();
    const { profile, isLoading: profileLoading } = useUserProfile(session?.userId);
    
    // Get loading state for videos page
    const isVideosPage = location.pathname.startsWith('/videos');
    const { isLoading: videosLoading, isFetchingNextPage: videosFetchingNextPage } = useVideos();
    const showLoadingIndicator = isVideosPage && (videosLoading || videosFetchingNextPage);

    const handleLogout = async () => {
        try {
            await logout.mutateAsync();
            navigate("/login");
        } catch {
            // error already logged in hook
        }
    };

    const handleAvatarClick = () => {
        if (session?.userId) {
            navigate(`/user/${session.userId}`);
        }
    };

    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="fixed" color="primary" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
                <Container maxWidth="xl">
                    <Toolbar sx={{ display: 'grid', gridTemplateColumns: '1fr auto 1fr', alignItems: 'center' }}>
                        <Stack direction="row" alignItems="center" spacing={2}>
                            <MenuItemLink to="/videos/upload" matchMode="startsWith">
                                Upload
                            </MenuItemLink>
                        </Stack>

                        <Stack direction="row" justifyContent="center" alignItems="center" spacing={1}>
                            <MenuItemLink
                                to="/videos"
                                matchMode="startsWith"
                            >
                                Highlights
                            </MenuItemLink>
                            {showLoadingIndicator && (
                                <CircularProgress 
                                    size={16} 
                                    sx={{ 
                                        color: 'inherit',
                                        animationDuration: '550ms'
                                    }} 
                                />
                            )}
                        </Stack>

                        <Stack direction="row" alignItems="center" spacing={1} justifyContent="flex-end">
                            {!session && (
                                <MenuItemLink to="/login" matchMode="startsWith">
                                    Login
                                </MenuItemLink>
                            )}
                            {session && (
                                <>
                                    <Tooltip title={`${session.displayName || session.email || "User"} - Click to view profile`}>
                                        <Avatar 
                                            src={profileLoading ? undefined : getAvatarUrl(profile?.avatarUrl)}
                                            onClick={handleAvatarClick}
                                            sx={{ 
                                                width: 32, 
                                                height: 32,
                                                cursor: 'pointer',
                                                transition: 'transform 0.2s',
                                                '&:hover': {
                                                    transform: 'scale(1.1)'
                                                }
                                            }}
                                        >
                                            {(session.displayName || session.email || "U").charAt(0).toUpperCase()}
                                        </Avatar>
                                    </Tooltip>
                                    <Button
                                        color="inherit"
                                        onClick={handleLogout}
                                        disabled={logout.isPending}
                                    >
                                        {logout.isPending ? "Logging out..." : "Logout"}
                                    </Button>
                                </>
                            )}
                        </Stack>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    )
}   