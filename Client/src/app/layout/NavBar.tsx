import { AppBar, Box, Button, Container, IconButton, Toolbar, Typography } from "@mui/material";
import Menu from '@mui/icons-material/Menu';

export default function Navbar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" sx={{
                backgroundImage: 'linear-gradient(135deg, #182a73 0%, #218aae 69%, #20a7ac 89%)'
            }}>
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
                        <Button color="inherit">Login</Button>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    )
}