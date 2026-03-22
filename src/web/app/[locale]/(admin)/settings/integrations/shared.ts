import type { GoogleConnectionStatusDto } from "@/server/Services/integrationService";
import type { GmailConnectionStatusDto } from "@/server/Services/integrationService";
import type { CalendarConnectionStatusDto } from "@/server/Services/integrationService";
import type { MsGraphConnectionStatusDto } from "@/server/Services/integrationService";
import type { AzureOpenAiConnectionStatusDto } from "@/server/Services/integrationService";

export type {
  GoogleConnectionStatusDto,
  GmailConnectionStatusDto,
  CalendarConnectionStatusDto,
  MsGraphConnectionStatusDto,
  AzureOpenAiConnectionStatusDto,
};

export interface IntegrationStatusData {
  googleDrive: GoogleConnectionStatusDto;
  gmail: GmailConnectionStatusDto;
  calendar: CalendarConnectionStatusDto;
  msGraph: MsGraphConnectionStatusDto;
  azureOpenAi: AzureOpenAiConnectionStatusDto;
}
