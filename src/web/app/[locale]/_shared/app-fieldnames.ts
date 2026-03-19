export const fieldnames = {
  "en-pr": {
    app_name: "ITDG CRM Platform",
    nav_dashboard: "Dashboard",
    nav_clients: "Clients",
    nav_documents: "Documents",
    nav_communications: "Communications",
    nav_settings: "Settings",
    portal_name: "Client Portal",
    portal_nav_messages: "Messages",
    portal_nav_documents: "Documents",
    portal_nav_payments: "Payments",
    portal_menu_open: "Open menu",
    portal_menu_close: "Close menu",
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
    portal_name: "Portal del Cliente",
    portal_nav_messages: "Mensajes",
    portal_nav_documents: "Documentos",
    portal_nav_payments: "Pagos",
    portal_menu_open: "Abrir menú",
    portal_menu_close: "Cerrar menú",
    required_error: "Este campo es requerido",
    email_invalid_error: "Correo electrónico inválido",
  },
} as const;

export type Locale = keyof typeof fieldnames;
export type FieldNames = (typeof fieldnames)[Locale];
