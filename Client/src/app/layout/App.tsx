import { Container, CssBaseline, Box, useTheme, useMediaQuery } from "@mui/material"
import NavBar from "./NavBar"
import { Outlet, useLocation } from "react-router"
import HomePage from "../../features/home/HomePage";

function App() {
  const location = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  // MUI Toolbar default height: 64px on desktop, 56px on mobile
  const toolbarHeight = isMobile ? 56 : 64;

  return (
    <>
      <CssBaseline />
      {location.pathname === '/' ? (
        <HomePage />
      ) : (
        <>
          <NavBar />
          <Container 
            maxWidth={false} 
            sx={{ 
              pt: `${toolbarHeight + 8}px`, // AppBar height + some spacing
              pb: 2,
              backgroundColor: '#F5F5F5', 
              minHeight: '100vh', 
              px: { xs: 2, sm: 3, md: 4 } 
            }}
          >
            <Box sx={{ mt: 3 }}>
              <Outlet />
            </Box>
          </Container>
        </>
      )}
    </>
  )
}

export default App
