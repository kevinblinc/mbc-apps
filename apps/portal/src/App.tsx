import { Container, Box, Typography, AppBar, Toolbar, Button } from '@mui/material';

export default function App() {
  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            MBC Automation Portal
          </Typography>
          <Button color="inherit">Sign Out</Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Box sx={{ p: 3, bgcolor: '#f5f5f5', borderRadius: 2 }}>
          <Typography variant="h5" gutterBottom>
            Dashboard
          </Typography>
          <Typography variant="body1">
            This is the internal portal for managing automation services, logs, and configurations.
          </Typography>
        </Box>
      </Container>
    </>
  );
}
