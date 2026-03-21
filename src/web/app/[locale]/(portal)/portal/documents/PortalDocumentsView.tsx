"use client";

import { useState, useCallback, useMemo, useTransition, useRef } from "react";
import { useLocale } from "next-intl";
import {
  FileText,
  FileImage,
  FileSpreadsheet,
  File,
  Upload,
  Download,
  Eye,
  Search,
  Filter,
} from "lucide-react";
import { Button } from "@/app/_components/ui/button";
import { Card, CardContent } from "@/app/_components/ui/card";
import { Badge } from "@/app/_components/ui/badge";
import { Input } from "@/app/_components/ui/input";
import { EmptyState } from "@/app/_components/EmptyState";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type PortalDocumentDto,
  type DocumentCategoryDto,
  formatFileSize,
  formatDate,
  getMimeTypeIcon,
  getYearOptions,
} from "./shared";
import {
  fetchPortalDocuments,
  fetchPortalDocumentDownload,
  uploadPortalDocumentAction,
} from "./actions";

function FileIcon({ mimeType }: { mimeType: string }) {
  const iconType = getMimeTypeIcon(mimeType);
  const className = "h-5 w-5 text-muted-foreground";
  switch (iconType) {
    case "pdf":
      return <FileText className={className} />;
    case "image":
      return <FileImage className={className} />;
    case "spreadsheet":
      return <FileSpreadsheet className={className} />;
    case "document":
      return <FileText className={className} />;
    default:
      return <File className={className} />;
  }
}

interface PortalDocumentsViewProps {
  initialDocuments: PortalDocumentDto[];
  initialCategories: DocumentCategoryDto[];
}

