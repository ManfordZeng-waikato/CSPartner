import { Container, CssBaseline, Box } from "@mui/material"
import NavBar from "./NavBar"
import { Outlet } from "react-router"

function App() {
  return (
    <Container maxWidth={false} sx={{ py: 2, backgroundColor: '#eeeeee', minHeight: '100vh', px: { xs: 2, sm: 3, md: 4 } }}>
      <CssBaseline />
      <NavBar />
      <Box sx={{ mt: 3 }}>
       <Outlet />
      </Box>
    </Container>
  )
}

export default App
