import { AppBar, Box, Container, IconButton, Toolbar, Typography } from "@mui/material";
import Menu from '@mui/icons-material/Menu';
import MenuItemLink from "../shared/components/MenuItemLink";

export default function Navbar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="primary">
                <Container maxWidth="xl">
                    <Toolbar sx={{ position: 'relative', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <IconButton
                            size="large"
                            edge="start"
                            color="inherit"
                            aria-label="menu"
                            sx={{ mr: 2 }}
                        >
                            <Menu />
                        </IconButton>
                        <Typography
                            variant="h5"
                            component="div"
                            sx={{
                                position: 'absolute',
                                left: '50%',
                                transform: 'translateX(-50%)',
                                pointerEvents: 'none'
                            }}
                        >
                            Highlights
                        </Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <MenuItemLink
                                to="/videos"
                                matchMode="startsWith"
                                exclude={["/videos/upload"]}
                            >
                                Videos
                            </MenuItemLink>
                            <MenuItemLink to="/videos/upload" matchMode="startsWith">
                                Upload
                            </MenuItemLink>
                            <MenuItemLink to="/login" matchMode="startsWith">
                                Login
                            </MenuItemLink>
                        </Box>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    )
}   