export default function PortalDocumentsView({
  initialDocuments,
  initialCategories,
}: PortalDocumentsViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const [isPending, startTransition] = useTransition();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // State
  const [documents, setDocuments] = useState<PortalDocumentDto[]>(initialDocuments);
  const [categories] = useState(initialCategories);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [selectedCategoryId, setSelectedCategoryId] = useState("");
  const [selectedYear, setSelectedYear] = useState("");
  const [searchQuery, setSearchQuery] = useState("");
  const [isDragOver, setIsDragOver] = useState(false);
  const [uploadCategoryId, setUploadCategoryId] = useState("");
  const [uploadStatus, setUploadStatus] = useState<PageStatus>("idle");
  const [uploadMessage, setUploadMessage] = useState("");

  const yearOptions = useMemo(() => getYearOptions(), []);

  // Load documents with filters
  const loadDocuments = useCallback(
    (categoryId?: string, year?: number, search?: string) => {
      setStatus("loading");
      startTransition(async () => {
        const result = await fetchPortalDocuments({
          categoryId: categoryId || undefined,
          year: year || undefined,
          search: search || undefined,
          page: 1,
          pageSize: 50,
        });
        if (result.success && result.data) {
          setDocuments(result.data.items);
          setStatus("success");
        } else {
          setDocuments([]);
          setStatus("failed");
        }
      });
    },
    [startTransition],
  );

  // Handle filter changes
  const handleCategoryChange = useCallback(
    (categoryId: string) => {
      setSelectedCategoryId(categoryId);
      loadDocuments(
        categoryId,
        selectedYear ? Number(selectedYear) : undefined,
        searchQuery || undefined,
      );
    },
    [loadDocuments, selectedYear, searchQuery],
  );

  const handleYearChange = useCallback(
    (year: string) => {
      setSelectedYear(year);
      loadDocuments(
        selectedCategoryId || undefined,
        year ? Number(year) : undefined,
        searchQuery || undefined,
      );
    },
    [loadDocuments, selectedCategoryId, searchQuery],
  );

  const handleSearch = useCallback(
    (query: string) => {
      setSearchQuery(query);
      loadDocuments(
        selectedCategoryId || undefined,
        selectedYear ? Number(selectedYear) : undefined,
        query || undefined,
      );
    },
    [loadDocuments, selectedCategoryId, selectedYear],
  );

  // View document
  const handleViewDocument = useCallback(
    (documentId: string) => {
      startTransition(async () => {
        const result = await fetchPortalDocumentDownload(documentId);
        if (result.success && result.data?.web_view_link) {
          window.open(result.data.web_view_link, "_blank", "noopener,noreferrer");
        }
      });
    },
    [startTransition],
  );

  // Download document
  const handleDownloadDocument = useCallback(
    (documentId: string) => {
      startTransition(async () => {
        const result = await fetchPortalDocumentDownload(documentId);
        if (result.success && result.data?.web_view_link) {
          const downloadLink = result.data.web_view_link.replace(
            /\/view(\?|$)/,
            "/export$1",
          );
          window.open(downloadLink, "_blank", "noopener,noreferrer");
        }
      });
    },
    [startTransition],
  );

  // Drag and drop handlers
  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragOver(false);
      const files = e.dataTransfer.files;
      if (files.length > 0 && uploadCategoryId) {
        handleUploadFile(files[0]);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [uploadCategoryId],
  );

  const handleUploadClick = useCallback(() => {
    if (uploadCategoryId) {
      fileInputRef.current?.click();
    }
  }, [uploadCategoryId]);

  const handleFileInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (files && files.length > 0) {
        handleUploadFile(files[0]);
      }
      // Reset input so same file can be re-selected
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [uploadCategoryId],
  );

  const handleUploadFile = useCallback(
    (file: File) => {
      if (!uploadCategoryId) return;
      setUploadStatus("loading");
      setUploadMessage(t.documents_uploading);

      startTransition(async () => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("category_id", uploadCategoryId);

        const result = await uploadPortalDocumentAction(formData);
        if (result.success) {
          setUploadStatus("success");
          setUploadMessage(t.documents_upload_success);
          // Refresh documents
          loadDocuments(
            selectedCategoryId || undefined,
            selectedYear ? Number(selectedYear) : undefined,
            searchQuery || undefined,
          );
        } else {
          setUploadStatus("failed");
          setUploadMessage(result.message || t.documents_upload_error);
        }
      });
    },
    [uploadCategoryId, t, startTransition, loadDocuments, selectedCategoryId, selectedYear, searchQuery],
  );

  // Filtered documents (local search)
  const filteredDocuments = useMemo(() => {
    if (!searchQuery) return documents;
    const query = searchQuery.toLowerCase();
    return documents.filter((doc) =>
      doc.file_name.toLowerCase().includes(query),
    );
  }, [documents, searchQuery]);

  // Group documents by category
  const documentsByCategory = useMemo(() => {
    const groups: Record<string, { name: string; docs: PortalDocumentDto[] }> = {};
    for (const doc of filteredDocuments) {
      const key = doc.category_id;
      if (!groups[key]) {
        groups[key] = {
          name: doc.category_name || t.documents_all_categories,
          docs: [],
        };
      }
      groups[key].docs.push(doc);
    }
    return groups;
  }, [filteredDocuments, t]);

  return (
    <div className="space-y-6">
      {/* Page title */}
      <div>
        <h1 className="text-xl font-semibold text-foreground">
          {t.portal_nav_documents}
        </h1>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder={t.documents_search_placeholder}
            value={searchQuery}
            onChange={(e) => handleSearch(e.target.value)}
            className="h-9 pl-9 text-sm"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={selectedCategoryId}
            onChange={(e) => handleCategoryChange(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm text-foreground"
            aria-label={t.documents_select_category}
          >
            <option value="">{t.documents_all_categories}</option>
            {categories.map((cat) => (
              <option key={cat.category_id} value={cat.category_id}>
                {cat.name}
              </option>
            ))}
          </select>
          <select
            value={selectedYear}
            onChange={(e) => handleYearChange(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm text-foreground"
            aria-label={t.documents_date}
          >
            <option value="">{t.portal_documents_all_years}</option>
            {yearOptions.map((year) => (
              <option key={year} value={String(year)}>
                {year}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Upload section */}
      <Card>
        <CardContent className="p-4 space-y-3">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <Upload className="h-4 w-4" />
              {t.documents_upload}
            </div>
            <select
              value={uploadCategoryId}
              onChange={(e) => setUploadCategoryId(e.target.value)}
              className="h-9 rounded-md border border-input bg-background px-3 text-sm text-foreground sm:flex-1 sm:max-w-[240px]"
              aria-label={t.documents_select_category}
            >
              <option value="">{t.documents_select_category}</option>
              {categories.map((cat) => (
                <option key={cat.category_id} value={cat.category_id}>
                  {cat.name}
                </option>
              ))}
            </select>
            <Button
              variant="outline"
              size="sm"
              onClick={handleUploadClick}
              disabled={!uploadCategoryId || isPending}
            >
              <Upload className="mr-1.5 h-3.5 w-3.5" />
              {t.documents_upload}
            </Button>
            <input
              ref={fileInputRef}
              type="file"
              className="hidden"
              onChange={handleFileInputChange}
              aria-label={t.documents_upload}
            />
          </div>

          {/* Drop zone */}
          <div
            role="region"
            aria-label={t.documents_drop_zone}
            className={`rounded-lg border-2 border-dashed p-6 text-center transition-colors ${
              !uploadCategoryId
                ? "border-muted-foreground/15 text-muted-foreground/50"
                : isDragOver
                  ? "border-blue-500 bg-blue-50 text-blue-600"
                  : "border-muted-foreground/25 text-muted-foreground cursor-pointer"
            }`}
            onDragOver={uploadCategoryId ? handleDragOver : undefined}
            onDragLeave={uploadCategoryId ? handleDragLeave : undefined}
            onDrop={uploadCategoryId ? handleDrop : undefined}
            onClick={uploadCategoryId ? handleUploadClick : undefined}
          >
            <Upload className="mx-auto mb-2 h-6 w-6" />
            <p className="text-sm">
              {!uploadCategoryId
                ? t.documents_select_category
                : isDragOver
                  ? t.documents_drop_zone_active
                  : t.documents_drop_zone}
            </p>
          </div>

          {/* Upload status */}
          {uploadStatus !== "idle" && (
            <p
              className={`text-sm ${
                uploadStatus === "success"
                  ? "text-green-600"
                  : uploadStatus === "failed"
                    ? "text-red-600"
                    : "text-muted-foreground"
              }`}
            >
              {uploadMessage}
            </p>
          )}
        </CardContent>
      </Card>

      {/* Document list */}
      {isPending || status === "loading" ? (
        <div className="flex items-center justify-center py-12">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-muted-foreground border-t-transparent" />
        </div>
      ) : filteredDocuments.length === 0 ? (
        <EmptyState
          icon={FileText}
          title={t.documents_empty_title}
          message={
            searchQuery || selectedCategoryId || selectedYear
              ? t.documents_no_results
              : t.documents_empty_message
          }
        />
      ) : (
        <div className="space-y-4">
          {Object.entries(documentsByCategory).map(([categoryId, group]) => (
            <div key={categoryId}>
              <div className="mb-2 flex items-center gap-2">
                <Filter className="h-4 w-4 text-muted-foreground" />
                <h2 className="text-sm font-semibold text-foreground">
                  {group.name}
                </h2>
                <Badge variant="secondary" className="text-xs">
                  {group.docs.length}
                </Badge>
              </div>

              {/* Desktop table */}
              <div className="hidden md:block">
                <Card>
                  <CardContent className="p-0">
                    <table className="w-full text-sm" role="table">
                      <thead>
                        <tr className="border-b text-left text-muted-foreground">
                          <th className="px-4 py-2.5 font-medium">{t.documents_name}</th>
                          <th className="px-4 py-2.5 font-medium">{t.documents_size}</th>
                          <th className="px-4 py-2.5 font-medium">{t.documents_date}</th>
                          <th className="px-4 py-2.5 font-medium">{t.documents_version}</th>
                          <th className="px-4 py-2.5 font-medium" />
                        </tr>
                      </thead>
                      <tbody>
                        {group.docs.map((doc) => (
                          <tr
                            key={doc.document_id}
                            className="border-b last:border-0 hover:bg-accent/50"
                          >
                            <td className="px-4 py-2.5">
                              <div className="flex items-center gap-2">
                                <FileIcon mimeType={doc.mime_type} />
                                <span className="font-medium text-foreground truncate max-w-[250px]">
                                  {doc.file_name}
                                </span>
                              </div>
                            </td>
                            <td className="px-4 py-2.5 text-muted-foreground">
                              {formatFileSize(doc.file_size)}
                            </td>
                            <td className="px-4 py-2.5 text-muted-foreground">
                              {formatDate(doc.created_at, locale)}
                            </td>
                            <td className="px-4 py-2.5">
                              <Badge
                                variant="secondary"
                                className="bg-blue-100 text-blue-700"
                              >
                                v{doc.current_version}
                              </Badge>
                            </td>
                            <td className="px-4 py-2.5">
                              <div className="flex items-center gap-1">
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  className="h-7 w-7 p-0"
                                  onClick={() => handleViewDocument(doc.document_id)}
                                  title={t.documents_view}
                                >
                                  <Eye className="h-3.5 w-3.5" />
                                </Button>
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  className="h-7 w-7 p-0"
                                  onClick={() => handleDownloadDocument(doc.document_id)}
                                  title={t.documents_download}
                                >
                                  <Download className="h-3.5 w-3.5" />
                                </Button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </CardContent>
                </Card>
              </div>

              {/* Mobile cards */}
              <div className="md:hidden space-y-2">
                {group.docs.map((doc) => (
                  <Card key={doc.document_id}>
                    <CardContent className="p-3">
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex items-center gap-2 min-w-0">
                          <FileIcon mimeType={doc.mime_type} />
                          <div className="min-w-0">
                            <p className="text-sm font-medium text-foreground truncate">
                              {doc.file_name}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              {formatFileSize(doc.file_size)} ·{" "}
                              {formatDate(doc.created_at, locale)}
                            </p>
                          </div>
                        </div>
                        <Badge
                          variant="secondary"
                          className="bg-blue-100 text-blue-700 shrink-0"
                        >
                          v{doc.current_version}
                        </Badge>
                      </div>
                      <div className="mt-2 flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          className="h-7 text-xs flex-1"
                          onClick={() => handleViewDocument(doc.document_id)}
                        >
                          <Eye className="mr-1 h-3 w-3" />
                          {t.documents_view}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          className="h-7 text-xs flex-1"
                          onClick={() => handleDownloadDocument(doc.document_id)}
                        >
                          <Download className="mr-1 h-3 w-3" />
                          {t.documents_download}
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
