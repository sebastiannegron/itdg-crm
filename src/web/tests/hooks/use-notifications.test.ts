import { renderHook, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

// Declare mock functions using vi.hoisted so they are available in vi.mock factory
const {
  mockOn,
  mockOff,
  mockStart,
  mockStop,
  mockOnReconnecting,
  mockOnReconnected,
  mockOnClose,
} = vi.hoisted(() => ({
  mockOn: vi.fn(),
  mockOff: vi.fn(),
  mockStart: vi.fn(() => Promise.resolve()),
  mockStop: vi.fn(() => Promise.resolve()),
  mockOnReconnecting: vi.fn(),
  mockOnReconnected: vi.fn(),
  mockOnClose: vi.fn(),
}));

// Mock @microsoft/signalr
vi.mock("@microsoft/signalr", () => {
  const HubConnectionState = {
    Disconnected: "Disconnected",
    Connecting: "Connecting",
    Connected: "Connected",
    Disconnecting: "Disconnecting",
    Reconnecting: "Reconnecting",
  };

  const LogLevel = {
    Warning: 4,
  };

  const mockConnection = {
    on: mockOn,
    off: mockOff,
    start: mockStart,
    stop: mockStop,
    onreconnecting: mockOnReconnecting,
    onreconnected: mockOnReconnected,
    onclose: mockOnClose,
  };

  class MockHubConnectionBuilder {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() { return mockConnection; }
  }

  return {
    HubConnectionBuilder: MockHubConnectionBuilder,
    HubConnectionState,
    LogLevel,
  };
});

import { useNotifications, type NotificationDto } from "@/hooks/use-notifications";

describe("useNotifications", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockStart.mockReturnValue(Promise.resolve());
  });

  it("initializes with default empty state", () => {
    const { result } = renderHook(() => useNotifications());

    expect(result.current.notifications).toEqual([]);
    expect(result.current.unreadCount).toBe(0);
  });

  it("initializes with provided initial notifications", () => {
    const initialNotifications = [
      {
        id: "1",
        type: "document" as const,
        message: "Test notification",
        timestamp: "2024-01-01",
        read: false,
      },
    ];

    const { result } = renderHook(() =>
      useNotifications({ initialNotifications, initialUnreadCount: 3 }),
    );

    expect(result.current.notifications).toEqual(initialNotifications);
    expect(result.current.unreadCount).toBe(3);
  });

  it("connects to SignalR hub on mount", () => {
    renderHook(() => useNotifications());

    expect(mockStart).toHaveBeenCalledTimes(1);
  });

  it("registers ReceiveNotification and UnreadCountUpdated handlers", () => {
    renderHook(() => useNotifications());

    const registeredEvents = mockOn.mock.calls.map(
      (call) => call[0] as string,
    );
    expect(registeredEvents).toContain("ReceiveNotification");
    expect(registeredEvents).toContain("UnreadCountUpdated");
  });

  it("stops connection on unmount", () => {
    const { unmount } = renderHook(() => useNotifications());

    unmount();

    expect(mockOff).toHaveBeenCalled();
    expect(mockStop).toHaveBeenCalled();
  });

  it("prepends new notification on ReceiveNotification event", () => {
    const { result } = renderHook(() => useNotifications());

    // Find the ReceiveNotification handler
    const receiveCall = mockOn.mock.calls.find(
      (call) => call[0] === "ReceiveNotification",
    );
    const handler = receiveCall![1] as (dto: NotificationDto) => void;

    const dto: NotificationDto = {
      notification_id: "new-1",
      user_id: "user-1",
      event_type: "TaskAssigned",
      channel: "InApp",
      title: "New Task",
      body: "You have a new task",
      status: "Delivered",
      delivered_at: "2024-01-01T00:00:00Z",
      read_at: null,
      created_at: "2024-01-01T00:00:00Z",
    };

    act(() => {
      handler(dto);
    });

    expect(result.current.notifications).toHaveLength(1);
    expect(result.current.notifications[0].id).toBe("new-1");
    expect(result.current.notifications[0].type).toBe("task");
    expect(result.current.notifications[0].message).toBe(
      "You have a new task",
    );
    expect(result.current.notifications[0].read).toBe(false);
  });

  it("updates unread count on UnreadCountUpdated event", () => {
    const { result } = renderHook(() => useNotifications());

    // Find the UnreadCountUpdated handler
    const countCall = mockOn.mock.calls.find(
      (call) => call[0] === "UnreadCountUpdated",
    );
    const handler = countCall![1] as (count: number) => void;

    act(() => {
      handler(7);
    });

    expect(result.current.unreadCount).toBe(7);
  });

  it("maps event types to notification types correctly", () => {
    const { result } = renderHook(() => useNotifications());

    const receiveCall = mockOn.mock.calls.find(
      (call) => call[0] === "ReceiveNotification",
    );
    const handler = receiveCall![1] as (dto: NotificationDto) => void;

    const testCases = [
      { event_type: "DocumentUploaded", expectedType: "document" },
      { event_type: "PaymentCompleted", expectedType: "payment" },
      { event_type: "TaskAssigned", expectedType: "task" },
      { event_type: "EscalationReceived", expectedType: "escalation" },
      { event_type: "PortalMessageReceived", expectedType: "message" },
      { event_type: "SystemAlert", expectedType: "system" },
      { event_type: "UnknownType", expectedType: "system" },
    ];

    for (const { event_type, expectedType } of testCases) {
      act(() => {
        handler({
          notification_id: `id-${event_type}`,
          user_id: "user-1",
          event_type,
          channel: "InApp",
          title: "Test",
          body: "Test body",
          status: "Delivered",
          delivered_at: "2024-01-01T00:00:00Z",
          read_at: null,
          created_at: "2024-01-01T00:00:00Z",
        });
      });

      const latestNotification = result.current.notifications[0];
      expect(latestNotification.type).toBe(expectedType);
    }
  });

  it("exposes setNotifications for external state updates", () => {
    const { result } = renderHook(() => useNotifications());

    act(() => {
      result.current.setNotifications([
        {
          id: "ext-1",
          type: "document",
          message: "External update",
          timestamp: "now",
          read: true,
        },
      ]);
    });

    expect(result.current.notifications).toHaveLength(1);
    expect(result.current.notifications[0].id).toBe("ext-1");
  });

  it("exposes setUnreadCount for external state updates", () => {
    const { result } = renderHook(() => useNotifications());

    act(() => {
      result.current.setUnreadCount(42);
    });

    expect(result.current.unreadCount).toBe(42);
  });

  it("registers reconnection handlers", () => {
    renderHook(() => useNotifications());

    expect(mockOnReconnecting).toHaveBeenCalledTimes(1);
    expect(mockOnReconnected).toHaveBeenCalledTimes(1);
    expect(mockOnClose).toHaveBeenCalledTimes(1);
  });
});
