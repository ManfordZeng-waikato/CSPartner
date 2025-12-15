import { Container, CssBaseline, Box } from "@mui/material"
import NavBar from "./NavBar"
import VideoList from "../../features/Videos/Dashboard/VideoDashboard"

function App() {
  return (
    <Container maxWidth={false} sx={{ py: 2, backgroundColor: '#eeeeee', minHeight: '100vh', px: { xs: 2, sm: 3, md: 4 } }}>
      <CssBaseline />
      <NavBar />
      <Box sx={{ mt: 3 }}>
        <VideoList />
      </Box>
    </Container>
  )
}

export default App
