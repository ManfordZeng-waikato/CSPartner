import { z } from "zod";

const MAX_VIDEO_BYTES = 50 * 1024 * 1024; // 50MB

const countWords = (value: string) => {
  const trimmed = value.trim();
  if (!trimmed) return 0;
  return trimmed.split(/\s+/).length;
};

export const MAP_OPTIONS = [
  "Mirage",
  "Inferno",
  "Nuke",
  "Overpass",
  "Ancient",
  "Anubis",
  "Dust II"
] as const;

export const WEAPON_OPTIONS = [
  "Rifles",
  "Snipers",
  "Pistols",
  "Other"
] as const;

export const HIGHLIGHT_TYPE_OPTIONS = [
  "Clutch",
  "SprayTransfer",
  "OpeningKill"
] as const;

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
  map: z.enum(MAP_OPTIONS, {
    required_error: "Please select a map",
    invalid_type_error: "Please select a valid map"
  }),
  weapon: z.enum(WEAPON_OPTIONS, {
    required_error: "Please select a weapon type",
    invalid_type_error: "Please select a valid weapon type"
  }),
  highlightType: z.enum(HIGHLIGHT_TYPE_OPTIONS, {
    required_error: "Please select a highlight type",
    invalid_type_error: "Please select a valid highlight type"
  })
});

export type VideoUploadFormValues = z.infer<typeof videoUploadSchema>;

