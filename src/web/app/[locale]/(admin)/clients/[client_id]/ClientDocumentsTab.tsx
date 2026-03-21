"use client";

import { useState, useCallback, useTransition, useRef, useEffect } from "react";
import { useLocale } from "next-intl";
import {
  FileText,
  FileImage,
  FileSpreadsheet,
  File,
  Eye,
  Download,
  Upload,
  Clock,
  ChevronRight,
  Trash2,
  X,
} from "lucide-react";
import { Button } from "@/app/_components/ui/button";
import { Card, CardContent } from "@/app/_components/ui/card";
import { Badge } from "@/app/_components/ui/badge";
import { Input } from "@/app/_components/ui/input";
import { Separator } from "@/app/_components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/app/_components/ui/dialog";
import { EmptyState } from "@/app/_components/EmptyState";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import {
  formatFileSize,
  formatDate,
} from "@/app/[locale]/(admin)/documents/shared";
import type { DocumentDto } from "@/server/Services/documentService";
import type { DocumentDetailDto } from "@/server/Services/documentService";
import {
  fetchClientDocumentsAction,
  fetchDocumentDetailAction,
  uploadNewVersionAction,
  deleteDocumentAction,
} from "./actions";

function getMimeTypeIcon(mimeType: string) {
  if (mimeType.includes("pdf")) return <FileText className="h-5 w-5 text-red-500" />;
  if (mimeType.includes("image")) return <FileImage className="h-5 w-5 text-blue-500" />;
  if (mimeType.includes("spreadsheet") || mimeType.includes("excel"))
    return <FileSpreadsheet className="h-5 w-5 text-green-500" />;
  if (mimeType.includes("document") || mimeType.includes("word") || mimeType.includes("msword"))
    return <FileText className="h-5 w-5 text-blue-600" />;
  return <File className="h-5 w-5 text-muted-foreground" />;
}

interface ClientDocumentsTabProps {
  clientId: string;
}

