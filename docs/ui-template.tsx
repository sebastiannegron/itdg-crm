import { useState, useEffect } from "react";

const OR = "#E85320";
const NV = "#1a2744";
const BL = "#0088C8";

function getBP() {
  if (typeof window === "undefined") return "de";
  if (window.innerWidth < 640) return "mo";
  if (window.innerWidth < 1024) return "ta";
  return "de";
}

function useBreakpoint() {
  const [bp, setBp] = useState(getBP);
  useEffect(() => {
    const handler = () => setBp(getBP());
    window.addEventListener("resize", handler);
    return () => window.removeEventListener("resize", handler);
  }, []);
  return bp;
}

const CLIENTS = [
  { id:1, name:"Rodriguez & Associates LLC", contact:"Carlos Rodriguez", email:"carlos@rodassoc.com", phone:"787-555-0101", tier:1, associate:"Maria Santos", status:"Active", lastActivity:"Mar 17", industry:"Legal" },
  { id:2, name:"Familia Pérez", contact:"Ana Pérez", email:"aperez@gmail.com", phone:"787-555-0102", tier:2, associate:"José Rivera", status:"Active", lastActivity:"Mar 15", industry:"Individual" },
  { id:3, name:"TechSolutions PR, Inc.", contact:"David Torres", email:"dtorres@techsolpr.com", phone:"787-555-0103", tier:1, associate:"Maria Santos", status:"Active", lastActivity:"Mar 16", industry:"Technology" },
  { id:4, name:"Restaurante El Bohío", contact:"Carmen Díaz", email:"carmen@elbohio.com", phone:"787-555-0104", tier:2, associate:"Luis Morales", status:"Pending Docs", lastActivity:"Mar 10", industry:"F&B" },
  { id:5, name:"Inversiones Caribe Corp.", contact:"Miguel Vega", email:"mvega@invcaribe.com", phone:"787-555-0105", tier:1, associate:"Maria Santos", status:"Active", lastActivity:"Mar 17", industry:"Finance" },
  { id:6, name:"Familia González", contact:"Roberto González", email:"rgonzalez@yahoo.com", phone:"787-555-0106", tier:3, associate:"José Rivera", status:"Active", lastActivity:"Mar 8", industry:"Individual" },
  { id:7, name:"Constructora Boricua LLC", contact:"Patricia Medina", email:"pmedina@constboricua.com", phone:"787-555-0107", tier:2, associate:"Luis Morales", status:"Active", lastActivity:"Mar 14", industry:"Construction" },
  { id:8, name:"Farmacia San Jorge", contact:"Héctor Ortiz", email:"hortiz@fsanjorge.com", phone:"787-555-0108", tier:3, associate:"José Rivera", status:"Awaiting Payment", lastActivity:"Mar 5", industry:"Healthcare" },
];

const TASKS = [
  { id:1, title:"File 2025 Corporate Tax Return", client:"TechSolutions PR", assignee:"Maria Santos", initials:"MS", due:"Mar 31", priority:"High", status:"In Progress" },
  { id:2, title:"Prepare Q4 2025 Quarterly Report", client:"Inversiones Caribe", assignee:"Maria Santos", initials:"MS", due:"Mar 20", priority:"High", status:"Review" },
  { id:3, title:"Send annual document request", client:"Familia Pérez", assignee:"José Rivera", initials:"JR", due:"Mar 19", priority:"Medium", status:"To Do" },
  { id:4, title:"Reconcile payroll filings", client:"Restaurante El Bohío", assignee:"Luis Morales", initials:"LM", due:"Mar 25", priority:"Medium", status:"In Progress" },
  { id:5, title:"Review personal deductions", client:"Familia González", assignee:"José Rivera", initials:"JR", due:"Mar 28", priority:"Low", status:"To Do" },
  { id:6, title:"W-2 filing review", client:"Constructora Boricua", assignee:"Luis Morales", initials:"LM", due:"Mar 18", priority:"High", status:"Done" },
  { id:7, title:"480.6B filing", client:"Rodriguez & Assoc.", assignee:"Maria Santos", initials:"MS", due:"Mar 22", priority:"High", status:"To Do" },
  { id:8, title:"Annual review meeting prep", client:"Farmacia San Jorge", assignee:"José Rivera", initials:"JR", due:"Apr 5", priority:"Low", status:"To Do" },
];

const NOTIFS_INIT = [
  { id:1, type:"doc",     msg:"Ana Pérez uploaded 3 documents",              time:"5 min ago",  read:false },
  { id:2, type:"alert",   msg:"Task overdue — Q4 Report, Inversiones Caribe", time:"22 min ago", read:false },
  { id:3, type:"payment", msg:"Payment failed — Farmacia San Jorge ($850.00)", time:"1 hr ago",   read:false },
  { id:4, type:"task",    msg:"Task assigned: 480.6B — Rodriguez",             time:"2 hr ago",   read:true  },
  { id:5, type:"msg",     msg:"New portal message from Carlos Rodriguez",       time:"3 hr ago",   read:true  },
  { id:6, type:"doc",     msg:"David Torres uploaded Bank Statement Feb 2026",  time:"Yesterday",  read:true  },
];

const EMAILS = [
  { id:1, client:"Rodriguez & Associates LLC", subject:"Re: 2025 Tax Filing Documents", date:"Mar 17, 2:34 PM",
    thread:[
      { from:"Maria Santos",   date:"Mar 16, 10:00 AM", body:"Dear Mr. Rodriguez, please provide W-2 forms, 1099s, and business expense receipts. Submission deadline: March 28.", team:true  },
      { from:"Carlos Rodriguez", date:"Mar 17, 2:34 PM",  body:"Thank you! I'll have everything ready by Friday. Should I upload through the portal?", team:false },
    ]},
  { id:2, client:"Familia Pérez", subject:"Documentos para radicación 2025", date:"Mar 15, 9:12 AM",
    thread:[
      { from:"José Rivera", date:"Mar 10, 11:00 AM", body:"Estimada Sra. Pérez, le enviamos la lista de documentos para su radicación 2025. Por favor sométalos antes del 28 de marzo.", team:true  },
      { from:"Ana Pérez",   date:"Mar 15, 9:12 AM",  body:"Buenas tardes, adjunto los documentos solicitados. Por favor confirmen recibo. Gracias.", team:false },
    ]},
  { id:3, client:"TechSolutions PR, Inc.", subject:"Q4 Corporate Return — Pending Items", date:"Mar 16, 4:45 PM",
    thread:[
      { from:"David Torres", date:"Mar 16, 4:45 PM", body:"Hi Maria, we still need to discuss the Q4 equipment purchases. Can we schedule a call this week?", team:false },
    ]},
];

const DOC_TREE = {
  "Rodriguez & Associates LLC": { "2025":["Bank Statements","Tax Documents","Invoices","Contracts"], "2024":["Bank Statements","Tax Documents"] },
  "Familia Pérez":              { "2025":["Tax Documents","Bank Statements","General"],              "2024":["Tax Documents"] },
  "TechSolutions PR, Inc.":     { "2025":["Tax Documents","Invoices","Reports","Contracts"],         "2024":["Tax Documents","Reports"] },
};

