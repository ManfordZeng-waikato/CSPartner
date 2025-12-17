import { z } from "zod";

const MAX_VIDEO_BYTES = 50 * 1024 * 1024; // 50MB

const countWords = (value: string) => {
  const trimmed = value.trim();
  if (!trimmed) return 0;
  return trimmed.split(/\s+/).length;
};

export const videoUploadSchema = z.object({
  title: z
    .string()
    .trim()
    .min(1, "Title is required")
    .superRefine((val, ctx) => {
      if (countWords(val) > 10) {
        ctx.addIssue({
          code: "custom",
          message: "Title must be 10 words or fewer"
        });
      }
    }),
  description: z
    .string()
    .trim()
    .optional()
    .superRefine((val, ctx) => {
      if (!val) return;
      if (countWords(val) > 20) {
        ctx.addIssue({
          code: "custom",
          message: "Description must be 20 words or fewer"
        });
      }
    }),
  videoFile: z.custom<File | null>((file) => {
    if (!(file instanceof File)) return false;
    if (file.size <= 0) return false;
    if (file.size > MAX_VIDEO_BYTES) return false;
    return true;
  }, { message: "Please select a video file under 50 MB" }),
  visibility: z.union([z.literal(1), z.literal(2)]),
  uploaderUserId: z.string().trim().min(1, "Uploader user ID is required")
});

export type VideoUploadFormValues = z.infer<typeof videoUploadSchema>;

