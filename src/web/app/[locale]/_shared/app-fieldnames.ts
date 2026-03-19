export const fieldnames = {
  "en-pr": {
    app_name: "ITDG CRM Platform",
    nav_dashboard: "Dashboard",
    nav_clients: "Clients",
    nav_documents: "Documents",
    nav_communications: "Communications",
    nav_settings: "Settings",
    required_error: "This field is required",
    email_invalid_error: "Invalid email address",
  },
  "es-pr": {
    app_name: "ITDG CRM Plataforma",
    nav_dashboard: "Tablero",
    nav_clients: "Clientes",
    nav_documents: "Documentos",
    nav_communications: "Comunicaciones",
    nav_settings: "Configuración",
    required_error: "Este campo es requerido",
    email_invalid_error: "Correo electrónico inválido",
  },
} as const;

export type Locale = keyof typeof fieldnames;
export type FieldNames = (typeof fieldnames)[Locale];
