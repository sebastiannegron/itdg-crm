import {
  getGoogleConnectionStatus,
  getGmailConnectionStatus,
  getCalendarConnectionStatus,
  getMsGraphConnectionStatus,
  getAzureOpenAiConnectionStatus,
  type GoogleConnectionStatusDto,
  type GmailConnectionStatusDto,
  type CalendarConnectionStatusDto,
  type MsGraphConnectionStatusDto,
  type AzureOpenAiConnectionStatusDto,
} from "@/server/Services/integrationService";
import IntegrationsView from "./IntegrationsView";

export default async function IntegrationsPage() {
  let googleDrive: GoogleConnectionStatusDto;
  let gmail: GmailConnectionStatusDto;
  let calendar: CalendarConnectionStatusDto;
  let msGraph: MsGraphConnectionStatusDto;
  let azureOpenAi: AzureOpenAiConnectionStatusDto;

  try {
    googleDrive = await getGoogleConnectionStatus();
  } catch (error) {
    console.error(
      "[IntegrationsPage] Failed to fetch Google Drive status:",
      error,
    );
    googleDrive = { is_connected: false, connected_at: null };
  }

  try {
    gmail = await getGmailConnectionStatus();
  } catch (error) {
    console.error(
      "[IntegrationsPage] Failed to fetch Gmail status:",
      error,
    );
    gmail = { is_connected: false, connected_at: null };
  }

  try {
    calendar = await getCalendarConnectionStatus();
  } catch (error) {
    console.error(
      "[IntegrationsPage] Failed to fetch Calendar status:",
      error,
    );
    calendar = { is_connected: false, connected_at: null };
  }

  try {
    msGraph = await getMsGraphConnectionStatus();
  } catch (error) {
    console.error(
      "[IntegrationsPage] Failed to fetch MS Graph status:",
      error,
    );
    msGraph = { is_configured: false };
  }

  try {
    azureOpenAi = await getAzureOpenAiConnectionStatus();
  } catch (error) {
    console.error(
      "[IntegrationsPage] Failed to fetch Azure OpenAI status:",
      error,
    );
    azureOpenAi = { is_configured: false };
  }

  return (
    <IntegrationsView
      initialGoogleDrive={googleDrive}
      initialGmail={gmail}
      initialCalendar={calendar}
      initialMsGraph={msGraph}
      initialAzureOpenAi={azureOpenAi}
    />
  );
}
