# GitPilot3 Design System Usage Guide

## Overview
This document provides guidance on how to use the GitPilot3 design system components and styles consistently throughout the application.

## Color Palette

### Primary Colors
- **Primary**: `#2196F3` - Main brand color, used for primary actions and focus states
- **Primary Light**: `#64B5F6` - Hover states and lighter variants
- **Primary Dark**: `#1976D2` - Active states and borders

### Background Colors
- **Background Primary**: `#1E1E1E` - Main application background
- **Background Secondary**: `#252525` - Panel backgrounds
- **Background Tertiary**: `#2D2D2D` - Header and elevated surfaces
- **Background Quaternary**: `#3C3C3C` - Toolbar and active areas

### Text Colors
- **Text Primary**: `#FFFFFF` - Main text content
- **Text Secondary**: `#E0E0E0` - Secondary information
- **Text Tertiary**: `#C0C0C0` - Supporting text
- **Text Muted**: `#A0A0A0` - Timestamps and metadata

### Semantic Colors
- **Success**: `#4CAF50` - Git additions, success states
- **Warning**: `#FF9800` - Git modifications, warnings
- **Error**: `#F44336` - Git deletions, errors
- **Info**: `#2196F3` - Information states

## Button Usage

### Primary Buttons
Use for main actions like "Clone", "Commit", "Push":
```xml
<Button Classes="Primary" Content="Clone Repository" />
```

### Secondary Buttons
Use for secondary actions like "Cancel", "Settings":
```xml
<Button Classes="Secondary" Content="Cancel" />
```

### Icon Buttons
Use for toolbar actions and icon-only buttons:
```xml
<Button Classes="Icon Toolbar" Content="🔄" />
```

### Semantic Buttons
Use for actions with semantic meaning:
```xml
<Button Classes="Success" Content="Merge" />
<Button Classes="Warning" Content="Force Push" />
<Button Classes="Error" Content="Delete Branch" />
```

### Size Variants
```xml
<Button Classes="Small" Content="Small Button" />
<Button Classes="Large Primary" Content="Large Primary" />
```

## Typography

### Headings
```xml
<TextBlock Classes="H1" Text="Main Title" />
<TextBlock Classes="H2" Text="Section Title" />
<TextBlock Classes="H3" Text="Subsection" />
```

### Body Text
```xml
<TextBlock Classes="Body" Text="Regular body text" />
<TextBlock Classes="BodyLarge" Text="Large body text" />
<TextBlock Classes="BodySmall" Text="Small body text" />
```

### Specialized Text
```xml
<TextBlock Classes="CommitMessage" Text="feat: Add new feature" />
<TextBlock Classes="CommitHash" Text="a1b2c3d" />
<TextBlock Classes="BranchName" Text="main" />
<TextBlock Classes="FilePath" Text="/src/components/Button.tsx" />
<TextBlock Classes="AuthorName" Text="John Doe" />
<TextBlock Classes="Timestamp" Text="2 hours ago" />
```

### Code Text
```xml
<TextBlock Classes="CodeInline" Text="git status" />
<TextBlock Classes="CodeBlock" Text="const example = 'code block';" />
```

## Input Controls

### Text Inputs
```xml
<TextBox Classes="Search" Watermark="Search files..." />
<TextBox Classes="Code" AcceptsReturn="True" />
<TextBox Classes="Large" />
<TextBox Classes="Small" />
```

### Validation States
```xml
<TextBox Classes="Error" />
<TextBox Classes="Success" />
```

## Panels and Containers

### Cards
```xml
<Border Classes="Card">
    <StackPanel>
        <TextBlock Classes="H4" Text="Card Title" />
        <TextBlock Classes="Body" Text="Card content" />
    </StackPanel>
</Border>
```

### Elevated Cards
```xml
<Border Classes="ElevatedCard">
    <!-- Content -->
</Border>
```

### Layout Panels
```xml
<Border Classes="Header">
    <TextBlock Classes="H3" Text="Section Header" />
</Border>

<Border Classes="Content">
    <!-- Main content -->
</Border>

<Border Classes="Footer">
    <TextBlock Text="Status information" />
</Border>
```

### Semantic Panels
```xml
<Border Classes="Warning">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="⚠️" />
        <TextBlock Classes="WarningText" Text="Warning message" />
    </StackPanel>
</Border>

<Border Classes="Error">
    <TextBlock Classes="ErrorText" Text="Error message" />
</Border>

<Border Classes="Success">
    <TextBlock Classes="SuccessText" Text="Success message" />
</Border>
```

## Git-Specific Components

### File Status Indicators
```xml
<TextBlock Classes="GitAdded" Text="+ Added file" />
<TextBlock Classes="GitModified" Text="M Modified file" />
<TextBlock Classes="GitDeleted" Text="- Deleted file" />
<TextBlock Classes="GitUntracked" Text="? Untracked file" />
```

### Branch Indicators
```xml
<TextBlock Classes="BranchLocal" Text="main" />
<TextBlock Classes="BranchRemote" Text="origin/main" />
<TextBlock Classes="BranchCurrent" Text="feature/new-ui" />
```

### File Change Panels
```xml
<Border Classes="FileAdded">
    <StackPanel>
        <TextBlock Classes="Body" Text="MainWindow.axaml" />
        <TextBlock Classes="Caption GitAdded" Text="+45 -0" />
    </StackPanel>
</Border>
```

### Status Badges
```xml
<Border Classes="StatusBadge StatusBadgeSuccess">
    <TextBlock Classes="Badge" Text="✓" />
</Border>
```

### Sync Status
```xml
<TextBlock Classes="SyncAhead" Text="↑ 2" />
<TextBlock Classes="SyncBehind" Text="↓ 1" />
<TextBlock Classes="SyncUpToDate" Text="Up to date" />
```

## Best Practices

### 1. Consistency
Always use the provided classes instead of inline styles to maintain consistency.

### 2. Semantic Meaning
Use semantic colors and styles that match the meaning:
- Green for additions/success
- Orange for modifications/warnings  
- Red for deletions/errors
- Blue for information/current state

### 3. Accessibility
Ensure proper contrast by using the provided text color classes with appropriate backgrounds.

### 4. Responsive Design
Use the size variants (Small, Large) to create hierarchy and improve usability.

### 5. Component Composition
Combine classes to create complex components:
```xml
<Button Classes="Primary Large" Content="Main Action" />
<TextBlock Classes="H3 BranchCurrent" Text="Current Branch" />
```

## Resource References

All colors and styles are available as static resources:
- Colors: `{StaticResource PrimaryBrush}`
- Fonts: `{StaticResource DefaultFontFamily}`
- Sizes: `{StaticResource FontSizeBase}`

This design system provides a solid foundation for building a consistent, professional Git client interface that scales well as the application grows.