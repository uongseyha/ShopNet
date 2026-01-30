# 🎨 ShopNet Global Blue Color Theme

## Color Palette

### Primary Blue Colors
```css
--primary-blue: #2563eb        /* Main blue (Tailwind blue-600) */
--primary-blue-dark: #1d4ed8   /* Dark blue (Tailwind blue-700) */
--primary-blue-light: #3b82f6  /* Light blue (Tailwind blue-500) */
--primary-blue-50: #eff6ff     /* Very light blue backgrounds */
--primary-blue-100: #dbeafe    /* Light blue backgrounds */
--primary-blue-600: #2563eb    /* Standard buttons/actions */
--primary-blue-700: #1d4ed8    /* Hover states */
--primary-blue-800: #1e40af    /* Active/pressed states */
```

## Usage Guidelines

### Tailwind Classes
Use these standardized Tailwind classes throughout the application:

#### Text Colors
- `text-blue-600` - Primary text color
- `text-blue-700` - Darker text for emphasis
- `hover:text-blue-600` - Hover state for links/buttons

#### Background Colors
- `bg-blue-600` - Primary button background
- `bg-blue-700` - Hover state for buttons
- `bg-blue-50` - Light background for hover effects
- `hover:bg-blue-50` - Subtle hover backgrounds

#### Border Colors
- `border-blue-600` - Primary borders
- `hover:border-blue-600` - Hover state borders

### CSS Custom Properties
For custom components, use the CSS variables:

```css
.my-custom-component {
  color: var(--primary-blue);
  background-color: var(--primary-blue-50);
  border-color: var(--primary-blue);
}

.my-custom-component:hover {
  background-color: var(--primary-blue-700);
}
```

### Angular Material Theme
The Material theme is configured with blue as the primary color in `material-theme.scss`:

```scss
@include mat.badge-overrides((background-color: blue));
```

## Component Examples

### Buttons
```html
<!-- Primary Button -->
<button class="!bg-blue-600 hover:!bg-blue-700 !text-white">
  Primary Action
</button>

<!-- Outlined Button -->
<button class="!border-blue-600 !text-blue-600 hover:!bg-blue-50">
  Secondary Action
</button>
```

### Links
```html
<a class="text-gray-700 hover:text-blue-600 hover:bg-blue-50">
  Navigation Link
</a>
```

### Active States
```html
<a routerLinkActive="active" class="[&.active]:!text-blue-600 [&.active]:!bg-blue-50">
  Active Link
</a>
```

## Files Updated with Blue Theme

1. **src/styles.css** - Global color variables and utility classes
2. **src/app/layout/header/header.component.html** - Navigation and buttons
3. **src/app/layout/header/header.component.css** - Badge and active states
4. **src/app/features/home/home.component.html** - Feature cards
5. **src/material-theme.scss** - Material Design theme

## Color Accessibility

All blue colors meet WCAG AA contrast requirements:
- Blue-600 (#2563eb) on white: 4.8:1 ✅
- White on blue-600: 4.8:1 ✅
- Blue-700 (#1d4ed8) on white: 6.5:1 ✅

## Quick Reference

| Element | Tailwind Class | Hex Color |
|---------|---------------|-----------|
| Primary Button | `bg-blue-600` | #2563eb |
| Button Hover | `hover:bg-blue-700` | #1d4ed8 |
| Link Hover | `hover:text-blue-600` | #2563eb |
| Light Background | `bg-blue-50` | #eff6ff |
| Border | `border-blue-600` | #2563eb |
| Badge | CSS: #2563eb | #2563eb |

## Migration from Purple to Blue

All instances of purple have been replaced:
- `purple-600` → `blue-600`
- `purple-700` → `blue-700`
- `purple-50` → `blue-50`
- `#7c3aed` → `#2563eb`
- `#f5f3ff` → `#eff6ff`
