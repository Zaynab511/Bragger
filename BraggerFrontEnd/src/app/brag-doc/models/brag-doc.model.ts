import { Tag } from "./tag.model";

export interface BragDoc {
  id?: number;
  title: string;
  description: string;
  date: string;
  userId: number;
  achievementTags: Tag[];  // Array of Tag objects
  aiGeneratedTags?: Tag[]; // Optional, in case of AI-generated tag
}
