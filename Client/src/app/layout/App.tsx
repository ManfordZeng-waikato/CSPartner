import { Container, CssBaseline, Box } from "@mui/material"
import NavBar from "./NavBar"
import { Outlet, useLocation } from "react-router"
import HomePage from "../../features/home/HomePage";

function App() {
  const location = useLocation();


  return (
    <>
      <CssBaseline />
      {location.pathname === '/' ? (
        <HomePage />
      ) : (
        <Container maxWidth={false} sx={{ py: 2, backgroundColor: '#F5F5F5', minHeight: '100vh', px: { xs: 2, sm: 3, md: 4 } }}>
          <NavBar />
          <Box sx={{ mt: 3 }}>
            <Outlet />
          </Box>
        </Container>
      )}
    </>
  )
}

export default App