export default function ClientDocumentsTab({ clientId }: ClientDocumentsTabProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, startTransition] = useTransition();
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDocument, setSelectedDocument] = useState<DocumentDetailDto | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [isDetailLoading, setIsDetailLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [statusMessage, setStatusMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [hasLoaded, setHasLoaded] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadDocuments = useCallback(
    (search?: string) => {
      startTransition(async () => {
        const result = await fetchClientDocumentsAction({
          clientId,
          page: 1,
          pageSize: 50,
          search: search || undefined,
        });
        if (result.success && result.data) {
          setDocuments(result.data.items);
          setTotalCount(result.data.total_count);
        }
        setHasLoaded(true);
      });
    },
    [clientId],
  );

  // Load documents on mount
  useEffect(() => {
    loadDocuments();
  }, [loadDocuments]);

  const handleSearch = useCallback(
    (value: string) => {
      setSearchQuery(value);
      loadDocuments(value);
    },
    [loadDocuments],
  );

  const handleDocumentClick = useCallback(async (documentId: string) => {
    setIsDetailLoading(true);
    setIsDetailOpen(true);
    const result = await fetchDocumentDetailAction(documentId);
    if (result.success && result.data) {
      setSelectedDocument(result.data);
    }
    setIsDetailLoading(false);
  }, []);

  const handleUploadNewVersion = useCallback(
    async (file: globalThis.File) => {
      if (!selectedDocument) return;
      setIsUploading(true);
      setStatusMessage(null);
      const formData = new FormData();
      formData.append("file", file);
      const result = await uploadNewVersionAction(selectedDocument.document_id, formData);
      if (result.success) {
        setStatusMessage({ type: "success", text: t.documents_upload_version_success });
        // Refresh document detail
        const detailResult = await fetchDocumentDetailAction(selectedDocument.document_id);
        if (detailResult.success && detailResult.data) {
          setSelectedDocument(detailResult.data);
        }
        loadDocuments(searchQuery);
      } else {
        setStatusMessage({ type: "error", text: t.documents_upload_version_error });
      }
      setIsUploading(false);
    },
    [selectedDocument, searchQuery, loadDocuments, t],
  );

  const handleDeleteDocument = useCallback(
    async (documentId: string) => {
      setStatusMessage(null);
      const result = await deleteDocumentAction(documentId);
      if (result.success) {
        setStatusMessage({ type: "success", text: t.documents_delete_success });
        setIsDetailOpen(false);
        setSelectedDocument(null);
        loadDocuments(searchQuery);
      } else {
        setStatusMessage({ type: "error", text: t.documents_delete_error });
      }
    },
    [searchQuery, loadDocuments, t],
  );

  const handleViewDocument = useCallback((googleDriveFileId: string) => {
    window.open(`https://drive.google.com/file/d/${googleDriveFileId}/view`, "_blank");
  }, []);

  const handleDownloadDocument = useCallback((googleDriveFileId: string) => {
    window.open(`https://drive.google.com/uc?export=download&id=${googleDriveFileId}`, "_blank");
  }, []);

  return (
    <div className="space-y-4">
      {/* Status message */}
      {statusMessage && !isDetailOpen && (
        <div
          className={`flex items-center justify-between rounded-md px-4 py-3 text-sm ${
            statusMessage.type === "success"
              ? "bg-green-50 text-green-800 dark:bg-green-950 dark:text-green-200"
              : "bg-red-50 text-red-800 dark:bg-red-950 dark:text-red-200"
          }`}
        >
          <span>{statusMessage.text}</span>
          <button
            onClick={() => setStatusMessage(null)}
            className="ml-2 rounded-sm opacity-70 hover:opacity-100"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      )}

      {/* Search bar */}
      <div className="flex items-center gap-2">
        <Input
          placeholder={t.documents_search_placeholder}
          value={searchQuery}
          onChange={(e) => handleSearch(e.target.value)}
          className="max-w-sm"
        />
        {totalCount > 0 && (
          <span className="text-sm text-muted-foreground">
            {totalCount} {totalCount === 1 ? "document" : "documents"}
          </span>
        )}
      </div>

      {/* Document list */}
      {!hasLoaded || isLoading ? (
        <div className="flex items-center justify-center py-12">
          <div className="text-sm text-muted-foreground">{t.documents_uploading.replace("…", "")}</div>
        </div>
      ) : documents.length === 0 ? (
        <EmptyState
          icon={FileText}
          title={t.documents_empty_title}
          message={t.documents_empty_message}
        />
      ) : (
        <div className="space-y-2">
          {/* Desktop table view */}
          <div className="hidden md:block">
            <div className="rounded-md border">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="px-4 py-3 text-left font-medium">{t.documents_name}</th>
                    <th className="px-4 py-3 text-left font-medium">{t.documents_size}</th>
                    <th className="px-4 py-3 text-left font-medium">{t.documents_date}</th>
                    <th className="px-4 py-3 text-left font-medium">{t.documents_version}</th>
                    <th className="px-4 py-3 text-right font-medium">&nbsp;</th>
                  </tr>
                </thead>
                <tbody>
                  {documents.map((doc) => (
                    <tr
                      key={doc.document_id}
                      className="border-b last:border-0 hover:bg-muted/30 cursor-pointer"
                      onClick={() => handleDocumentClick(doc.document_id)}
                    >
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          {getMimeTypeIcon(doc.mime_type)}
                          <span className="truncate max-w-[200px]">{doc.file_name}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 font-mono text-muted-foreground">
                        {formatFileSize(doc.file_size)}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDate(doc.created_at, locale)}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant="secondary">v{doc.current_version}</Badge>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <ChevronRight className="h-4 w-4 text-muted-foreground inline-block" />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Mobile card view */}
          <div className="md:hidden space-y-2">
            {documents.map((doc) => (
              <Card
                key={doc.document_id}
                className="cursor-pointer hover:bg-muted/30"
                onClick={() => handleDocumentClick(doc.document_id)}
              >
                <CardContent className="p-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 min-w-0">
                      {getMimeTypeIcon(doc.mime_type)}
                      <div className="min-w-0">
                        <p className="text-sm font-medium truncate">{doc.file_name}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatFileSize(doc.file_size)} · {formatDate(doc.created_at, locale)}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="secondary">v{doc.current_version}</Badge>
                      <ChevronRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Document Detail Dialog */}
      <Dialog open={isDetailOpen} onOpenChange={setIsDetailOpen}>
        <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{t.documents_detail_title}</DialogTitle>
            <DialogDescription className="sr-only">
              {t.documents_detail_title}
            </DialogDescription>
          </DialogHeader>

          {isDetailLoading ? (
            <div className="flex items-center justify-center py-8">
              <div className="text-sm text-muted-foreground">{t.documents_uploading.replace("…", "")}</div>
            </div>
          ) : selectedDocument ? (
            <div className="space-y-4">
              {/* Status message inside dialog */}
              {statusMessage && (
                <div
                  className={`flex items-center justify-between rounded-md px-3 py-2 text-sm ${
                    statusMessage.type === "success"
                      ? "bg-green-50 text-green-800 dark:bg-green-950 dark:text-green-200"
                      : "bg-red-50 text-red-800 dark:bg-red-950 dark:text-red-200"
                  }`}
                >
                  <span>{statusMessage.text}</span>
                  <button
                    onClick={() => setStatusMessage(null)}
                    className="ml-2 rounded-sm opacity-70 hover:opacity-100"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </div>
              )}

              {/* Document metadata */}
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  {getMimeTypeIcon(selectedDocument.mime_type)}
                  <h3 className="text-sm font-medium truncate">{selectedDocument.file_name}</h3>
                </div>

                <div className="grid grid-cols-2 gap-3 text-sm">
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_detail_category}</p>
                    <p>{selectedDocument.category_name ?? "—"}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_size}</p>
                    <p className="font-mono">{formatFileSize(selectedDocument.file_size)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_detail_type}</p>
                    <p>{selectedDocument.mime_type}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_version}</p>
                    <p>v{selectedDocument.current_version}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_detail_created}</p>
                    <p>{formatDate(selectedDocument.created_at, locale)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">{t.documents_detail_updated}</p>
                    <p>{formatDate(selectedDocument.updated_at, locale)}</p>
                  </div>
                </div>
              </div>

              {/* Action buttons */}
              <div className="flex flex-wrap gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => handleViewDocument(selectedDocument.google_drive_file_id)}
                >
                  <Eye className="mr-1.5 h-4 w-4" />
                  {t.documents_view}
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => handleDownloadDocument(selectedDocument.google_drive_file_id)}
                >
                  <Download className="mr-1.5 h-4 w-4" />
                  {t.documents_download}
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploading}
                >
                  <Upload className="mr-1.5 h-4 w-4" />
                  {isUploading ? t.documents_uploading : t.documents_upload_new_version}
                </Button>
                <Button
                  size="sm"
                  variant="destructive"
                  onClick={() => handleDeleteDocument(selectedDocument.document_id)}
                >
                  <Trash2 className="mr-1.5 h-4 w-4" />
                  {t.documents_delete}
                </Button>
                <input
                  ref={fileInputRef}
                  type="file"
                  className="hidden"
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (file) {
                      handleUploadNewVersion(file);
                      e.target.value = "";
                    }
                  }}
                />
              </div>

              <Separator />

              {/* Version history */}
              <div className="space-y-3">
                <h4 className="text-sm font-medium flex items-center gap-1.5">
                  <Clock className="h-4 w-4" />
                  {t.documents_version_history}
                </h4>

                {selectedDocument.versions.length === 0 ? (
                  <p className="text-sm text-muted-foreground">{t.documents_no_versions}</p>
                ) : (
                  <div className="space-y-2">
                    {selectedDocument.versions.map((version) => (
                      <div
                        key={version.version_id}
                        className="flex items-center justify-between rounded-md border p-3 text-sm"
                      >
                        <div className="space-y-0.5">
                          <div className="flex items-center gap-2">
                            <Badge variant={version.version_number === selectedDocument.current_version ? "default" : "secondary"}>
                              v{version.version_number}
                            </Badge>
                            {version.version_number === selectedDocument.current_version && (
                              <span className="text-xs text-muted-foreground">(latest)</span>
                            )}
                          </div>
                          <p className="text-xs text-muted-foreground">
                            {formatDate(version.uploaded_at, locale)}
                          </p>
                        </div>
                        <div className="flex items-center gap-1">
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleViewDocument(version.google_drive_file_id);
                            }}
                            title={t.documents_view}
                          >
                            <Eye className="h-3.5 w-3.5" />
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDownloadDocument(version.google_drive_file_id);
                            }}
                            title={t.documents_download}
                          >
                            <Download className="h-3.5 w-3.5" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>
    </div>
  );
}
