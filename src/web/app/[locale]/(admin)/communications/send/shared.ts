import type { CommunicationTemplateDto } from "@/server/Services/templateService";
import type { RenderedTemplateDto } from "@/server/Services/templateService";

export type { CommunicationTemplateDto, RenderedTemplateDto };

export type SendStep = "select" | "preview" | "confirm";

export interface ClientOption {
  client_id: string;
  name: string;
  contact_email: string | null;
}
