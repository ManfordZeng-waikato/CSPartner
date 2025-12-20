import {
  AppBar,
  Avatar,
  Box,
  Container,
  Toolbar,
  Button,
  Stack,
  Tooltip
} from "@mui/material";
import MenuItemLink from "../shared/components/MenuItemLink";
import { useLogout } from "../../features/hooks/useAccount";
import { useAuthSession } from "../../features/hooks/useAuthSession";
import { useUserProfile } from "../../features/hooks/useUserProfile";
import { useNavigate } from "react-router";
import { getAvatarUrl } from "../../lib/utils/avatar";

export default function Navbar() {
    const logout = useLogout();
    const navigate = useNavigate();
    const { session } = useAuthSession();
    const { profile, isLoading: profileLoading } = useUserProfile(session?.userId);

    const handleLogout = async () => {
        try {
            await logout.mutateAsync();
            navigate("/login");
        } catch {
            // error already logged in hook
        }
    };

    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="primary">
                <Container maxWidth="xl">
                    <Toolbar sx={{ display: 'grid', gridTemplateColumns: '1fr auto 1fr', alignItems: 'center' }}>
                        <Stack direction="row" alignItems="center" spacing={2}>
                            <MenuItemLink to="/videos/upload" matchMode="startsWith">
                                Upload
                            </MenuItemLink>
                        </Stack>

                        <Stack direction="row" justifyContent="center">
                            <MenuItemLink
                                to="/videos"
                                matchMode="startsWith"
                            >
                                Highlights
                            </MenuItemLink>
                        </Stack>

                        <Stack direction="row" alignItems="center" spacing={1} justifyContent="flex-end">
                            {!session && (
                                <MenuItemLink to="/login" matchMode="startsWith">
                                    Login
                                </MenuItemLink>
                            )}
                            {session && (
                                <>
                                    <Tooltip title={session.displayName || session.email || "User"}>
                                        <Avatar 
                                            src={profileLoading ? undefined : getAvatarUrl(profile?.avatarUrl)}
                                            sx={{ width: 32, height: 32 }}
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