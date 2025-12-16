import { AppBar, Box, Container, IconButton, Toolbar, Typography } from "@mui/material";
import Menu from '@mui/icons-material/Menu';
import MenuItemLink from "../shared/components/MenuItemLink";

export default function Navbar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="primary">
                <Container maxWidth="xl">
                    <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <IconButton
                            size="large"
                            edge="start"
                            color="inherit"
                            aria-label="menu"
                            sx={{ mr: 2 }}
                        >
                            <Menu />
                        </IconButton>
                        <Typography variant="h5" component="div" >
                            HighlightsHub
                        </Typography>
                        <MenuItemLink to="/login">Login</MenuItemLink>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    )
}   