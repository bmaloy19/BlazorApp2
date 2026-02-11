# Project Architecture & CSS Guide

## Project Structure

```
BlazorApp2/
├── Components/
│   ├── Account/                    # Identity/Auth components (ASP.NET Identity)
│   │   ├── Pages/                  # Auth pages (Login, Register, etc.)
│   │   │   └── Manage/             # Account management pages
│   │   └── Shared/                 # Shared auth components
│   │
│   ├── Layout/                     # App layout components
│   │   ├── MainLayout.razor        # Main application layout
│   │   └── NavMenu.razor           # Navigation sidebar
│   │
│   ├── Pages/                      # Main application pages
│   │   ├── Home.razor              # Landing page
│   │   ├── Garage.razor            # Vehicle list/grid
│   │   ├── AddVehicle.razor        # Add new vehicle page
│   │   ├── VehicleDetails.razor    # Vehicle detail/edit page
│   │   └── Error.razor             # Error page
│   │
│   ├── Shared/                     # Reusable UI components (generic)
│   │   ├── Badge.razor             # Badge component
│   │   ├── ConfirmDialog.razor     # Modal confirmation dialog
│   │   ├── DetailCard.razor        # Card with header
│   │   ├── EmptyState.razor        # No data display
│   │   ├── LoadingState.razor      # Loading spinner
│   │   └── PageHeader.razor        # Page header component
│   │
│   ├── Vehicles/                   # Vehicle-specific components
│   │   ├── VehicleCard.razor       # Vehicle display card/row
│   │   ├── AddVehicleCard.razor    # "Add new" card
│   │   ├── VehicleHero.razor       # Vehicle hero section
│   │   ├── CarSelectorCard.razor   # Manual vehicle selector
│   │   └── VinDecoderCard.razor    # VIN decoder card
│   │
│   ├── App.razor                   # Root app component
│   ├── Routes.razor                # Router configuration
│   └── _Imports.razor              # Global using statements
│
├── Data/
│   ├── Auth/                       # Identity data
│   │   ├── ApplicationDbContext.cs
│   │   └── ApplicationUser.cs
│   └── Models/                     # Domain models
│       ├── Make.cs
│       ├── Vehicle.cs
│       ├── UserVehicle.cs
│       └── ServiceRecord.cs
│
├── Services/                       # Business logic services
│   ├── VehicleService.cs
│   └── ServiceRecordService.cs
│
├── wwwroot/
│   ├── app.css                     # Main CSS file
│   ├── css/
│   │   ├── base/
│   │   │   └── _variables.css      # CSS Design Tokens (for whitelabeling)
│   │   └── components/
│   │       └── _components.css     # Reusable component styles
│   └── lib/                        # Third-party libraries
│
├── Program.cs                      # Application entry point
└── appsettings.json                # Configuration
```

---

## CSS Design System

### Design Tokens (`wwwroot/css/base/_variables.css`)

The foundation for **whitelabeling**. Change brand colors here:

```css
:root {
    /* Brand Colors - CHANGE THESE FOR WHITELABELING */
    --brand-primary: #d10000;
    --brand-primary-hover: #a00000;
    --brand-primary-light: #ff4444;
    --brand-primary-rgb: 209, 0, 0;
    
    /* Semantic tokens reference brand colors */
    --color-danger: var(--brand-primary);
    --border-focus: var(--brand-primary);
    /* ... etc */
}
```

### Dark Mode

Built-in dark mode support:
```css
[data-theme="dark"] { /* Dark overrides */ }
@media (prefers-color-scheme: dark) { /* Auto dark mode */ }
```

---

## Reusable Components

### Generic Components (`Components/Shared/`)

| Component | Purpose | Usage |
|-----------|---------|-------|
| `Badge` | Status/label display | `<Badge Variant="Badge.BadgeVariant.Primary">Text</Badge>` |
| `ConfirmDialog` | Modal confirmations | `<ConfirmDialog @ref="_dialog" OnConfirm="..." />` |
| `DetailCard` | Card with header | `<DetailCard Title="Info"><Content>...</Content></DetailCard>` |
| `EmptyState` | No data display | `<EmptyState Title="No items" />` |
| `LoadingState` | Loading spinner | `<LoadingState Message="Loading..." />` |
| `PageHeader` | Page title/actions | `<PageHeader Title="My Page" ShowBackButton="true" />` |

### Vehicle Components (`Components/Vehicles/`)

| Component | Purpose |
|-----------|---------|
| `VehicleCard` | Display vehicle in card or list format |
| `AddVehicleCard` | "Add new vehicle" action card |
| `VehicleHero` | Hero section for vehicle details |
| `CarSelectorCard` | Manual make/model/year selector |
| `VinDecoderCard` | VIN input and decode |

---

## Pages

| Page | Route | Description |
|------|-------|-------------|
| `Home.razor` | `/` | Landing page with features |
| `Garage.razor` | `/garage` | Vehicle list (requires auth) |
| `AddVehicle.razor` | `/garage/add` | Add new vehicle (requires auth) |
| `VehicleDetails.razor` | `/garage/vehicle/{id}` | Vehicle details (requires auth) |
| `Error.razor` | `/Error` | Error page |

---

## Adding New Features

### Adding a new page:
1. Create `Components/Pages/NewPage.razor`
2. Add route: `@page "/new-page"`
3. If protected: `@attribute [Authorize]`
4. Add to NavMenu if needed

### Adding a new reusable component:
1. Generic component → `Components/Shared/`
2. Domain-specific → `Components/Vehicles/` (or create new folder)
3. Add namespace to `_Imports.razor` if new folder

### Adding new styles:
1. Component-specific → Use `<style>` block in component
2. Reusable patterns → Add to `wwwroot/css/components/_components.css`
3. Theme tokens → Add to `wwwroot/css/base/_variables.css`

---

## Whitelabeling Checklist

To rebrand the app for a different client:

1. **Change brand colors** in `_variables.css`:
   - `--brand-primary`
   - `--brand-primary-hover`
   - `--brand-primary-light`
   - `--brand-primary-rgb`

2. **Update app name** in:
   - `NavMenu.razor` (navbar brand)
   - `Home.razor` (hero title)
   - Page titles

3. **Replace favicon** in `wwwroot/`

4. **Optional**: Create theme file:
   ```css
   [data-theme="client-name"] {
       --brand-primary: #new-color;
       /* ... */
   }
   ```
