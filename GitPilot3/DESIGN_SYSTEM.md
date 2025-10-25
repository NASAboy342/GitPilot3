# GitPilot3 Design System - Component Examples

## 🎨 Color Palette

Our design system uses a carefully curated dark theme with the following core colors:

### Primary Colors
- **Primary Blue**: `#2196F3` - Main actions, current branch, focus states
- **Primary Light**: `#64B5F6` - Hover states
- **Primary Dark**: `#1976D2` - Active states

### Semantic Colors
- **Success Green**: `#4CAF50` - Git additions, success states, local branches
- **Warning Orange**: `#FF9800` - Git modifications, warnings, remote branches
- **Error Red**: `#F44336` - Git deletions, errors
- **Info Blue**: `#2196F3` - Information, current states

### Background Colors
- **Primary**: `#1E1E1E` - Main application background
- **Secondary**: `#252525` - Panel backgrounds (sidebars)
- **Tertiary**: `#2D2D2D` - Headers, elevated surfaces
- **Quaternary**: `#3C3C3C` - Toolbars, active areas

### Text Colors
- **Primary**: `#FFFFFF` - Main text content
- **Secondary**: `#E0E0E0` - Secondary information
- **Muted**: `#A0A0A0` - Timestamps, metadata

## 🔘 Button Components

### Example Usage:
```xml
<!-- Primary action buttons -->
<Button Classes="Primary" Content="Clone Repository" />
<Button Classes="Primary Large" Content="Large Primary Action" />

<!-- Secondary buttons -->
<Button Classes="Secondary" Content="Cancel" />
<Button Classes="Secondary Small" Content="Settings" />

<!-- Semantic buttons -->
<Button Classes="Success" Content="Merge Branch" />
<Button Classes="Warning" Content="Force Push" />

<!-- Toolbar buttons -->
<Button Classes="Secondary Toolbar" Content="Fetch" />
<Button Classes="Success Toolbar" Content="Pull" />
```

### Button Variants:
- **Primary**: Main actions (Clone, Commit, Push)
- **Secondary**: Secondary actions (Cancel, Settings)
- **Success**: Positive actions (Merge, Pull)
- **Warning**: Cautionary actions (Force Push, Delete)
- **Toolbar**: Toolbar-specific styling
- **Small/Large**: Size variants

## 📝 Typography System

### Headings:
```xml
<TextBlock Classes="H4" Text="Section Title" />
<TextBlock Classes="H5" Text="Subsection Title" />
<TextBlock Classes="H6" Text="Panel Header" />
```

### Body Text:
```xml
<TextBlock Classes="Body" Text="Regular content text" />
<TextBlock Classes="Caption" Text="Supporting information" />
<TextBlock Classes="Label" Text="Form labels and UI text" />
```

### Specialized Text:
```xml
<TextBlock Classes="CommitMessage" Text="feat: Add new feature" />
<TextBlock Classes="CommitHash" Text="a1b2c3d4e5f6" />
<TextBlock Classes="AuthorName" Text="John Doe" />
<TextBlock Classes="Timestamp" Text="2 hours ago" />
<TextBlock Classes="FilePath" Text="/src/components/Button.tsx" />
<TextBlock Classes="CodeInline" Text="git status" />
```

## 📦 Layout Components

### Panel Types:
```xml
<!-- Header panels -->
<Border Classes="Header">
    <TextBlock Classes="H5" Text="Repository Information" />
</Border>

<!-- Content areas -->
<Border Classes="Content">
    <!-- Main application content -->
</Border>

<!-- Sidebars -->
<Border Classes="Sidebar">
    <!-- Navigation and tools -->
</Border>

<!-- Section dividers -->
<Border Classes="Section">
    <TextBlock Classes="H6" Text="Working Directory" />
</Border>

<!-- Status/footer areas -->
<Border Classes="Footer">
    <TextBlock Text="Ready • Branch: main" />
</Border>
```

## 🔄 Git-Specific Components

### File Status Indicators:
```xml
<TextBlock Classes="GitAdded" Text="+ Added file" />
<TextBlock Classes="GitModified" Text="M Modified file" />
<TextBlock Classes="GitDeleted" Text="- Deleted file" />
```

### File Change Panels:
```xml
<Border Classes="FileAdded">
    <StackPanel>
        <StackPanel Orientation="Horizontal">
            <TextBlock Classes="FileIcon" Text="📄" />
            <TextBlock Classes="Body" Text="MainWindow.axaml" />
            <TextBlock Classes="CodeInline GitAdded" Text="+45 -12" />
        </StackPanel>
        <TextBlock Classes="Caption" Text="Modified window layout" />
    </StackPanel>
</Border>
```

### Branch Indicators:
```xml
<TextBlock Classes="BranchCurrent" Text="main" />
<TextBlock Classes="BranchName" Text="feature/new-ui" />
```

### Sync Status:
```xml
<TextBlock Classes="SyncAhead" Text="↑ 2" ToolTip.Tip="Commits ahead" />
<TextBlock Classes="SyncBehind" Text="↓ 1" ToolTip.Tip="Commits behind" />
```

## 🎯 Commit Graph Elements

### Commit Nodes:
```xml
<Ellipse Classes="CommitNode CommitNodeCurrent" />
<Ellipse Classes="CommitNode CommitNodeLocal" />
<Ellipse Classes="CommitNode CommitNodeRemote" />
```

### Branch Lines:
```xml
<Line Classes="BranchLine BranchLineCurrent" StartPoint="60,70" EndPoint="60,120" />
<Line Classes="BranchLine BranchLineLocal" StartPoint="60,70" EndPoint="60,120" />
<Line Classes="BranchLine BranchLineRemote" StartPoint="60,70" EndPoint="60,120" />
```

## 🎨 Best Practices

### 1. Semantic Usage
Always use semantic classes that match the meaning:
- Use `Success` colors for additions and positive actions
- Use `Warning` colors for modifications and caution
- Use `Error` colors for deletions and dangerous actions
- Use `Primary` colors for main actions and current states

### 2. Consistent Hierarchy
Follow the typography hierarchy:
- H4 for main section titles
- H5 for subsection titles
- H6 for panel headers
- Body for content text
- Caption for supporting information

### 3. Proper Contrast
The design system ensures proper contrast ratios:
- Primary text (`#FFFFFF`) for main content
- Secondary text (`#E0E0E0`) for labels
- Muted text (`#A0A0A0`) for timestamps and metadata

### 4. Component Composition
Combine classes for complex components:
```xml
<Button Classes="Primary Large" Content="Main Action" />
<TextBlock Classes="H5 BranchCurrent" Text="Current Branch" />
<Border Classes="FileAdded">
    <TextBlock Classes="GitAdded" Text="Added File" />
</Border>
```

## 🚀 Implementation Benefits

### Consistency
- Uniform appearance across all components
- Predictable behavior and styling
- Easy to maintain and update

### Accessibility  
- Proper contrast ratios for readability
- Semantic color usage for meaning
- Clear visual hierarchy

### Developer Experience
- Easy to use class-based system
- Self-documenting component names
- Extensible and maintainable

### Git Client Specific
- Purpose-built for Git workflow visualization
- Intuitive color coding for Git states
- Professional appearance matching industry standards

This design system provides a solid foundation for building a comprehensive Git client that's both functional and visually appealing, with consistent styling that scales well as new features are added.