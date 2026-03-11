-- Seed script: run in Supabase SQL Editor to bypass Npgsql ObjectDisposedException
-- Idempotent: safe to run multiple times

-- 1. Insert demo videos (requires demo user to exist from `dotnet run -- seed` or manual creation)
INSERT INTO "Videos" (
    "Id", "UploaderUserId", "Title", "Description", "VideoUrl", "ThumbnailUrl",
    "LikeCount", "CommentCount", "ViewCount", "Visibility", "IsDeleted",
    "AiDescription", "TagsJson", "AiHighlightType", "AiStatus", "AiLastError", "AiUpdatedAtUtc",
    "CreatedAtUtc", "UpdatedAtUtc"
)
SELECT
    gen_random_uuid(),
    u."Id",
    v."Title",
    v."Description",
    v."VideoUrl",
    v."ThumbnailUrl",
    0, 0, 0, 1, false,
    NULL, NULL, 0, 0, NULL, NULL,
    NOW() AT TIME ZONE 'UTC',
    NULL
FROM (VALUES
    ('CS2 1v3 Clutch', 'Demo highlight', 'https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-BjNidspXxfeoDSzXtkjuNUjrYd.mp4', NULL),
    ('FACEIT Entry Frag', 'Demo highlight', 'https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-BnnHntrpFdmsqHQVHcgBuCejfq.mp4', NULL),
    ('3 Kills', 'Demo highlight', 'https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-iSGOUcIiMUEfdSONmiInQCnUy.mp4', NULL),
    ('4 Kills', 'Demo highlight', 'https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-cpwfgHvMjOjrdscmvXUipxJAW.mp4', NULL),
    ('DEMO', 'Demo highlight', 'https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-PPmluanzFFBeumBAJDqWXdhuT.mp4', NULL)
) AS v("Title", "Description", "VideoUrl", "ThumbnailUrl")
CROSS JOIN "AspNetUsers" u
WHERE u."NormalizedEmail" = 'DEMO@HIGHLIGHTHUB.LOCAL'
  AND NOT EXISTS (SELECT 1 FROM "Videos" v2 WHERE v2."VideoUrl" = v."VideoUrl");
