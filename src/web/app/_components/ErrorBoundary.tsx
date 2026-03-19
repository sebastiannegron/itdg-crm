"use client";

import { Component, type ErrorInfo, type ReactNode } from "react";
import { cn } from "@/lib/utils";
import { AlertTriangle } from "lucide-react";
import { Button } from "@/app/_components/ui/button";

interface ErrorBoundaryProps {
  children: ReactNode;
  fallbackTitle?: string;
  fallbackMessage?: string;
  className?: string;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error("ErrorBoundary caught an error:", error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        <div
          className={cn(
            "flex flex-col items-center justify-center gap-4 py-12 text-center",
            this.props.className
          )}
          role="alert"
        >
          <div className="rounded-full bg-destructive/10 p-4">
            <AlertTriangle
              className="h-8 w-8 text-destructive"
              aria-hidden="true"
            />
          </div>
          <div className="space-y-1">
            <h3 className="text-lg font-semibold text-foreground">
              {this.props.fallbackTitle ?? "Something went wrong"}
            </h3>
            <p className="text-sm text-muted-foreground">
              {this.props.fallbackMessage ??
                "An unexpected error occurred. Please try again."}
            </p>
          </div>
          <Button variant="outline" onClick={this.handleReset}>
            Try again
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}
