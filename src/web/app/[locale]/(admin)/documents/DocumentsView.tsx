"use client";

import { useState, useCallback, useMemo, useTransition, useRef } from "react";
import { useLocale } from "next-intl";
import {
  FileText,
  FileImage,
  FileSpreadsheet,
  File,
  FolderOpen,
  ChevronRight,
  ChevronDown,
  Upload,
  Download,
  Eye,
  FolderTree,
  Files,
  Search,
} from "lucide-react";
import { PageHeader } from "@/app/_components/PageHeader";
import { EmptyState } from "@/app/_components/EmptyState";
import { Button } from "@/app/_components/ui/button";
import { Card, CardContent } from "@/app/_components/ui/card";
import { Badge } from "@/app/_components/ui/badge";
import { Input } from "@/app/_components/ui/input";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type DocumentDto,
  type PaginatedDocuments,
  type ClientDto,
  type DocumentCategoryDto,
  formatFileSize,
  formatDate,
  getMimeTypeIcon,
  getYearOptions,
} from "./shared";
import {
  fetchClientDocuments,
  fetchDocumentDownload,
} from "./actions";
import DocumentSearchPanel from "./DocumentSearchPanel";

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

interface TreeNode {
  id: string;
  label: string;
  type: "client" | "year" | "category";
  clientId?: string;
  categoryId?: string;
  year?: number;
  children?: TreeNode[];
}

function buildTree(
  clients: ClientDto[],
  categories: DocumentCategoryDto[],
): TreeNode[] {
  const years = getYearOptions();
  return clients.map((client) => ({
    id: `client-${client.client_id}`,
    label: client.name,
    type: "client" as const,
    clientId: client.client_id,
    children: years.map((year) => ({
      id: `year-${client.client_id}-${year}`,
      label: String(year),
      type: "year" as const,
      clientId: client.client_id,
      year,
      children: categories.map((cat) => ({
        id: `cat-${client.client_id}-${year}-${cat.category_id}`,
        label: cat.name,
        type: "category" as const,
        clientId: client.client_id,
        categoryId: cat.category_id,
        year,
      })),
    })),
  }));
}

interface TreeItemProps {
  node: TreeNode;
  selectedId: string;
  expandedIds: Set<string>;
  onToggle: (id: string) => void;
  onSelect: (node: TreeNode) => void;
  level: number;
}

