import { Box, Button, Paper, Typography } from "@mui/material";
import { Link } from "react-router";

export default function HomePage() {
    return (
        <Paper sx={{
            gap: 6,
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            alignItems: 'center',
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            width: '100vw',
            height: '100vh',
            backgroundImage: 'url(/Home.png)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            backgroundRepeat: 'no-repeat',
            overflow: 'auto'
        }}>
            <Box sx={{
                display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3
            }}>
             <Box
               component={Link}
               to="/videos"
               sx={{
                 display: 'flex',
                 flexDirection: 'column',
                 alignItems: 'center',
                 gap: 3,
                 textDecoration: 'none',
                 cursor: 'pointer',
                 transition: 'transform 0.2s ease-in-out',
                 '&:hover': {
                   transform: 'scale(1.02)'
                 }
               }}
             >
               <Typography 
                 variant="h1" 
                 sx={{
                   fontWeight: 900,
                   fontSize: { xs: '3rem', sm: '4rem', md: '5rem' },
                   textShadow: '4px 4px 8px rgba(0, 0, 0, 0.8), 0 0 20px rgba(0, 0, 0, 0.5)',
                   letterSpacing: '0.1em',
                   color: '#FFFFFF',
                   textAlign: 'center',
                   pointerEvents: 'none'
                 }}
               >
                 CSPartner
               </Typography>
               <Button 
                 variant="contained" 
                 size="large" 
                 sx={{ 
                   fontSize: '1.5rem',
                   fontWeight: 600,
                   color: '#FFFFFF',
                   borderRadius: 2,
                   px: 4,
                   py: 1.5,
                   minHeight: 56,
                   backgroundColor: '#FF6B35',
                   textShadow: '1px 1px 3px rgba(0, 0, 0, 0.5)',
                   boxShadow: '0 4px 6px rgba(0, 0, 0, 0.3)',
                   pointerEvents: 'none',
                   '&:hover': {
                     backgroundColor: '#E55100',
                     boxShadow: '0 6px 8px rgba(0, 0, 0, 0.4)',
                     transform: 'translateY(-2px)'
                   }
                 }}
               >       
                 Get Started
               </Button>
             </Box>
            </Box>
           
        </Paper>
    )
}  