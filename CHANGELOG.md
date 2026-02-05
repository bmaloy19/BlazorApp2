# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

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
