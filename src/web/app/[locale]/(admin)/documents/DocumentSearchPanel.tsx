"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { Search, FileText, Calendar, X } from "lucide-react";
import { Button } from "@/app/_components/ui/button";
import { Card, CardContent } from "@/app/_components/ui/card";
import { Badge } from "@/app/_components/ui/badge";
import { Input } from "@/app/_components/ui/input";
import { EmptyState } from "@/app/_components/EmptyState";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type {
  DocumentSearchResultDto,
  ClientDto,
  DocumentCategoryDto,
} from "./shared";
import { formatDate } from "./shared";
import { fetchSearchDocuments } from "./actions";

function sanitizeSnippet(html: string): string {
  return html
    .replace(/<(?!\/?em>)[^>]*>/gi, "")
    .replace(/&(?!amp;|lt;|gt;|quot;|#\d+;)/g, "&amp;");
}

interface DocumentSearchPanelProps {
  clients: ClientDto[];
  categories: DocumentCategoryDto[];
  onClose: () => void;
}

export default function DocumentSearchPanel({
  clients,
  categories,
  onClose,
}: DocumentSearchPanelProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const [isPending, startTransition] = useTransition();

  const [searchQuery, setSearchQuery] = useState("");
  const [selectedClientId, setSelectedClientId] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [results, setResults] = useState<DocumentSearchResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const handleSearch = useCallback(
    (searchPage: number = 1) => {
      if (!searchQuery.trim()) return;
      setStatus("loading");
      startTransition(async () => {
        const result = await fetchSearchDocuments({
          query: searchQuery.trim(),
          clientId: selectedClientId || undefined,
          category: selectedCategory || undefined,
          dateFrom: dateFrom || undefined,
          dateTo: dateTo || undefined,
          page: searchPage,
          pageSize,
        });
        if (result.success && result.data) {
          setResults(result.data.items);
          setTotalCount(result.data.total_count);
          setPage(searchPage);
          setStatus("success");
        } else {
          setResults([]);
          setTotalCount(0);
          setStatus("failed");
        }
      });
    },
    [searchQuery, selectedClientId, selectedCategory, dateFrom, dateTo, startTransition],
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "Enter") {
        handleSearch(1);
      }
    },
    [handleSearch],
  );

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <Card className="h-full overflow-hidden">
      <CardContent className="p-0 h-full flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between border-b p-3">
          <h2 className="text-sm font-semibold text-foreground flex items-center gap-2">
            <Search className="h-4 w-4" />
            {t.documents_search_title}
          </h2>
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={onClose}
            aria-label="Close search"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        {/* Search input + filters */}
        <div className="border-b p-3 space-y-2">
          <div className="flex gap-2">
            <Input
              placeholder={t.documents_search_placeholder}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              className="h-8 text-sm flex-1"
            />
            <Button
              size="sm"
              onClick={() => handleSearch(1)}
              disabled={!searchQuery.trim() || isPending}
              className="h-8"
            >
              <Search className="mr-1.5 h-3.5 w-3.5" />
              {t.documents_search_button}
            </Button>
          </div>

          {/* Filters row */}
          <div className="flex flex-wrap gap-2">
            <select
              value={selectedClientId}
              onChange={(e) => setSelectedClientId(e.target.value)}
              className="h-8 rounded-md border border-input bg-background px-2 text-xs"
              aria-label={t.documents_search_client}
            >
              <option value="">{t.documents_search_all_clients}</option>
              {clients.map((c) => (
                <option key={c.client_id} value={c.client_id}>
                  {c.name}
                </option>
              ))}
            </select>
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="h-8 rounded-md border border-input bg-background px-2 text-xs"
              aria-label={t.documents_search_category}
            >
              <option value="">{t.documents_search_all_categories}</option>
              {categories.map((cat) => (
                <option key={cat.category_id} value={cat.name}>
                  {cat.name}
                </option>
              ))}
            </select>
            <div className="flex items-center gap-1">
              <label className="text-xs text-muted-foreground">{t.documents_search_date_from}</label>
              <input
                type="date"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
                className="h-8 rounded-md border border-input bg-background px-2 text-xs"
              />
            </div>
            <div className="flex items-center gap-1">
              <label className="text-xs text-muted-foreground">{t.documents_search_date_to}</label>
              <input
                type="date"
                value={dateTo}
                onChange={(e) => setDateTo(e.target.value)}
                className="h-8 rounded-md border border-input bg-background px-2 text-xs"
              />
            </div>
          </div>
        </div>

        {/* Results */}
        <div className="flex-1 overflow-y-auto p-3">
          {status === "idle" ? (
            <EmptyState
              icon={Search}
              title={t.documents_search_title}
              message={t.documents_search_placeholder}
            />
          ) : isPending || status === "loading" ? (
            <div className="flex items-center justify-center py-12">
              <div className="h-6 w-6 animate-spin rounded-full border-2 border-muted-foreground border-t-transparent" />
            </div>
          ) : results.length === 0 ? (
            <EmptyState
              icon={Search}
              title={t.documents_search_no_results}
              message={t.documents_search_no_results_message}
            />
          ) : (
            <div className="space-y-2">
              <p className="text-xs text-muted-foreground mb-3">
                {totalCount} {t.documents_search_results.toLowerCase()}
              </p>
              {results.map((doc) => (
                <Card key={doc.document_id} className="hover:bg-accent/50 transition-colors">
                  <CardContent className="p-3">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex items-start gap-2 min-w-0">
                        <FileText className="h-5 w-5 text-muted-foreground mt-0.5 shrink-0" />
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-foreground truncate">
                            {doc.file_name}
                          </p>
                          <div className="flex flex-wrap items-center gap-1.5 mt-1">
                            <Badge variant="outline" className="text-xs">
                              {doc.client_name}
                            </Badge>
                            <Badge variant="secondary" className="text-xs">
                              {doc.category}
                            </Badge>
                            <span className="text-xs text-muted-foreground flex items-center gap-1">
                              <Calendar className="h-3 w-3" />
                              {formatDate(doc.uploaded_at, locale)}
                            </span>
                          </div>
                          {doc.relevance_snippet && (
                            <p
                              className="text-xs text-muted-foreground mt-1.5 line-clamp-2"
                              dangerouslySetInnerHTML={{ __html: sanitizeSnippet(doc.relevance_snippet) }}
                            />
                          )}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 pt-3">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1 || isPending}
                    onClick={() => handleSearch(page - 1)}
                  >
                    ←
                  </Button>
                  <span className="text-xs text-muted-foreground">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages || isPending}
                    onClick={() => handleSearch(page + 1)}
                  >
                    →
                  </Button>
                </div>
              )}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
