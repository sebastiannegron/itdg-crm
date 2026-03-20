import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { fieldnames } from "@/app/[locale]/_shared/app-fieldnames";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
    className?: string;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
  usePathname: () => "/portal/messages",
  useRouter: () => ({ push: vi.fn() }),
}));

const mockSendPortalMessage = vi.fn().mockResolvedValue({ success: true, message: "OK" });
const mockMarkPortalMessageAsRead = vi.fn().mockResolvedValue({ success: true, message: "OK" });
const mockFetchPortalMessages = vi.fn().mockResolvedValue({ success: true, data: [] });

vi.mock(
  "@/app/[locale]/(portal)/portal/messages/actions",
  () => ({
    sendPortalMessage: (...args: unknown[]) => mockSendPortalMessage(...args),
    markPortalMessageAsRead: (...args: unknown[]) => mockMarkPortalMessageAsRead(...args),
    fetchPortalMessages: (...args: unknown[]) => mockFetchPortalMessages(...args),
  }),
);

import MessagesView from "@/app/[locale]/(portal)/portal/messages/MessagesView";

const t = fieldnames["en-pr"];

const mockMessages = [
  {
    id: "m1",
    client_id: "c1",
    sender_id: "s1",
    direction: "Inbound",
    subject: "Tax Filing Update",
    body: "Your tax filing has been processed successfully.",
    template_id: null,
    is_portal_message: true,
    is_read: false,
    attachments: null,
    created_at: "2025-06-15T10:30:00Z",
  },
  {
    id: "m2",
    client_id: "c1",
    sender_id: "c1",
    direction: "Outbound",
    subject: "Question about documents",
    body: "I have a question about the documents needed.",
    template_id: null,
    is_portal_message: true,
    is_read: true,
    attachments: null,
    created_at: "2025-06-14T09:00:00Z",
  },
];

describe("MessagesView", () => {
  beforeEach(() => {
    mockSendPortalMessage.mockReset().mockResolvedValue({ success: true, message: "OK" });
    mockMarkPortalMessageAsRead.mockReset().mockResolvedValue({ success: true, message: "OK" });
    mockFetchPortalMessages.mockReset().mockResolvedValue({ success: true, data: [] });
  });

  describe("Inbox List", () => {
    it("renders inbox title and new message button", () => {
      render(<MessagesView initialMessages={mockMessages} />);
      expect(screen.getByText(t.messages_inbox)).toBeInTheDocument();
      expect(screen.getByText(t.messages_new)).toBeInTheDocument();
    });

    it("renders message subjects in the list", () => {
      render(<MessagesView initialMessages={mockMessages} />);
      expect(screen.getByText("Tax Filing Update")).toBeInTheDocument();
      expect(screen.getByText("Question about documents")).toBeInTheDocument();
    });

    it("shows unread badge for unread messages", () => {
      render(<MessagesView initialMessages={mockMessages} />);
      expect(screen.getByText(t.messages_unread)).toBeInTheDocument();
    });

    it("shows sender labels correctly", () => {
      render(<MessagesView initialMessages={mockMessages} />);
      expect(screen.getByText(t.messages_from_advisor)).toBeInTheDocument();
      expect(screen.getByText(t.messages_from_you)).toBeInTheDocument();
    });

    it("renders empty state when no messages", () => {
      render(<MessagesView initialMessages={[]} />);
      expect(screen.getByText(t.messages_empty_title)).toBeInTheDocument();
      expect(screen.getByText(t.messages_empty_message)).toBeInTheDocument();
    });
  });

  describe("Message Detail", () => {
    it("shows message detail when clicking a message", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText("Tax Filing Update"));

      expect(
        screen.getByText("Your tax filing has been processed successfully."),
      ).toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: t.messages_back_to_inbox }),
      ).toBeInTheDocument();
    });

    it("shows reply form in message detail", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText("Tax Filing Update"));

      expect(screen.getByText(t.messages_reply)).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText(t.messages_reply_placeholder),
      ).toBeInTheDocument();
    });

    it("navigates back to inbox on back button click", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText("Tax Filing Update"));
      expect(
        screen.getByText("Your tax filing has been processed successfully."),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole("button", { name: t.messages_back_to_inbox }),
      );
      expect(screen.getByText(t.messages_inbox)).toBeInTheDocument();
    });

    it("marks message as read when opening an unread message", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText("Tax Filing Update"));

      expect(mockMarkPortalMessageAsRead).toHaveBeenCalledWith("m1");
    });

    it("does not mark already read message as read", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText("Question about documents"));

      expect(mockMarkPortalMessageAsRead).not.toHaveBeenCalled();
    });
  });

  describe("Compose View", () => {
    it("shows compose form when clicking new message", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText(t.messages_new));

      expect(
        screen.getByPlaceholderText(t.messages_new_subject_placeholder),
      ).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText(t.messages_new_body_placeholder),
      ).toBeInTheDocument();
    });

    it("navigates back to inbox from compose view", async () => {
      const user = userEvent.setup();
      render(<MessagesView initialMessages={mockMessages} />);

      await user.click(screen.getByText(t.messages_new));
      expect(
        screen.getByPlaceholderText(t.messages_new_subject_placeholder),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole("button", { name: t.messages_back_to_inbox }),
      );
      expect(screen.getByText(t.messages_inbox)).toBeInTheDocument();
    });
  });

  describe("i18n", () => {
    it("includes correct message fieldnames for en-pr locale", () => {
      expect(fieldnames["en-pr"].messages_inbox).toBe("Inbox");
      expect(fieldnames["en-pr"].messages_subject).toBe("Subject");
      expect(fieldnames["en-pr"].messages_unread).toBe("Unread");
      expect(fieldnames["en-pr"].messages_read).toBe("Read");
      expect(fieldnames["en-pr"].messages_reply).toBe("Reply");
      expect(fieldnames["en-pr"].messages_send).toBe("Send");
      expect(fieldnames["en-pr"].messages_new).toBe("New Message");
      expect(fieldnames["en-pr"].messages_empty_title).toBe("No messages yet");
    });

    it("includes correct message fieldnames for es-pr locale", () => {
      expect(fieldnames["es-pr"].messages_inbox).toBe("Bandeja de entrada");
      expect(fieldnames["es-pr"].messages_subject).toBe("Asunto");
      expect(fieldnames["es-pr"].messages_unread).toBe("No leído");
      expect(fieldnames["es-pr"].messages_read).toBe("Leído");
      expect(fieldnames["es-pr"].messages_reply).toBe("Responder");
      expect(fieldnames["es-pr"].messages_send).toBe("Enviar");
      expect(fieldnames["es-pr"].messages_new).toBe("Nuevo Mensaje");
      expect(fieldnames["es-pr"].messages_empty_title).toBe("Sin mensajes aún");
    });
  });
});
