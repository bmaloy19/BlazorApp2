# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added - February 14, 2026
- added basic user vehicle sharing my email.
- css updates

### Added - February 11, 2026
- completed email service
- overhauled auth pages flow and css architecture
- same todo

### Added - February 10, 2026
- overhauled folder structure	
- componitized pages
- kept css architecture in mind and updated
- started email service for auth in progress.
- prettied landing page/ nav bar/ and error page.
- same todos just did behind the scenes updates today.

### Added - February 8, 2026
- **Vehicle Details Page** (`/garage/vehicle/{id}`) - View and manage individual vehicles
  - Hero section with vehicle info, edit/delete buttons
  - Odometer & hours tracking card with toggle for hour meters
  - Vehicle information card (full-width) with all details
  - Maintenance summary and service history cards
  - Delete confirmation modal with proper backdrop
- **Clickable Garage Cards** - Cards now navigate directly to vehicle details (removed separate View button)
- **Vehicle Delete** - Full delete functionality with cascade removal of service records

### TODO -
- vehicle maintenance tracking
- share vehicles with other users
- light/dark mode toggle
- user profile management
- printable vehicle reports (XML/PDF export)
- calendar integration for service reminders
- optimize mobile responsiveness
- add odomedeter auto tracking.

### Added - February 5, 2026
- **Database Integration** - Vehicles now save/load from PostgreSQL with `VehicleService`. User-vehicle relationships tracked with ownership support.
- **Enhanced VIN Decoder** - Shows all decoded values with expandable details section. Extracts body class, vehicle type, and series.
- **CarSelectorCard Redesign** - Step-by-step form with live preview, expanded makes list from database.
- **Garage & UI Improvements** - Redesigned vehicle cards, improved responsive layouts, extensive CSS updates for cleaner styling.

### TODO - 
- Implement vehicle editing/deletion
- add vehicle view pages
- share vehicles with other users
- vehicle maintenance tracking
- light/dark mode toggle
- user profile management
- printable vehicle reports (XML/PDF export)
- calendar integration for service reminders
- optimize mobile responsiveness

### Added - February 4, 2026
- **Garage Page** (`/garage`) - New page to display and manage user's vehicles
  - Card view and list/row view toggle for displaying vehicles
  - Responsive design with sleek hover effects
  - Navigation link with car icon in sidebar

- **Add Vehicle Page** (`/garage/add`) - New page for adding vehicles to the garage
  - VIN Decoder integration using NHTSA API
  - Manual vehicle selector with Make/Year/Model dropdowns
  - Toggle between VIN decode and manual entry methods
  - Vehicle preview card with additional fields (color, license plate, mileage, notes)

- **New Components**
  - `VinDecoderCard.razor` - Decodes VIN using NHTSA API and extracts vehicle details
  - `CarSelectorCard.razor` - Manual vehicle selection with Make (temp list: FORD, MAZDA, MERCURY), Year, and Model dropdowns that fetch from NHTSA API

- **CSS Enhancements** (`app.css`)
  - Reorganized CSS variables for future light/dark mode support
  - Added comprehensive component styles (cards, alerts, badges, tables, modals, etc.)
  - Added utility classes for spacing, flexbox, colors

### Changed
- Cleaned up and organized existing CSS with semantic variables
- Improved button, form, and navigation styling
- Updated nav menu to include Garage link

### Next Steps
 - Add backend integration for saving vehicles to user profiles
 - Implement vehicle editing and deletion functionality
 - Expand vehicle data fields and improve UI/UX

### Future Ideas
 - Light/Dark mode toggle
 - user profile management, saving preferences
 - Vehicle maintenance tracking
 - sharing vehicles with other users/ plus ui for that
 - printable vehicle reports, xml/pdf export
 - calendar integration for service reminders?
 - keep moble responsiveness in mind
