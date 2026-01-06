import { Card, CardContent, Box, Skeleton } from "@mui/material";

export default function VideoCardSkeleton() {
    return (
        <Card elevation={3} sx={{ height: '100%', display: 'flex', flexDirection: 'column', position: 'relative', borderRadius: 3, overflow: 'hidden' }}>
            {/* Avatar skeleton */}
            <Skeleton
                variant="circular"
                width={40}
                height={40}
                sx={{
                    position: 'absolute',
                    top: 16,
                    right: 16,
                    zIndex: 1,
                }}
            />
            
            <CardContent sx={{ flexGrow: 1 }}>
                {/* Title skeleton */}
                <Skeleton variant="text" width="70%" height={32} sx={{ mb: 1 }} />
                
                {/* Description skeleton */}
                <Skeleton variant="text" width="90%" height={20} />
                <Skeleton variant="text" width="80%" height={20} sx={{ mb: 2 }} />
                
                {/* Stats skeleton */}
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                    <Skeleton variant="rounded" width={80} height={32} />
                    <Skeleton variant="rounded" width={80} height={32} />
                    <Skeleton variant="rounded" width={80} height={32} />
                </Box>
                
                {/* Video player skeleton */}
                <Skeleton 
                    variant="rectangular" 
                    width="100%" 
                    height={400}
                    sx={{ borderRadius: '8px' }}
                />
            </CardContent>
        </Card>
    );
}