const DOC_FILES = {
  "Bank Statements": [
    { name:"BankStatement_Jan2026.pdf", size:"1.2 MB", uploaded:"Mar 15", by:"Carlos Rodriguez", version:"v1" },
    { name:"BankStatement_Feb2026.pdf", size:"1.1 MB", uploaded:"Mar 15", by:"Carlos Rodriguez", version:"v1" },
  ],
  "Tax Documents": [
    { name:"W2_2025_Rodriguez.pdf", size:"456 KB", uploaded:"Mar 16", by:"Maria Santos", version:"v2" },
    { name:"480.6B_Draft.docx",     size:"234 KB", uploaded:"Mar 17", by:"Maria Santos", version:"v1" },
  ],
  "Invoices":   [{ name:"Invoice_Q4_2025.pdf",         size:"567 KB", uploaded:"Mar 10", by:"Maria Santos", version:"v1" }],
  "Contracts":  [{ name:"ServiceAgreement_2026.pdf",   size:"890 KB", uploaded:"Jan 5",  by:"Maria Santos", version:"v3" }],
  "Reports":    [{ name:"AnnualReport_2024.pdf",        size:"1.8 MB", uploaded:"Feb 12", by:"Maria Santos", version:"v1" }],
  "General":    [{ name:"Notes_Perez_2025.docx",        size:"102 KB", uploaded:"Mar 10", by:"José Rivera",  version:"v1" }],
};

const TEMPLATES = [
  { id:1, name:"Annual Doc Request — Individual", cat:"Tax Season" },
  { id:2, name:"Annual Doc Request — Corporate",  cat:"Tax Season" },
  { id:3, name:"Payment Reminder",                cat:"Payment"   },
  { id:4, name:"Welcome / Onboarding",            cat:"Onboarding"},
  { id:5, name:"Tax Filing Confirmation",         cat:"Tax Season" },
  { id:6, name:"Document Receipt Confirmation",   cat:"Documents" },
];

const KANBAN_COLS = ["To Do","In Progress","Review","Done"];
const COL_DOT = { "To Do":"#6B7280", "In Progress":BL, "Review":"#7C3AED", "Done":"#059669" };
const NAV_ITEMS = [
  { id:"dashboard",      label:"Dashboard", icon:"⊞" },
  { id:"clients",        label:"Clients",   icon:"👥" },
  { id:"documents",      label:"Documents", icon:"📄" },
  { id:"communications", label:"Comms",     icon:"✉"  },
  { id:"tasks",          label:"Tasks",     icon:"✓"  },
];

// ── Small shared components ─────────────────────────────────────────
function TierBadge({ t }) {
  const map = {
    1: { color:"#92400E", bg:"#FEF3C7", border:"#FCD34D" },
    2: { color:"#1E40AF", bg:"#DBEAFE", border:"#93C5FD" },
    3: { color:"#374151", bg:"#F3F4F6", border:"#D1D5DB" },
  };
  const s = map[t] || map[3];
  return (
    <span style={{ fontSize:10, padding:"2px 8px", borderRadius:20, background:s.bg, color:s.color, border:`1px solid ${s.border}`, fontWeight:600 }}>
      Tier {t}
    </span>
  );
}

function StatusBadge({ s }) {
  const map = {
    "Active":           { c:"#065F46", bg:"#ECFDF5" },
    "Pending Docs":     { c:"#92400E", bg:"#FFFBEB" },
    "Awaiting Payment": { c:"#991B1B", bg:"#FEF2F2" },
    "To Do":            { c:"#374151", bg:"#F9FAFB" },
    "In Progress":      { c:"#1E40AF", bg:"#EFF6FF" },
    "Review":           { c:"#6D28D9", bg:"#F5F3FF" },
    "Done":             { c:"#065F46", bg:"#ECFDF5" },
  };
  const st = map[s] || map["To Do"];
  return (
    <span style={{ fontSize:10, padding:"2px 8px", borderRadius:20, background:st.bg, color:st.c, fontWeight:600 }}>
      {s}
    </span>
  );
}

function NotifDot({ type }) {
  const colors = { doc:BL, alert:"#EF4444", payment:"#EF4444", task:"#059669", msg:"#7C3AED" };
  return <div style={{ width:8, height:8, borderRadius:"50%", background:colors[type] || "#9CA3AF", flexShrink:0, marginTop:3 }} />;
}

