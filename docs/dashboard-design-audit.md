# Dashboard Design Audit

**Date:** 2026-03-04
**Design file:** `designs/amalgam.pen` (frames: `Desktop - Dashboard`, `Tablet - Dashboard`, `Mobile - Dashboard`)
**Running app:** `http://localhost:4200/dashboard`

---

## Summary

The current running dashboard diverges significantly from the design. The main issues span the sidebar navigation, main content layout, stat cards, quick action buttons, a missing validation banner, and an incorrect content background color. Below is a detailed list of every discrepancy.

---

## 1. Sidebar / Navigation

### 1.1 Missing nav items
- **Design:** 6 nav items — Dashboard, Repositories, Backend Config, Frontend Config, Templates, YAML Preview
- **Current app:** 4 nav items — Dashboard, Repos, Config, Templates
- **Changes needed:**
  - Rename "Repos" to "Repositories"
  - Split "Config" into "Backend Config" (icon: `tune`) and "Frontend Config" (icon: `web`)
  - Add "YAML Preview" (icon: `code`) nav item
  - Update `navItems` in `layout-container.component.ts` accordingly

### 1.2 Sidebar header / logo
- **Design:** Blue rounded square (#3B82F6, 32×32, corner-radius 8) containing white "A" (Inter 18px 800) + "Amalgam" text (Inter 18px 700, #F1F5F9). Padding: 20px. Aligned center with gap: 10px.
- **Current app:** Plain `<h2>Amalgam</h2>` with padding 16px 24px and a bottom border
- **Changes needed:**
  - Add the blue "A" logo square before the title text
  - Update padding from `16px 24px` to `20px`
  - Remove the `border-bottom` (design uses no divider under the logo row)
  - Set font-size to 18px, font-weight to 700

### 1.3 Nav item styling
- **Design:** Active item has background `#1E3A5F` with blue icon/text (#3B82F6), corner-radius 8px, padding `10px 12px`. Inactive items have `#94A3B8` icon/text, font-weight 500. Nav list has gap: 4px and padding `8px 12px`.
- **Current app:** Active item uses `rgba(255,255,255,0.12)` background, no border-radius. Padding is `12px 24px`. Nav list has padding `8px 0`.
- **Changes needed:**
  - Change `.nav-item` padding from `12px 24px` to `10px 12px`
  - Add `border-radius: 8px` to `.nav-item`
  - Change `.nav-list` padding from `8px 0` to `8px 12px`
  - Add `gap: 4px` (use flex-direction column + gap) on `.nav-list`
  - Active state: change background to `#1E3A5F`, icon/text color to `#3B82F6`, font-weight to 600
  - Inactive state: set icon/text color to `#94A3B8`, font-weight to 500

### 1.4 Sidebar border
- **Design:** 1px inside border `#334155`
- **Current app:** No visible border
- **Changes needed:** Add `border-right: 1px solid #334155` to `.sidebar`

---

## 2. Main Content Area Background

- **Design:** Background is `#0F172A` (the global `$background` token)
- **Current app:** Background is `#f8fafc` (light gray) — set in `.content` in `layout-container.component.scss`
- **Changes needed:** Change `.content` background from `#f8fafc` to `#0F172A` (or use the `$background` variable)

---

## 3. Dashboard Page Header

### 3.1 Title + action button row
- **Design:** Flex row with `justify-content: space-between`. Left: "Dashboard" (Inter 28px 700, #F1F5F9). Right: "New Configuration" primary button (blue filled, #3B82F6 bg, white text, corner-radius 8, padding 10px 24px).
- **Current app:** Only has `<h1>Dashboard</h1>` with no action button on the right.
- **Changes needed:**
  - Wrap the heading in a flex row with `justify-content: space-between`
  - Add a "New Configuration" primary button on the right side (should navigate to `/wizard`)
  - Set h1 font-weight to 700 (currently 600)

### 3.2 Page padding
- **Design:** 32px padding on the main content area, 24px gap between sections
- **Current app:** 24px padding, 24px margin-bottom on h1
- **Changes needed:** Increase `.dashboard-page` padding from `24px` to `32px`, use flex/vertical layout with `gap: 24px` between sections

---

## 4. Validation Banner (Missing)

- **Design:** Success alert banner immediately after the header row — green themed (#1A2E1A bg, #86EFAC text), with check_circle icon, title "Configuration Valid", description "All 5 repositories validated successfully. Last validated 2 minutes ago." Full width.
- **Current app:** The validation status is buried inside a card titled "Validation Status". No standalone banner exists.
- **Changes needed:**
  - Add a top-level `<ui-alert>` banner between the header row and the stats grid
  - Show `severity="success"` when validation passes, `severity="error"` when it fails
  - Display it as a full-width banner with the validation message and description text
  - The alert component may need enhancement to support a title + description (currently only has `message`)

---

## 5. Stats Cards

### 5.1 Layout and count
- **Design:** 4 equal-width stat cards in a single row — Microservices (2), Libraries (1), Plugins (1), Dashboard (1). Each card shows a large number + label.
- **Current app:** 3 generic cards in an auto-fill grid — "Total Repositories", "By Type" (with chips), "Validation Status"
- **Changes needed:**
  - Replace the 3-card layout with 4 color-coded stat cards
  - Each card maps to a repository type from `countByType`
  - Remove the "Total Repositories", "By Type", and "Validation Status" cards entirely

### 5.2 Individual stat card design
- **Design per card:**
  - Corner-radius: 12px
  - Padding: 20px
  - Vertical layout, gap: 4px, centered items
  - Large number: Inter 36px 700
  - Label: Inter 14px 500
  - Each card has a unique color theme:
    - **Microservices:** bg `#1E3A5F`, text `#93C5FD`
    - **Libraries:** bg `#1A2E1A`, text `#86EFAC`
    - **Plugins:** bg `#2E1A3B`, text `#C4B5FD`
    - **Dashboard:** bg `#2E2A1A`, text `#FCD34D`
- **Current app:** Uses generic `<ui-card>` with Material card styling (dark surface bg, title header)
- **Changes needed:**
  - Create styled stat cards (either new component or custom styling)
  - Use `display: flex; gap: 16px` for the row (not CSS grid)
  - Each card should be `flex: 1` (equal width)
  - Apply the color themes per repository type
  - Remove `max-width: 960px` constraint on `.dashboard-page` — design uses full width

---

## 6. Quick Actions Row (Missing)

- **Design:** 4 outline buttons in a row — "Run Wizard", "Add Repository", "Validate", "Export YAML". Each button: outline style (#1E293B bg, 1.5px `#334155` border, `#3B82F6` text, Inter 14px 600), corner-radius 8, padding 10px 24px, justify-content center, full container width per button. Row has gap: 16px.
- **Current app:** Only a single FAB-style "Add Repository" button
- **Changes needed:**
  - Replace the FAB with a row of 4 outline buttons
  - Use existing `<ui-button variant="outline">` component
  - Wire up click handlers:
    - "Run Wizard" → navigate to `/wizard`
    - "Add Repository" → navigate to `/repositories/add`
    - "Validate" → trigger validation
    - "Export YAML" → navigate to `/config/yaml` or trigger export
  - Style the row with `display: flex; gap: 16px` with each button taking equal width

---

## 7. Recent Changes Section (Missing — Desktop design)

- **Design (mobile):** Has a "RECENT CHANGES" section header (Inter 12px 600, #64748B, letter-spacing 1px) followed by a card listing recent changes with icons, titles, descriptions, and timestamps.
- **Current app:** No recent changes section at all
- **Changes needed:**
  - Add a "RECENT CHANGES" section label
  - Add a card with a list of recent change entries
  - Each entry should show an icon, title (14px 600), and timestamp (13px, #94A3B8)
  - This requires a new API endpoint or data source for recent changes

---

## 8. Section Labels (Missing)

- **Design:** Has uppercase section labels above the stats grid ("REPOSITORIES") and quick actions ("QUICK ACTIONS") — Inter 12px 600, color `#64748B`, letter-spacing 1px
- **Current app:** No section labels
- **Changes needed:** Add styled section labels above the stats grid and quick actions row

---

## 9. Responsive / Tablet Layout

- **Design (Tablet - 768px):** Uses a narrow icon rail (72px) instead of a full sidebar. Rail shows "A" logo, icon-only nav items with small labels (10px). Main content uses 24px padding.
- **Current app:** Switches directly from full sidebar to bottom nav at 767px breakpoint
- **Changes needed:**
  - Add an intermediate breakpoint (e.g. 768px–1023px) that shows the icon rail layout
  - Create the rail component: 72px wide, vertical, icons + small labels, #1E293B bg with border
  - Adjust content padding to 24px for tablet

---

## 10. Mobile Layout Differences

- **Design (Mobile - 360px):**
  - Top header bar: hamburger menu icon, "Amalgam" title (blue), settings icon. `#1E293B` bg with bottom border.
  - Content padding: 16px, gap: 16px
  - Stats use a 2-column grid (2 cards per row) instead of 4 columns
  - Quick actions: only 2 buttons ("Run Wizard", "Add Repo") side by side
  - Bottom nav with 4 items: Dashboard, Repos, Config, Templates
- **Current app:** Has bottom nav but missing the header bar, stats grid is auto-fill, no quick action buttons
- **Changes needed:**
  - Add a mobile header bar with hamburger, title, and settings icon
  - Adjust stats grid for 2 columns on mobile
  - Add 2 quick action buttons on mobile

---

## File Change Summary

| File | Changes |
|------|---------|
| `domain/.../layout-container.component.ts` | Add 2 nav items (Frontend Config, YAML Preview), rename Repos→Repositories |
| `domain/.../layout-container.component.html` | Add logo mark, add tablet rail layout, add mobile header |
| `domain/.../layout-container.component.scss` | Restyle sidebar header (logo), nav items (border-radius, colors, padding), add rail styles, fix `.content` bg |
| `domain/.../dashboard-page.component.html` | Replace card layout with header row + banner + stat cards + quick actions + recent changes |
| `domain/.../dashboard-page.component.scss` | Rewrite: new stat card styles, quick action row, section labels, remove max-width |
| `domain/.../dashboard-page.component.ts` | Add navigation methods for wizard/validate/export, wire up quick actions |
| `components/.../alert/alert.component.*` | Add support for `title` + `description` (currently only `message`) |
| `api/.../dashboard.service.ts` | Possibly add endpoint for recent changes data |
| `amalgam/src/styles.scss` | Already has correct dark theme tokens — no changes needed |

---

## Priority Order

1. **P0 — Content background color** (`.content` bg: `#f8fafc` → `#0F172A`) — single line fix, highest visual impact
2. **P0 — Sidebar nav items** — add missing items, fix labels
3. **P0 — Stat cards** — replace 3 generic cards with 4 color-coded stat cards
4. **P0 — Quick actions row** — replace FAB with 4 outline buttons
5. **P1 — Sidebar styling** — logo, nav item border-radius/colors/padding
6. **P1 — Header row** — add "New Configuration" button
7. **P1 — Validation banner** — add top-level alert banner
8. **P1 — Section labels** — add "REPOSITORIES", "QUICK ACTIONS" labels
9. **P2 — Recent changes section** — new section + potential API work
10. **P2 — Tablet rail layout** — intermediate responsive breakpoint
11. **P2 — Mobile header** — hamburger + settings icons header bar
