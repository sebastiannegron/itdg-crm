import {
  getPortalMessages,
  type MessageDto,
} from "@/server/Services/portalMessageService";
import MessagesView from "./MessagesView";

export default async function MessagesPage() {
  let messages: MessageDto[];
  try {
    messages = await getPortalMessages();
  } catch (error) {
    console.error("[MessagesPage] Failed to fetch messages:", error);
    messages = [];
  }

  return <MessagesView initialMessages={messages} />;
}