function TreeItem({
  node,
  selectedId,
  expandedIds,
  onToggle,
  onSelect,
  level,
}: TreeItemProps) {
  const isExpanded = expandedIds.has(node.id);
  const isSelected = selectedId === node.id;
  const hasChildren = node.children && node.children.length > 0;

  return (
    <div>
      <button
        type="button"
        className={`flex w-full items-center gap-1.5 rounded-md px-2 py-1.5 text-sm transition-colors hover:bg-accent ${
          isSelected ? "bg-blue-50 text-blue-700 font-medium" : "text-foreground"
        }`}
        style={{ paddingLeft: `${level * 16 + 8}px` }}
        onClick={() => {
          if (hasChildren) onToggle(node.id);
          onSelect(node);
        }}
      >
        {hasChildren ? (
          isExpanded ? (
            <ChevronDown className="h-3.5 w-3.5 shrink-0" />
          ) : (
            <ChevronRight className="h-3.5 w-3.5 shrink-0" />
          )
        ) : (
          <span className="w-3.5" />
        )}
        {node.type === "client" && (
          <FolderOpen className="h-4 w-4 shrink-0 text-muted-foreground" />
        )}
        <span className="truncate">{node.label}</span>
      </button>
      {isExpanded && hasChildren && (
        <div>
          {node.children!.map((child) => (
            <TreeItem
              key={child.id}
              node={child}
              selectedId={selectedId}
              expandedIds={expandedIds}
              onToggle={onToggle}
              onSelect={onSelect}
              level={level + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface DocumentsViewProps {
  initialClients: ClientDto[];
  initialCategories: DocumentCategoryDto[];
}

export default function DocumentsView({
  initialClients,
  initialCategories,
}: DocumentsViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const [isPending, startTransition] = useTransition();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // State
  const [clients] = useState(initialClients);
  const [categories] = useState(initialCategories);
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [selectedNodeId, setSelectedNodeId] = useState("");
  const [selectedClientId, setSelectedClientId] = useState("");
  const [selectedCategoryId, setSelectedCategoryId] = useState("");
  const [selectedYear, setSelectedYear] = useState<number | undefined>();
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [searchQuery, setSearchQuery] = useState("");
  const [isDragOver, setIsDragOver] = useState(false);
  const [mobileTab, setMobileTab] = useState<"explorer" | "files">("explorer");
  const [showSearch, setShowSearch] = useState(false);

  // Build tree
  const tree = useMemo(() => buildTree(clients, categories), [clients, categories]);

  // Load documents for selected client/category/year
  const loadDocuments = useCallback(
    (clientId: string, categoryId?: string, year?: number) => {
      setStatus("loading");
      startTransition(async () => {
        const result = await fetchClientDocuments({
          clientId,
          categoryId,
          year,
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

  // Tree interactions
  const handleToggle = useCallback((id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const handleSelect = useCallback(
    (node: TreeNode) => {
      setSelectedNodeId(node.id);
      if (node.clientId) setSelectedClientId(node.clientId);
      if (node.categoryId) {
        setSelectedCategoryId(node.categoryId);
      } else {
        setSelectedCategoryId("");
      }
      if (node.year) {
        setSelectedYear(node.year);
      } else {
        setSelectedYear(undefined);
      }

      // On mobile, switch to files tab when a node is selected
      setMobileTab("files");

      // Load documents for any node that has a clientId
      if (node.clientId) {
        loadDocuments(node.clientId, node.categoryId, node.year);
      }
    },
    [loadDocuments],
  );

  // Filtered documents
  const filteredDocuments = useMemo(() => {
    if (!searchQuery) return documents;
    const query = searchQuery.toLowerCase();
    return documents.filter((doc) =>
      doc.file_name.toLowerCase().includes(query),
    );
  }, [documents, searchQuery]);

  // View document (open in browser)
  const handleViewDocument = useCallback(
    (documentId: string) => {
      startTransition(async () => {
        const result = await fetchDocumentDownload(documentId);
        if (result.success && result.data?.web_view_link) {
          window.open(result.data.web_view_link, "_blank", "noopener,noreferrer");
        }
      });
    },
    [startTransition],
  );

  // Download document (same link, signals download intent)
  const handleDownloadDocument = useCallback(
    (documentId: string) => {
      startTransition(async () => {
        const result = await fetchDocumentDownload(documentId);
        if (result.success && result.data?.web_view_link) {
          // Google Drive export link triggers download
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

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    // Upload requires multipart POST to /api/v1/Clients/{client_id}/Documents
    // with category_id. Full upload integration depends on category selection
    // and the backend upload endpoint (issue #41).
    const files = e.dataTransfer.files;
    if (files.length > 0 && fileInputRef.current) {
      fileInputRef.current.files = files;
    }
  }, []);

  const handleUploadClick = useCallback(() => {
    fileInputRef.current?.click();
  }, []);

  // Explorer panel content
  const explorerContent = (
    <div className="flex h-full flex-col">
      <div className="border-b p-3">
        <h2 className="text-sm font-semibold text-foreground">
          {t.documents_explorer}
        </h2>
      </div>
      <div className="flex-1 overflow-y-auto p-2">
        {clients.length === 0 ? (
          <p className="p-3 text-sm text-muted-foreground">
            {t.documents_select_client}
          </p>
        ) : (
          tree.map((node) => (
            <TreeItem
              key={node.id}
              node={node}
              selectedId={selectedNodeId}
              expandedIds={expandedIds}
              onToggle={handleToggle}
              onSelect={handleSelect}
              level={0}
            />
          ))
        )}
      </div>
    </div>
  );

  // Files panel content
  const filesContent = (
    <div className="flex h-full flex-col">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center gap-2 border-b p-3">
        <div className="flex-1 min-w-[200px]">
          <Input
            placeholder={t.documents_search_placeholder}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="h-8 text-sm"
          />
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={handleUploadClick}
          disabled={!selectedClientId}
        >
          <Upload className="mr-1.5 h-3.5 w-3.5" />
          {t.documents_upload}
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={filteredDocuments.length === 0}
        >
          <Download className="mr-1.5 h-3.5 w-3.5" />
          {t.documents_download_all}
        </Button>
        <input
          ref={fileInputRef}
          type="file"
          className="hidden"
          multiple
          aria-label={t.documents_upload}
        />
      </div>

      {/* Drop zone */}
      {selectedClientId && (
        <div
          role="region"
          aria-label={t.documents_drop_zone}
          className={`mx-3 mt-3 rounded-lg border-2 border-dashed p-4 text-center transition-colors ${
            isDragOver
              ? "border-blue-500 bg-blue-50 text-blue-600"
              : "border-muted-foreground/25 text-muted-foreground"
          }`}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
        >
          <Upload className="mx-auto mb-1.5 h-5 w-5" />
          <p className="text-xs">
            {isDragOver ? t.documents_drop_zone_active : t.documents_drop_zone}
          </p>
        </div>
      )}

      {/* Document list */}
      <div className="flex-1 overflow-y-auto p-3">
        {!selectedClientId ? (
          <EmptyState
            icon={FolderTree}
            title={t.documents_select_client}
            message={t.documents_empty_message}
          />
        ) : isPending || status === "loading" ? (
          <div className="flex items-center justify-center py-12">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-muted-foreground border-t-transparent" />
          </div>
        ) : filteredDocuments.length === 0 ? (
          <EmptyState
            icon={Files}
            title={t.documents_empty_title}
            message={
              searchQuery ? t.documents_no_results : t.documents_empty_message
            }
          />
        ) : (
          <>
            {/* Desktop table */}
            <div className="hidden md:block">
              <table className="w-full text-sm" role="table">
                <thead>
                  <tr className="border-b text-left text-muted-foreground">
                    <th className="pb-2 pr-3 font-medium">{t.documents_name}</th>
                    <th className="pb-2 pr-3 font-medium">{t.documents_size}</th>
                    <th className="pb-2 pr-3 font-medium">{t.documents_date}</th>
                    <th className="pb-2 pr-3 font-medium">
                      {t.documents_uploaded_by}
                    </th>
                    <th className="pb-2 pr-3 font-medium">
                      {t.documents_version}
                    </th>
                    <th className="pb-2 font-medium" />
                  </tr>
                </thead>
                <tbody>
                  {filteredDocuments.map((doc) => (
                    <tr
                      key={doc.document_id}
                      className="border-b last:border-0 hover:bg-accent/50"
                    >
                      <td className="py-2.5 pr-3">
                        <div className="flex items-center gap-2">
                          <FileIcon mimeType={doc.mime_type} />
                          <span className="font-medium text-foreground truncate max-w-[200px]">
                            {doc.file_name}
                          </span>
                        </div>
                      </td>
                      <td className="py-2.5 pr-3 text-muted-foreground">
                        {formatFileSize(doc.file_size)}
                      </td>
                      <td className="py-2.5 pr-3 text-muted-foreground">
                        {formatDate(doc.created_at, locale)}
                      </td>
                      <td className="py-2.5 pr-3 text-muted-foreground font-mono text-xs">
                        {doc.uploaded_by_id.substring(0, 8)}…
                      </td>
                      <td className="py-2.5 pr-3">
                        <Badge
                          variant="secondary"
                          className="bg-blue-100 text-blue-700"
                        >
                          v{doc.current_version}
                        </Badge>
                      </td>
                      <td className="py-2.5">
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
            </div>

            {/* Mobile cards */}
            <div className="md:hidden space-y-2">
              {filteredDocuments.map((doc) => (
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
          </>
        )}
      </div>
    </div>
  );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <PageHeader
          title={t.documents_title}
          breadcrumbs={[
            { label: t.nav_dashboard, href: "/dashboard" },
            { label: t.documents_title },
          ]}
        />
        <Button
          variant={showSearch ? "default" : "outline"}
          size="sm"
          onClick={() => setShowSearch((prev) => !prev)}
        >
          <Search className="mr-1.5 h-3.5 w-3.5" />
          {t.documents_search_title}
        </Button>
      </div>

      {/* Search panel */}
      {showSearch && (
        <div className="h-[calc(100vh-200px)]">
          <DocumentSearchPanel
            clients={clients}
            categories={categories}
            onClose={() => setShowSearch(false)}
          />
        </div>
      )}

      {/* Main content (hidden when search is open) */}
      {!showSearch && (
        <>
          {/* Mobile tabs */}
          <div className="md:hidden flex rounded-lg border overflow-hidden">
            <button
              type="button"
              className={`flex-1 py-2 text-sm font-medium transition-colors ${
                mobileTab === "explorer"
                  ? "bg-slate-800 text-white"
                  : "bg-background text-foreground"
              }`}
              onClick={() => setMobileTab("explorer")}
            >
              <FolderTree className="inline-block mr-1.5 h-3.5 w-3.5" />
              {t.documents_explorer}
            </button>
            <button
              type="button"
              className={`flex-1 py-2 text-sm font-medium transition-colors ${
                mobileTab === "files"
                  ? "bg-slate-800 text-white"
                  : "bg-background text-foreground"
              }`}
              onClick={() => setMobileTab("files")}
            >
              <Files className="inline-block mr-1.5 h-3.5 w-3.5" />
              {t.documents_files}
            </button>
          </div>

          {/* Desktop layout: side-by-side */}
          <div className="hidden md:grid md:grid-cols-[280px_1fr] gap-4">
            <Card className="h-[calc(100vh-200px)] overflow-hidden">
              <CardContent className="p-0 h-full">{explorerContent}</CardContent>
            </Card>
            <Card className="h-[calc(100vh-200px)] overflow-hidden">
              <CardContent className="p-0 h-full">{filesContent}</CardContent>
            </Card>
          </div>

          {/* Mobile layout: tabbed */}
          <div className="md:hidden">
            {mobileTab === "explorer" ? (
              <Card className="h-[calc(100vh-240px)] overflow-hidden">
                <CardContent className="p-0 h-full">{explorerContent}</CardContent>
              </Card>
            ) : (
              <Card className="h-[calc(100vh-240px)] overflow-hidden">
                <CardContent className="p-0 h-full">{filesContent}</CardContent>
              </Card>
            )}
          </div>
        </>
      )}
    </div>
  );
}