// ── Main App ────────────────────────────────────────────────────────
export default function CRMApp() {
  const bp    = useBreakpoint();
  const isMo  = bp === "mo";
  const isDe  = bp === "de";

  const [nav,       setNav]       = useState("dashboard");
  const [role,      setRole]      = useState("Administrator");
  const [showN,     setShowN]     = useState(false);
  const [notifs,    setNotifs]    = useState(NOTIFS_INIT);
  const [selClient, setSelClient] = useState(null);
  const [selEmail,  setSelEmail]  = useState(EMAILS[0]);
  const [commView,  setCommView]  = useState("list");   // "list" | "thread" | "compose"
  const [selTmpl,   setSelTmpl]   = useState(null);
  const [aiDraft,   setAiDraft]   = useState("");
  const [aiLoad,    setAiLoad]    = useState(false);
  const [cBody,     setCBody]     = useState("");
  const [expCl,     setExpCl]     = useState("Rodriguez & Associates LLC");
  const [expYr,     setExpYr]     = useState("2025");
  const [selCat,    setSelCat]    = useState(null);
  const [docPane,   setDocPane]   = useState("tree");   // "tree" | "files"  (mobile only)
  const [search,    setSearch]    = useState("");
  const [tierF,     setTierF]     = useState("All");

  const unread = notifs.filter(n => !n.read).length;

  const filteredClients = CLIENTS.filter(c => {
    const q = search.toLowerCase();
    const matchQ = c.name.toLowerCase().includes(q) || c.contact.toLowerCase().includes(q);
    const matchT = tierF === "All" || c.tier === Number(tierF);
    return matchQ && matchT;
  });

  function goNav(id) {
    setNav(id);
    setShowN(false);
    if (isMo) setSelClient(null);
  }

  function markAllRead() {
    setNotifs(prev => prev.map(n => ({ ...n, read:true })));
  }

  function triggerAI() {
    setAiLoad(true);
    setAiDraft("");
    setTimeout(() => {
      setAiDraft(
        "Dear Mr. Torres,\n\nThank you for reaching out. I'd be happy to discuss the Q4 equipment purchases.\n\n" +
        "I'm available Thursday March 19 between 10 AM–12 PM AST, or Friday March 20 after 2 PM.\n\n" +
        "Best regards,\nMaria Santos\nRaposo & Associates"
      );
      setAiLoad(false);
    }, 1800);
  }

  // ── Layout shell ──────────────────────────────────────────────────
  return (
    <div style={{ fontFamily:"system-ui,sans-serif", display:"flex", flexDirection:isMo ? "column" : "row", height:720, overflow:"hidden", background:"#F1F5F9", borderRadius:isMo ? 8 : 12, border:"1px solid #E2E8F0" }}>

      {/* ── Sidebar (tablet + desktop) ── */}
      {!isMo && (
        <div style={{ width:isDe ? 210 : 52, background:NV, display:"flex", flexDirection:"column", flexShrink:0, transition:"width 0.2s" }}>
          {/* Logo */}
          <div style={{ padding:isDe ? "18px 16px 14px" : "14px 0", borderBottom:"1px solid rgba(255,255,255,0.08)", textAlign:isDe ? "left" : "center" }}>
            <div style={{ color:OR, fontWeight:800, fontSize:17 }}>R&A</div>
            {isDe && <div style={{ color:"rgba(255,255,255,0.3)", fontSize:9, marginTop:1, textTransform:"uppercase", letterSpacing:1.2 }}>CRM Platform · MVP</div>}
          </div>
          {/* User card (desktop only) */}
          {isDe && (
            <div style={{ padding:"10px 10px 4px" }}>
              <div onClick={() => setRole(r => r === "Administrator" ? "Associate" : "Administrator")}
                style={{ background:"rgba(255,255,255,0.07)", borderRadius:8, padding:"8px 10px", display:"flex", alignItems:"center", gap:8, cursor:"pointer" }}>
                <div style={{ width:28, height:28, borderRadius:"50%", background:OR, display:"flex", alignItems:"center", justifyContent:"center", color:"white", fontSize:11, fontWeight:700, flexShrink:0 }}>
                  {role === "Administrator" ? "MR" : "MS"}
                </div>
                <div style={{ minWidth:0 }}>
                  <div style={{ color:"white", fontSize:11, fontWeight:600, whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>
                    {role === "Administrator" ? "Manuel Raposo" : "Maria Santos"}
                  </div>
                  <div style={{ color:OR, fontSize:9, fontWeight:700, textTransform:"uppercase" }}>{role}</div>
                </div>
              </div>
              <div style={{ color:"rgba(255,255,255,0.18)", fontSize:9, textAlign:"center", marginTop:3 }}>tap to switch role</div>
            </div>
          )}
          {/* Nav */}
          <nav style={{ flex:1, padding:"6px 0" }}>
            {NAV_ITEMS.map(({ id, label, icon }) => {
              const active = nav === id;
              if (isDe) {
                return (
                  <div key={id} onClick={() => goNav(id)}
                    style={{ display:"flex", alignItems:"center", gap:9, padding:"9px 14px", cursor:"pointer", margin:"1px 8px", background:active ? "rgba(232,83,32,0.18)" : "transparent", borderLeft:active ? `3px solid ${OR}` : "3px solid transparent", borderRadius:"0 6px 6px 0" }}>
                    <span style={{ fontSize:14, color:active ? OR : "rgba(255,255,255,0.35)" }}>{icon}</span>
                    <span style={{ color:active ? "white" : "rgba(255,255,255,0.4)", fontSize:12, fontWeight:active ? 600 : 400 }}>{label}</span>
                  </div>
                );
              }
              return (
                <div key={id} onClick={() => goNav(id)}
                  style={{ display:"flex", justifyContent:"center", alignItems:"center", height:46, cursor:"pointer", background:active ? "rgba(232,83,32,0.18)" : "transparent", borderLeft:active ? `3px solid ${OR}` : "3px solid transparent" }}>
                  <span style={{ fontSize:18, color:active ? OR : "rgba(255,255,255,0.35)" }}>{icon}</span>
                </div>
              );
            })}
          </nav>
        </div>
      )}

      {/* ── Main area ── */}
      <div style={{ flex:1, display:"flex", flexDirection:"column", overflow:"hidden", minWidth:0 }}>

        {/* Header */}
        <div style={{ background:"white", borderBottom:"1px solid #E5E7EB", padding:"0 14px", height:50, display:"flex", alignItems:"center", justifyContent:"space-between", flexShrink:0, gap:8 }}>
          <div style={{ display:"flex", alignItems:"center", gap:8, flex:1, minWidth:0 }}>
            {isMo && selClient && nav === "clients" && (
              <button onClick={() => setSelClient(null)} style={{ background:"none", border:"none", cursor:"pointer", color:"#64748B", fontSize:18, padding:"0 4px", flexShrink:0 }}>←</button>
            )}
            {isMo && commView !== "list" && nav === "communications" && (
              <button onClick={() => { setCommView("list"); }} style={{ background:"none", border:"none", cursor:"pointer", color:"#64748B", fontSize:18, padding:"0 4px", flexShrink:0 }}>←</button>
            )}
            <span style={{ fontSize:14, fontWeight:700, color:NV, whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>
              {isMo && selClient && nav === "clients"
                ? selClient.name
                : isMo && commView === "thread" && nav === "communications"
                  ? selEmail?.client
                  : isMo && commView === "compose"
                    ? "New Email"
                    : NAV_ITEMS.find(n => n.id === nav)?.label}
            </span>
            {!isMo && <span style={{ fontSize:11, color:"#94A3B8", whiteSpace:"nowrap" }}>Raposo &amp; Associates · 2025</span>}
          </div>
          {/* Right header actions */}
          <div style={{ display:"flex", alignItems:"center", gap:8, flexShrink:0 }}>
            {isMo && (
              <span onClick={() => setRole(r => r === "Administrator" ? "Associate" : "Administrator")}
                style={{ fontSize:9, fontWeight:700, color:OR, cursor:"pointer", background:"#FFF8F5", padding:"3px 8px", borderRadius:10, border:`1px solid ${OR}`, whiteSpace:"nowrap" }}>
                {role === "Administrator" ? "Admin" : "Assoc."}
              </span>
            )}
            {/* Bell */}
            <div style={{ position:"relative" }}>
              <div onClick={() => setShowN(v => !v)}
                style={{ width:34, height:34, borderRadius:8, background:"#F1F5F9", display:"flex", alignItems:"center", justifyContent:"center", cursor:"pointer", position:"relative" }}>
                <span style={{ fontSize:16 }}>🔔</span>
                {unread > 0 && (
                  <div style={{ position:"absolute", top:-3, right:-3, width:16, height:16, borderRadius:"50%", background:OR, display:"flex", alignItems:"center", justifyContent:"center" }}>
                    <span style={{ color:"white", fontSize:8, fontWeight:800 }}>{unread}</span>
                  </div>
                )}
              </div>
              {showN && (
                <div style={{ position:"absolute", top:42, right:0, width:Math.min(300, (typeof window !== "undefined" ? window.innerWidth : 400) - 24), background:"white", borderRadius:10, boxShadow:"0 8px 32px rgba(0,0,0,0.15)", border:"1px solid #E5E7EB", zIndex:999 }}>
                  <div style={{ padding:"11px 14px", borderBottom:"1px solid #F1F5F9", display:"flex", justifyContent:"space-between", alignItems:"center" }}>
                    <span style={{ fontWeight:700, fontSize:13, color:NV }}>Notifications</span>
                    <span onClick={markAllRead} style={{ fontSize:11, color:OR, cursor:"pointer" }}>Mark all read</span>
                  </div>
                  <div style={{ maxHeight:260, overflowY:"auto" }}>
                    {notifs.map(n => (
                      <div key={n.id} style={{ padding:"10px 14px", borderBottom:"1px solid #F9FAFB", background:n.read ? "white" : "#FFF8F5", display:"flex", gap:10, alignItems:"flex-start" }}>
                        <NotifDot type={n.type} />
                        <div style={{ flex:1 }}>
                          <div style={{ fontSize:11, color:"#374151", fontWeight:n.read ? 400 : 600, lineHeight:1.5 }}>{n.msg}</div>
                          <div style={{ fontSize:10, color:"#94A3B8", marginTop:2 }}>{n.time}</div>
                        </div>
                        {!n.read && <div style={{ width:7, height:7, borderRadius:"50%", background:OR, flexShrink:0, marginTop:4 }} />}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
            <div style={{ width:30, height:30, borderRadius:"50%", background:NV, display:"flex", alignItems:"center", justifyContent:"center", color:"white", fontSize:10, fontWeight:700, flexShrink:0 }}>
              {role === "Administrator" ? "MR" : "MS"}
            </div>
          </div>
        </div>

        {/* Page content */}
        <div style={{ flex:1, overflowY:"auto", padding:isMo ? 10 : 16 }} onClick={() => showN && setShowN(false)}>

          {/* ── DASHBOARD ── */}
          {nav === "dashboard" && <Dashboard isMo={isMo} isDe={isDe} setNav={setNav} />}

          {/* ── CLIENTS ── */}
          {nav === "clients" && (
            <ClientsScreen
              isMo={isMo} isDe={isDe} role={role}
              selClient={selClient} setSelClient={setSelClient}
              filteredClients={filteredClients}
              search={search} setSearch={setSearch}
              tierF={tierF} setTierF={setTierF}
              setNav={setNav}
              setSelEmail={setSelEmail}
              setCommView={setCommView}
            />
          )}

          {/* ── DOCUMENTS ── */}
          {nav === "documents" && (
            <DocsScreen
              isMo={isMo}
              docPane={docPane} setDocPane={setDocPane}
              expCl={expCl} setExpCl={setExpCl}
              expYr={expYr} setExpYr={setExpYr}
              selCat={selCat} setSelCat={setSelCat}
            />
          )}

          {/* ── COMMUNICATIONS ── */}
          {nav === "communications" && (
            <CommsScreen
              isMo={isMo}
              commView={commView} setCommView={setCommView}
              selEmail={selEmail} setSelEmail={setSelEmail}
              selTmpl={selTmpl} setSelTmpl={setSelTmpl}
              aiDraft={aiDraft} setAiDraft={setAiDraft}
              aiLoad={aiLoad}
              cBody={cBody} setCBody={setCBody}
              triggerAI={triggerAI}
            />
          )}

          {/* ── TASKS ── */}
          {nav === "tasks" && <TasksScreen isMo={isMo} role={role} />}

        </div>
      </div>

      {/* ── Bottom nav (mobile) ── */}
      {isMo && (
        <div style={{ height:56, background:NV, display:"flex", alignItems:"stretch", flexShrink:0 }}>
          {NAV_ITEMS.map(({ id, label, icon }) => {
            const active = nav === id;
            return (
              <div key={id} onClick={() => goNav(id)}
                style={{ flex:1, display:"flex", flexDirection:"column", alignItems:"center", justifyContent:"center", gap:2, cursor:"pointer", background:active ? "rgba(232,83,32,0.18)" : "transparent", borderTop:active ? `2px solid ${OR}` : "2px solid transparent" }}>
                <span style={{ fontSize:16, color:active ? OR : "rgba(255,255,255,0.4)" }}>{icon}</span>
                <span style={{ fontSize:9, color:active ? OR : "rgba(255,255,255,0.35)", fontWeight:active ? 700 : 400 }}>{label}</span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

// ── DASHBOARD screen ─────────────────────────────────────────────────
function Dashboard({ isMo, isDe, setNav }) {
  const stats = [
    { label:"Total Clients", val:CLIENTS.length,                                    sub:"3 T1 · 3 T2 · 2 T3",    color:BL  },
    { label:"Open Tasks",    val:TASKS.filter(t => t.status !== "Done").length,     sub:"2 high priority due soon", color:OR  },
    { label:"Docs Today",    val:5,                                                  sub:"From 3 clients via portal", color:"#059669" },
    { label:"Escalated",     val:2,                                                  sub:"Requires immediate action", color:"#DC2626" },
  ];
  return (
    <div>
      <div style={{ display:"grid", gridTemplateColumns:`repeat(${isMo ? 2 : 4},minmax(0,1fr))`, gap:10, marginBottom:14 }}>
        {stats.map(c => (
          <div key={c.label} style={{ background:"white", borderRadius:10, padding:isMo ? 10 : 14, border:"1px solid #E5E7EB" }}>
            <div style={{ fontSize:isMo ? 10 : 11, color:"#94A3B8", fontWeight:600, marginBottom:4 }}>{c.label}</div>
            <div style={{ fontSize:isMo ? 24 : 28, fontWeight:800, color:NV, lineHeight:1 }}>{c.val}</div>
            <div style={{ fontSize:10, color:"#94A3B8", marginTop:5 }}>{c.sub}</div>
            <div style={{ width:28, height:3, borderRadius:2, background:c.color, marginTop:8 }} />
          </div>
        ))}
      </div>

      <div style={{ display:"grid", gridTemplateColumns:isDe ? "minmax(0,1fr) 280px" : "1fr", gap:12, marginBottom:12 }}>
        {/* Pending tasks */}
        <div style={{ background:"white", borderRadius:10, padding:14, border:"1px solid #E5E7EB" }}>
          <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center", marginBottom:11 }}>
            <span style={{ fontWeight:700, fontSize:13, color:NV }}>Pending Tasks</span>
            <span onClick={() => setNav("tasks")} style={{ fontSize:11, color:OR, cursor:"pointer" }}>View all →</span>
          </div>
          {TASKS.filter(t => t.status !== "Done").slice(0, 5).map(t => (
            <div key={t.id} style={{ display:"flex", alignItems:"center", gap:10, padding:"9px 10px", background:"#F8FAFC", borderRadius:7, marginBottom:7, border:"1px solid #F1F5F9" }}>
              <div style={{ width:8, height:8, borderRadius:"50%", background:t.priority === "High" ? "#EF4444" : t.priority === "Medium" ? "#F59E0B" : "#9CA3AF", flexShrink:0 }} />
              <div style={{ flex:1, minWidth:0 }}>
                <div style={{ fontSize:12, fontWeight:600, color:"#1E293B", whiteSpace:"nowrap", overflow:"hidden", textOverflow:"ellipsis" }}>{t.title}</div>
                <div style={{ fontSize:10, color:"#94A3B8", marginTop:1 }}>{t.client}</div>
              </div>
              <div style={{ textAlign:"right", flexShrink:0 }}>
                <StatusBadge s={t.status} />
                <div style={{ fontSize:10, color:"#94A3B8", marginTop:2 }}>Due {t.due}</div>
              </div>
            </div>
          ))}
        </div>
        {/* Right column */}
        <div style={{ display:"flex", flexDirection:"column", gap:11 }}>
          <div style={{ background:"white", borderRadius:10, padding:14, border:"1px solid #E5E7EB" }}>
            <div style={{ fontWeight:700, fontSize:12, color:NV, marginBottom:9 }}>⚠ Escalated Issues</div>
            {[["Q4 Report overdue — Inversiones Caribe","22 min ago"],["Payment failed — Farmacia San Jorge ($850)","1 hr ago"]].map(([m, t], i) => (
              <div key={i} style={{ padding:"9px 10px", background:"#FEF2F2", borderRadius:7, border:"1px solid #FECACA", marginBottom:6 }}>
                <div style={{ fontSize:11, fontWeight:600, color:"#991B1B" }}>{m}</div>
                <div style={{ fontSize:10, color:"#B91C1C", marginTop:2 }}>{t}</div>
              </div>
            ))}
          </div>
          <div style={{ background:"white", borderRadius:10, padding:14, border:"1px solid #E5E7EB" }}>
            <div style={{ fontWeight:700, fontSize:12, color:NV, marginBottom:9 }}>📅 Upcoming Deadlines</div>
            {[["Mar 19","Doc request — Familia Pérez"],["Mar 20","Call — TechSolutions PR"],["Mar 22","480.6B — Rodriguez"],["Mar 28","All personal filings"]].map(([d, l], i) => (
              <div key={i} style={{ display:"flex", gap:8, alignItems:"center", marginBottom:7 }}>
                <div style={{ background:"#FFF8F5", border:`1px solid ${OR}`, borderRadius:5, padding:"2px 6px", fontSize:9, fontWeight:700, color:OR, flexShrink:0, whiteSpace:"nowrap" }}>{d}</div>
                <span style={{ fontSize:11, color:"#374151", overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{l}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

// ── CLIENTS screen ───────────────────────────────────────────────────
function ClientsScreen({ isMo, isDe, role, selClient, setSelClient, filteredClients, search, setSearch, tierF, setTierF, setNav, setSelEmail, setCommView }) {
  if (isMo && selClient) {
    return (
      <div style={{ background:"white", borderRadius:10, border:"1px solid #E5E7EB", padding:16 }}>
        <div style={{ display:"flex", justifyContent:"space-between", alignItems:"flex-start", marginBottom:14 }}>
          <div>
            <div style={{ fontWeight:700, fontSize:14, color:NV, lineHeight:1.4 }}>{selClient.name}</div>
            <div style={{ marginTop:7 }}><TierBadge t={selClient.tier} /></div>
          </div>
          <StatusBadge s={selClient.status} />
        </div>
        {[["Contact",selClient.contact],["Email",selClient.email],["Phone",selClient.phone],["Industry",selClient.industry],["Associate",selClient.associate],["Last Activity",selClient.lastActivity]].map(([l, v]) => (
          <div key={l} style={{ display:"flex", justifyContent:"space-between", padding:"9px 0", borderBottom:"1px solid #F1F5F9" }}>
            <span style={{ fontSize:11, color:"#94A3B8", fontWeight:600 }}>{l}</span>
            <span style={{ fontSize:12, color:"#374151", fontWeight:500, textAlign:"right", maxWidth:"60%", overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{v}</span>
          </div>
        ))}
        <div style={{ marginTop:14, display:"flex", flexDirection:"column", gap:8 }}>
          <button onClick={() => { setSelEmail(EMAILS.find(e => e.client === selClient.name) || EMAILS[0]); setCommView("thread"); setNav("communications"); }}
            style={{ width:"100%", padding:10, borderRadius:8, background:"#F8FAFC", border:"1px solid #E5E7EB", fontSize:12, fontWeight:600, color:"#374151", cursor:"pointer" }}>✉ View Emails</button>
          <button onClick={() => setNav("documents")}
            style={{ width:"100%", padding:10, borderRadius:8, background:"#F8FAFC", border:"1px solid #E5E7EB", fontSize:12, fontWeight:600, color:"#374151", cursor:"pointer" }}>📄 View Documents</button>
          <button onClick={() => setNav("tasks")}
            style={{ width:"100%", padding:10, borderRadius:8, background:OR, border:"none", fontSize:12, fontWeight:600, color:"white", cursor:"pointer" }}>✓ View Tasks</button>
        </div>
      </div>
    );
  }

  return (
    <div style={{ display:"flex", gap:14 }}>
      <div style={{ flex:1, minWidth:0 }}>
        <div style={{ display:"flex", gap:8, marginBottom:12, flexWrap:"wrap" }}>
          <div style={{ flex:1, minWidth:isMo ? "100%" : 200, position:"relative" }}>
            <span style={{ position:"absolute", left:10, top:"50%", transform:"translateY(-50%)", color:"#94A3B8", fontSize:14 }}>🔍</span>
            <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Search clients..."
              style={{ width:"100%", padding:"8px 10px 8px 32px", borderRadius:8, border:"1px solid #E5E7EB", fontSize:12, outline:"none", boxSizing:"border-box", background:"white" }} />
          </div>
          <select value={tierF} onChange={e => setTierF(e.target.value)}
            style={{ padding:"8px 10px", borderRadius:8, border:"1px solid #E5E7EB", fontSize:12, background:"white", outline:"none", color:"#374151" }}>
            <option value="All">All Tiers</option>
            <option value="1">Tier 1</option>
            <option value="2">Tier 2</option>
            <option value="3">Tier 3</option>
          </select>
          {role === "Administrator" && (
            <button style={{ padding:"8px 14px", borderRadius:8, background:OR, color:"white", border:"none", fontSize:12, fontWeight:600, cursor:"pointer", flexShrink:0 }}>+ New</button>
          )}
        </div>

        {isMo ? (
          <div style={{ display:"flex", flexDirection:"column", gap:9 }}>
            {filteredClients.map(c => (
              <div key={c.id} onClick={() => setSelClient(c)}
                style={{ background:"white", borderRadius:10, padding:13, border:"1px solid #E5E7EB", cursor:"pointer" }}>
                <div style={{ display:"flex", justifyContent:"space-between", alignItems:"flex-start", marginBottom:6 }}>
                  <div style={{ fontWeight:700, fontSize:13, color:NV, flex:1, marginRight:8 }}>{c.name}</div>
                  <TierBadge t={c.tier} />
                </div>
                <div style={{ fontSize:11, color:"#94A3B8", marginBottom:9 }}>{c.contact} · {c.industry}</div>
                <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center" }}>
                  <StatusBadge s={c.status} />
                  <span style={{ fontSize:10, color:"#94A3B8" }}>{c.associate}</span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div style={{ background:"white", borderRadius:10, border:"1px solid #E5E7EB", overflow:"hidden" }}>
            <table style={{ width:"100%", borderCollapse:"collapse", tableLayout:"fixed" }}>
              <thead>
                <tr style={{ background:"#F8FAFC", borderBottom:"1px solid #E5E7EB" }}>
                  {["Client / Contact","Tier","Associate","Status","Last Activity",""].map((h, i) => (
                    <th key={i} style={{ padding:"9px 12px", textAlign:"left", fontSize:10, fontWeight:700, color:"#64748B", textTransform:"uppercase", width:i===5?"40px":i===1||i===4?"80px":i===2?"110px":i===3?"100px":"auto" }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filteredClients.map(c => (
                  <tr key={c.id} onClick={() => setSelClient(selClient?.id === c.id ? null : c)}
                    style={{ borderBottom:"1px solid #F1F5F9", cursor:"pointer", background:selClient?.id === c.id ? "#FFF8F5" : "white" }}>
                    <td style={{ padding:"11px 12px" }}>
                      <div style={{ fontWeight:600, fontSize:12, color:NV, overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{c.name}</div>
                      <div style={{ fontSize:10, color:"#94A3B8", marginTop:1 }}>{c.contact}</div>
                    </td>
                    <td style={{ padding:"11px 12px" }}><TierBadge t={c.tier} /></td>
                    <td style={{ padding:"11px 12px", fontSize:11, color:"#374151", overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{c.associate}</td>
                    <td style={{ padding:"11px 12px" }}><StatusBadge s={c.status} /></td>
                    <td style={{ padding:"11px 12px", fontSize:11, color:"#94A3B8" }}>{c.lastActivity}</td>
                    <td style={{ padding:"11px 12px", fontSize:14, color:"#94A3B8", textAlign:"center" }}>›</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {!isMo && selClient && (
        <div style={{ width:260, background:"white", borderRadius:10, border:"1px solid #E5E7EB", padding:16, flexShrink:0, height:"fit-content" }}>
          <div style={{ display:"flex", justifyContent:"space-between", alignItems:"flex-start", marginBottom:13 }}>
            <div>
              <div style={{ fontWeight:700, fontSize:13, color:NV, lineHeight:1.4 }}>{selClient.name}</div>
              <div style={{ marginTop:7 }}><TierBadge t={selClient.tier} /></div>
            </div>
            <button onClick={() => setSelClient(null)} style={{ background:"none", border:"none", cursor:"pointer", color:"#94A3B8", fontSize:18 }}>✕</button>
          </div>
          {[["Contact",selClient.contact],["Email",selClient.email],["Phone",selClient.phone],["Industry",selClient.industry],["Associate",selClient.associate]].map(([l, v]) => (
            <div key={l} style={{ display:"flex", justifyContent:"space-between", padding:"7px 0", borderBottom:"1px solid #F1F5F9" }}>
              <span style={{ fontSize:10, color:"#94A3B8", fontWeight:600 }}>{l}</span>
              <span style={{ fontSize:11, color:"#374151", fontWeight:500, maxWidth:160, overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{v}</span>
            </div>
          ))}
          <div style={{ marginTop:13, display:"flex", flexDirection:"column", gap:7 }}>
            <button onClick={() => { setNav("communications"); setSelEmail(EMAILS.find(e => e.client === selClient.name) || EMAILS[0]); }}
              style={{ width:"100%", padding:8, borderRadius:7, background:"#F8FAFC", border:"1px solid #E5E7EB", fontSize:11, fontWeight:600, color:"#374151", cursor:"pointer" }}>✉ View Emails</button>
            <button onClick={() => setNav("documents")}
              style={{ width:"100%", padding:8, borderRadius:7, background:"#F8FAFC", border:"1px solid #E5E7EB", fontSize:11, fontWeight:600, color:"#374151", cursor:"pointer" }}>📄 Documents</button>
            <button onClick={() => setNav("tasks")}
              style={{ width:"100%", padding:8, borderRadius:7, background:OR, border:"none", fontSize:11, fontWeight:600, color:"white", cursor:"pointer" }}>✓ Tasks</button>
          </div>
        </div>
      )}
    </div>
  );
}

// ── DOCUMENTS screen ─────────────────────────────────────────────────
function DocsScreen({ isMo, docPane, setDocPane, expCl, setExpCl, expYr, setExpYr, selCat, setSelCat }) {
  return (
    <div>
      {isMo && (
        <div style={{ display:"flex", background:"white", borderRadius:8, border:"1px solid #E5E7EB", marginBottom:12, overflow:"hidden" }}>
          {["tree","files"].map(v => (
            <button key={v} onClick={() => setDocPane(v)}
              style={{ flex:1, padding:"9px", border:"none", background:docPane === v ? NV : "transparent", color:docPane === v ? "white" : "#64748B", fontSize:12, fontWeight:docPane === v ? 700 : 400, cursor:"pointer" }}>
              {v === "tree" ? "📁 Explorer" : "📄 Files"}
            </button>
          ))}
        </div>
      )}

      <div style={{ display:"flex", gap:12, height:isMo ? undefined : 580 }}>
        {(!isMo || docPane === "tree") && (
          <div style={{ width:isMo ? "100%" : 210, background:"white", borderRadius:10, border:"1px solid #E5E7EB", padding:13, flexShrink:0, overflowY:"auto", maxHeight:isMo ? 340 : undefined }}>
            <div style={{ fontWeight:700, fontSize:12, color:NV, marginBottom:11 }}>File Explorer</div>
            {Object.entries(DOC_TREE).map(([cl, yrs]) => (
              <div key={cl} style={{ marginBottom:2 }}>
                <div onClick={() => { setExpCl(expCl === cl ? null : cl); setSelCat(null); }}
                  style={{ display:"flex", alignItems:"center", gap:6, padding:"6px 7px", borderRadius:6, cursor:"pointer", background:expCl === cl ? "#FFF8F5" : "transparent" }}>
                  <span style={{ fontSize:10, color:"#94A3B8", display:"inline-block", transform:expCl === cl ? "rotate(90deg)" : "none", transition:"0.15s" }}>▶</span>
                  <span style={{ fontSize:10, fontWeight:600, color:expCl === cl ? OR : "#374151", lineHeight:1.4 }}>{cl.length > 24 ? cl.slice(0,24)+"…" : cl}</span>
                </div>
                {expCl === cl && Object.entries(yrs).map(([yr, cats]) => (
                  <div key={yr} style={{ marginLeft:14 }}>
                    <div onClick={() => setExpYr(expYr === yr ? null : yr)}
                      style={{ display:"flex", alignItems:"center", gap:6, padding:"5px 7px", borderRadius:6, cursor:"pointer" }}>
                      <span style={{ fontSize:10, color:"#94A3B8", display:"inline-block", transform:expYr === yr ? "rotate(90deg)" : "none" }}>▶</span>
                      <span style={{ fontSize:11 }}>📂</span>
                      <span style={{ fontSize:10, color:"#374151" }}>{yr}</span>
                    </div>
                    {expYr === yr && cats.map(cat => (
                      <div key={cat} onClick={() => { setSelCat(cat); if (isMo) setDocPane("files"); }}
                        style={{ display:"flex", alignItems:"center", gap:6, padding:"5px 7px 5px 26px", borderRadius:6, cursor:"pointer", background:selCat === cat ? "#EFF6FF" : "transparent" }}>
                        <span style={{ fontSize:11 }}>📁</span>
                        <span style={{ fontSize:10, color:selCat === cat ? BL : "#64748B" }}>{cat}</span>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            ))}
          </div>
        )}

        {(!isMo || docPane === "files") && (
          <div style={{ flex:1, background:"white", borderRadius:10, border:"1px solid #E5E7EB", padding:14, overflowY:"auto", minWidth:0 }}>
            <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center", marginBottom:12, flexWrap:"wrap", gap:8 }}>
              <div>
                <div style={{ fontWeight:700, fontSize:13, color:NV }}>{selCat || "Select a folder"}</div>
                {selCat && <div style={{ fontSize:10, color:"#94A3B8", marginTop:2 }}>{expCl} / {expYr} / {selCat}</div>}
              </div>
              {selCat && (
                <div style={{ display:"flex", gap:7 }}>
                  <button style={{ padding:"6px 10px", borderRadius:7, background:"#F8FAFC", border:"1px solid #E5E7EB", fontSize:11, fontWeight:600, color:"#374151", cursor:"pointer" }}>⬇ All</button>
                  <button style={{ padding:"6px 10px", borderRadius:7, background:OR, border:"none", fontSize:11, fontWeight:600, color:"white", cursor:"pointer" }}>⬆ Upload</button>
                </div>
              )}
            </div>
            {!selCat ? (
              <div style={{ display:"flex", flexDirection:"column", alignItems:"center", justifyContent:"center", height:200, color:"#CBD5E1" }}>
                <div style={{ fontSize:36 }}>📁</div>
                <div style={{ fontSize:13, marginTop:10 }}>Select a folder to view files</div>
              </div>
            ) : (
              (DOC_FILES[selCat] || []).map((f, i) => (
                <div key={i} style={{ display:"flex", alignItems:"center", gap:10, padding:"11px 10px", borderBottom:"1px solid #F8FAFC", flexWrap:isMo ? "wrap" : "nowrap" }}>
                  <span style={{ fontSize:16, flexShrink:0 }}>📄</span>
                  <div style={{ flex:1, minWidth:0 }}>
                    <div style={{ fontSize:12, color:"#374151", fontWeight:500, overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{f.name}</div>
                    <div style={{ fontSize:10, color:"#94A3B8", marginTop:2 }}>{f.size} · {f.uploaded} · {f.by}</div>
                  </div>
                  <span style={{ fontSize:10, padding:"2px 7px", borderRadius:4, background:"#EFF6FF", color:BL, fontWeight:700, flexShrink:0 }}>{f.version}</span>
                  <div style={{ display:"flex", gap:10, flexShrink:0 }}>
                    <span style={{ cursor:"pointer", color:"#94A3B8", fontSize:14 }}>👁</span>
                    <span style={{ cursor:"pointer", color:"#94A3B8", fontSize:14 }}>⬇</span>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
}

// ── COMMUNICATIONS screen ────────────────────────────────────────────
function CommsScreen({ isMo, commView, setCommView, selEmail, setSelEmail, selTmpl, setSelTmpl, aiDraft, setAiDraft, aiLoad, cBody, setCBody, triggerAI }) {
  const showList   = !isMo || commView === "list";
  const showThread = !isMo || commView === "thread" || commView === "compose";

  return (
    <div style={{ display:"flex", gap:12, height:isMo ? undefined : 580 }}>
      {showList && (
        <div style={{ width:isMo ? "100%" : 220, background:"white", borderRadius:10, border:"1px solid #E5E7EB", flexShrink:0, overflow:"hidden", display:"flex", flexDirection:"column" }}>
          <div style={{ padding:"11px 13px", borderBottom:"1px solid #F1F5F9", display:"flex", justifyContent:"space-between", alignItems:"center", flexShrink:0 }}>
            <span style={{ fontWeight:700, fontSize:12, color:NV }}>Client Emails</span>
            <button onClick={() => { setCommView("compose"); setCBody(""); setAiDraft(""); setSelTmpl(null); }}
              style={{ background:OR, border:"none", color:"white", borderRadius:6, padding:"4px 10px", fontSize:10, cursor:"pointer", fontWeight:600 }}>+ Compose</button>
          </div>
          <div style={{ overflowY:"auto", flex:1 }}>
            {EMAILS.map(em => (
              <div key={em.id} onClick={() => { setSelEmail(em); setCommView("thread"); }}
                style={{ padding:"11px 13px", cursor:"pointer", borderBottom:"1px solid #F9FAFB", background:!isMo && commView !== "compose" && selEmail?.id === em.id ? "#FFF8F5" : "white", borderLeft:!isMo && commView !== "compose" && selEmail?.id === em.id ? `3px solid ${OR}` : "3px solid transparent" }}>
                <div style={{ fontWeight:700, fontSize:11, color:NV }}>{em.client.length > 22 ? em.client.slice(0,22)+"…" : em.client}</div>
                <div style={{ fontSize:11, color:"#374151", marginTop:2, fontWeight:500, overflow:"hidden", whiteSpace:"nowrap", textOverflow:"ellipsis" }}>{em.subject}</div>
                <div style={{ fontSize:9, color:"#94A3B8", marginTop:3 }}>{em.date}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {showThread && (
        <div style={{ flex:1, background:"white", borderRadius:10, border:"1px solid #E5E7EB", display:"flex", flexDirection:"column", overflow:"hidden", minWidth:0 }}>
          {commView === "compose" ? (
            <div style={{ padding:16, overflowY:"auto", flex:1 }}>
              <div style={{ fontWeight:700, fontSize:13, color:NV, marginBottom:12 }}>New Email</div>
              <div style={{ marginBottom:13 }}>
                <div style={{ fontSize:11, fontWeight:700, color:"#374151", marginBottom:8 }}>Use a Template</div>
                <div style={{ display:"grid", gridTemplateColumns:"repeat(auto-fit,minmax(160px,1fr))", gap:7 }}>
                  {TEMPLATES.map(t => (
                    <div key={t.id} onClick={() => setSelTmpl(t)}
                      style={{ padding:"9px 11px", borderRadius:8, border:`1px solid ${selTmpl?.id === t.id ? OR : "#E5E7EB"}`, background:selTmpl?.id === t.id ? "#FFF8F5" : "white", cursor:"pointer" }}>
                      <div style={{ fontSize:11, fontWeight:600, color:selTmpl?.id === t.id ? OR : "#374151", lineHeight:1.4 }}>{t.name}</div>
                      <div style={{ fontSize:9, color:"#94A3B8", marginTop:3 }}>{t.cat}</div>
                    </div>
                  ))}
                </div>
              </div>
              <div style={{ display:"flex", alignItems:"center", gap:8, margin:"12px 0" }}>
                <div style={{ flex:1, height:1, background:"#F1F5F9" }} />
                <span style={{ fontSize:10, color:"#94A3B8", whiteSpace:"nowrap" }}>or use AI assistance</span>
                <div style={{ flex:1, height:1, background:"#F1F5F9" }} />
              </div>
              <input placeholder="To: client@email.com" style={{ width:"100%", padding:"8px 10px", borderRadius:7, border:"1px solid #E5E7EB", fontSize:12, outline:"none", marginBottom:8, boxSizing:"border-box" }} />
              <input placeholder="Subject" style={{ width:"100%", padding:"8px 10px", borderRadius:7, border:"1px solid #E5E7EB", fontSize:12, outline:"none", marginBottom:8, boxSizing:"border-box" }} />
              <textarea value={cBody || aiDraft} onChange={e => setCBody(e.target.value)} placeholder="Write your message..."
                style={{ width:"100%", height:90, padding:10, borderRadius:7, border:"1px solid #E5E7EB", fontSize:12, resize:"none", outline:"none", fontFamily:"inherit", boxSizing:"border-box" }} />
              {aiDraft && !cBody && (
                <div style={{ background:"#F0FDF4", border:"1px solid #BBF7D0", borderRadius:8, padding:12, marginTop:8 }}>
                  <div style={{ fontSize:10, color:"#166534", fontWeight:700, marginBottom:6 }}>✦ AI Draft — review before sending</div>
                  <div style={{ fontSize:11, color:"#1E293B", whiteSpace:"pre-line", lineHeight:1.6 }}>{aiDraft}</div>
                  <button onClick={() => setCBody(aiDraft)} style={{ marginTop:8, padding:"4px 12px", borderRadius:6, background:OR, border:"none", color:"white", fontSize:10, fontWeight:600, cursor:"pointer" }}>Use this draft</button>
                </div>
              )}
              <div style={{ display:"flex", gap:8, marginTop:10, flexWrap:"wrap" }}>
                <button onClick={triggerAI} disabled={aiLoad}
                  style={{ padding:"8px 14px", borderRadius:8, background:NV, border:"none", color:"white", fontSize:11, fontWeight:600, cursor:"pointer" }}>
                  {aiLoad ? "✦ Drafting…" : "✦ AI Draft"}
                </button>
                <button style={{ flex:1, padding:"8px 14px", borderRadius:8, background:OR, border:"none", color:"white", fontSize:11, fontWeight:600, cursor:"pointer" }}>➤ Send</button>
              </div>
              <div style={{ marginTop:10, padding:"8px 10px", borderRadius:7, background:"#FFFBEB", border:"1px solid #FDE68A", fontSize:10, color:"#92400E" }}>
                ⚠ AI drafts require your review. System will never auto-send.
              </div>
            </div>
          ) : selEmail ? (
            <>
              <div style={{ padding:"12px 16px", borderBottom:"1px solid #F1F5F9", display:"flex", justifyContent:"space-between", alignItems:"flex-start", gap:8, flexShrink:0, flexWrap:"wrap" }}>
                <div style={{ minWidth:0 }}>
                  <div style={{ fontWeight:700, fontSize:13, color:NV, overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>{selEmail.subject}</div>
                  <div style={{ fontSize:10, color:"#94A3B8", marginTop:2 }}>{selEmail.client} · {selEmail.thread.length} msg · Gmail</div>
                </div>
                <div style={{ display:"flex", gap:7, flexShrink:0 }}>
                  <button style={{ padding:"5px 10px", borderRadius:7, background:"#FEF2F2", border:"1px solid #FECACA", fontSize:10, fontWeight:600, color:"#991B1B", cursor:"pointer" }}>⚑ Escalate</button>
                  <button onClick={() => setCommView("compose")} style={{ padding:"5px 10px", borderRadius:7, background:OR, border:"none", fontSize:10, fontWeight:600, color:"white", cursor:"pointer" }}>Reply</button>
                </div>
              </div>
              <div style={{ flex:1, overflowY:"auto", padding:14, display:"flex", flexDirection:"column", gap:12 }}>
                {selEmail.thread.map((msg, i) => (
                  <div key={i} style={{ display:"flex", flexDirection:"column", alignItems:msg.team ? "flex-end" : "flex-start" }}>
                    <div style={{ maxWidth:"82%", background:msg.team ? NV : "#F8FAFC", borderRadius:10, padding:"12px 14px", border:msg.team ? "none" : "1px solid #E5E7EB" }}>
                      <div style={{ fontSize:10, fontWeight:700, color:msg.team ? "#93C5FD" : OR, marginBottom:5 }}>{msg.from}</div>
                      <div style={{ fontSize:12, color:msg.team ? "rgba(255,255,255,0.88)" : "#374151", lineHeight:1.65 }}>{msg.body}</div>
                      <div style={{ fontSize:9, color:msg.team ? "rgba(255,255,255,0.35)" : "#94A3B8", marginTop:7 }}>{msg.date}</div>
                    </div>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div style={{ display:"flex", alignItems:"center", justifyContent:"center", flex:1, color:"#CBD5E1", fontSize:13 }}>Select a thread</div>
          )}
        </div>
      )}
    </div>
  );
}

// ── TASKS screen ─────────────────────────────────────────────────────
function TasksScreen({ isMo, role }) {
  return (
    <div>
      <div style={{ display:"flex", gap:8, marginBottom:12, flexWrap:"wrap" }}>
        <input placeholder="Search tasks..." style={{ flex:1, minWidth:isMo ? "100%" : 200, padding:"8px 10px", borderRadius:8, border:"1px solid #E5E7EB", fontSize:12, outline:"none", background:"white", boxSizing:"border-box" }} />
        <select style={{ padding:"8px 10px", borderRadius:8, border:"1px solid #E5E7EB", fontSize:12, background:"white", outline:"none", color:"#374151" }}>
          <option>All Associates</option>
          <option>Maria Santos</option>
          <option>José Rivera</option>
          <option>Luis Morales</option>
        </select>
        {role === "Administrator" && (
          <button style={{ padding:"8px 14px", borderRadius:8, background:OR, color:"white", border:"none", fontSize:12, fontWeight:600, cursor:"pointer", flexShrink:0 }}>+ New Task</button>
        )}
      </div>
      <div style={{ display:"flex", gap:11, overflowX:"auto", paddingBottom:8 }}>
        {KANBAN_COLS.map(col => {
          const colTasks = TASKS.filter(t => t.status === col);
          return (
            <div key={col} style={{ minWidth:isMo ? 210 : undefined, flex:isMo ? "0 0 210px" : 1 }}>
              <div style={{ display:"flex", alignItems:"center", gap:7, marginBottom:9 }}>
                <div style={{ width:9, height:9, borderRadius:"50%", background:COL_DOT[col] }} />
                <span style={{ fontWeight:700, fontSize:12, color:NV }}>{col}</span>
                <span style={{ fontSize:10, color:"#94A3B8", marginLeft:"auto", background:"#F1F5F9", borderRadius:10, padding:"1px 7px" }}>{colTasks.length}</span>
              </div>
              <div style={{ display:"flex", flexDirection:"column", gap:8 }}>
                {colTasks.map(t => (
                  <div key={t.id} style={{ background:"white", borderRadius:9, padding:12, border:"1px solid #E5E7EB", cursor:"pointer" }}>
                    <div style={{ fontSize:11, fontWeight:700, color:NV, lineHeight:1.45, marginBottom:6 }}>{t.title}</div>
                    <div style={{ fontSize:10, color:"#94A3B8", marginBottom:8, overflow:"hidden", textOverflow:"ellipsis", whiteSpace:"nowrap" }}>👤 {t.client}</div>
                    <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center", marginBottom:8 }}>
                      <span style={{ fontSize:10, fontWeight:700, color:t.priority === "High" ? "#DC2626" : t.priority === "Medium" ? "#D97706" : "#6B7280" }}>{t.priority}</span>
                      <span style={{ fontSize:9, color:"#94A3B8" }}>⏰ {t.due}</span>
                    </div>
                    <div style={{ paddingTop:8, borderTop:"1px solid #F1F5F9", display:"flex", alignItems:"center", gap:6 }}>
                      <div style={{ width:20, height:20, borderRadius:"50%", background:NV, display:"flex", alignItems:"center", justifyContent:"center", fontSize:7, fontWeight:700, color:"white", flexShrink:0 }}>{t.initials}</div>
                      <span style={{ fontSize:10, color:"#64748B" }}>{t.assignee}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
