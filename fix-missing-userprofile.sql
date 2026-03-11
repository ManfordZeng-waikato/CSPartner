-- Fix: Create UserProfile for Identity users that don't have one
-- Run in Supabase SQL Editor when you see "User profile for user xxx does not exist"

INSERT INTO "UserProfiles" ("Id", "UserId", "DisplayName", "Bio", "AvatarUrl", "SteamProfileUrl", "FaceitProfileUrl", "CreatedAtUtc", "UpdatedAtUtc")
SELECT
    gen_random_uuid(),
    u."Id",
    COALESCE(u."UserName", 'User'),
    NULL,
    NULL,
    NULL,
    NULL,
    NOW() AT TIME ZONE 'UTC',
    NULL
FROM "AspNetUsers" u
WHERE NOT EXISTS (SELECT 1 FROM "UserProfiles" p WHERE p."UserId" = u."Id");
