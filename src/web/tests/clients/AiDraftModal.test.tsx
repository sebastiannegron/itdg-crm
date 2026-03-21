import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({ push: vi.fn() }),
  usePathname: () => "/clients/c1-uuid",
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

const mockGenerateAiDraft = vi.fn();

vi.mock("@/app/[locale]/(admin)/clients/[client_id]/actions", () => ({
  generateAiDraftAction: (...args: unknown[]) =>
    mockGenerateAiDraft(...args),
}));

import AiDraftModal from "@/app/[locale]/(admin)/clients/[client_id]/AiDraftModal";

describe("AiDraftModal", () => {
  const defaultProps = {
    clientName: "John Doe",
    onUseDraft: vi.fn(),
    onClose: vi.fn(),
  };

  beforeEach(() => {
    mockGenerateAiDraft.mockReset();
    defaultProps.onUseDraft.mockReset();
    defaultProps.onClose.mockReset();
  });

  it("renders the modal with title and warning", () => {
    render(<AiDraftModal {...defaultProps} />);
    expect(screen.getByText("AI Email Draft")).toBeInTheDocument();
    expect(
      screen.getByText(
        "AI drafts require your review. System will never auto-send.",
      ),
    ).toBeInTheDocument();
  });

  it("renders input step with topic field and generate button", () => {
    render(<AiDraftModal {...defaultProps} />);
    expect(
      screen.getByText("What would you like to communicate?"),
    ).toBeInTheDocument();
    expect(screen.getByText("Generate Draft")).toBeInTheDocument();
  });

  it("shows validation error when topic is empty and generate is clicked", async () => {
    render(<AiDraftModal {...defaultProps} />);
    fireEvent.click(screen.getByText("Generate Draft"));
    expect(screen.getByText("Topic is required.")).toBeInTheDocument();
  });

  it("calls generateAiDraftAction when generate is clicked with valid input", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Dear John Doe,\n\nRegarding your tax filing..." },
    });

    render(<AiDraftModal {...defaultProps} />);

    const textarea = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(textarea, {
      target: { value: "Remind about tax filing deadline" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(mockGenerateAiDraft).toHaveBeenCalledWith(
        expect.objectContaining({
          client_name: "John Doe",
          topic: "Remind about tax filing deadline",
          language: "en",
        }),
      );
    });
  });

  it("shows preview step with draft after successful generation", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Dear John Doe,\n\nRegarding your tax filing..." },
    });

    render(<AiDraftModal {...defaultProps} />);

    const textarea = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(textarea, {
      target: { value: "Remind about tax filing deadline" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(
        screen.getByText("AI Draft — review before sending"),
      ).toBeInTheDocument();
    });

    expect(screen.getByText("Use this draft")).toBeInTheDocument();
    expect(screen.getByText("Regenerate")).toBeInTheDocument();
  });

  it("shows error message when draft generation fails", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: false,
      message: "AI service unavailable",
    });

    render(<AiDraftModal {...defaultProps} />);

    const textarea = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(textarea, {
      target: { value: "Remind about tax filing deadline" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(screen.getByText("AI service unavailable")).toBeInTheDocument();
    });
  });

  it("calls onUseDraft when Use this draft is clicked", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Dear John Doe,\n\nDraft content here..." },
    });

    render(<AiDraftModal {...defaultProps} />);

    const textarea = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(textarea, {
      target: { value: "Remind about tax filing" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(screen.getByText("Use this draft")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByText("Use this draft"));

    expect(defaultProps.onUseDraft).toHaveBeenCalledWith(
      "Dear John Doe,\n\nDraft content here...",
    );
  });

  it("calls onClose when close button is clicked", () => {
    render(<AiDraftModal {...defaultProps} />);
    fireEvent.click(screen.getByLabelText("Close"));
    expect(defaultProps.onClose).toHaveBeenCalled();
  });

  it("calls onClose when Discard is clicked", () => {
    render(<AiDraftModal {...defaultProps} />);
    fireEvent.click(screen.getByText("Discard"));
    expect(defaultProps.onClose).toHaveBeenCalled();
  });

  it("returns to input step when Regenerate is clicked", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Some draft content" },
    });

    render(<AiDraftModal {...defaultProps} />);

    const textarea = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(textarea, {
      target: { value: "Remind about tax filing" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(screen.getByText("Regenerate")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByText("Regenerate"));

    expect(
      screen.getByText("What would you like to communicate?"),
    ).toBeInTheDocument();
  });

  it("passes additional context when provided", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Draft with context" },
    });

    render(<AiDraftModal {...defaultProps} />);

    const topicInput = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(topicInput, {
      target: { value: "Payment reminder" },
    });

    const contextInput = screen.getByPlaceholderText(
      /Client has outstanding balance/,
    );
    fireEvent.change(contextInput, {
      target: { value: "Outstanding balance of $5,000" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(mockGenerateAiDraft).toHaveBeenCalledWith(
        expect.objectContaining({
          additional_context: "Outstanding balance of $5,000",
        }),
      );
    });
  });

  it("allows editing the draft in preview step", async () => {
    mockGenerateAiDraft.mockResolvedValue({
      success: true,
      data: { draft: "Original draft content" },
    });

    render(<AiDraftModal {...defaultProps} />);

    const topicInput = screen.getByPlaceholderText(
      /Remind client about upcoming/,
    );
    fireEvent.change(topicInput, {
      target: { value: "Tax filing reminder" },
    });

    fireEvent.click(screen.getByText("Generate Draft"));

    await waitFor(() => {
      expect(screen.getByText("Use this draft")).toBeInTheDocument();
    });

    // Find the draft textarea (the one with the draft content)
    const draftTextarea = screen.getByDisplayValue("Original draft content");
    fireEvent.change(draftTextarea, {
      target: { value: "Edited draft content" },
    });

    fireEvent.click(screen.getByText("Use this draft"));

    expect(defaultProps.onUseDraft).toHaveBeenCalledWith(
      "Edited draft content",
    );
  });
});
