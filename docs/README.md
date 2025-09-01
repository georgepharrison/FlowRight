# FlowRight API Documentation

This directory contains the DocFX configuration and build system for generating comprehensive API documentation for the FlowRight Result pattern library.

## 🎯 Overview

The documentation system generates:
- **Complete API Reference** - All public APIs across Core, Validation, and Http libraries
- **Interactive Documentation** - Searchable, cross-referenced HTML documentation
- **Getting Started Guide** - Comprehensive tutorial and examples
- **Migration Guide** - Transitioning from exception-based error handling
- **Best Practices** - Production-ready patterns and recommendations

## 📋 Prerequisites

- .NET 9.0 SDK or later
- DocFX (installed automatically by build scripts)

## 🚀 Quick Start

### Windows (PowerShell)
```powershell
# Build and generate documentation
.\build-docs.ps1

# Build and serve locally
.\build-docs.ps1 -Serve
```

### Linux/macOS (Bash)
```bash
# Build and generate documentation
./build-docs.sh

# Serve locally after generation
docfx serve docs/_site
```

### Manual Build
```bash
# 1. Install DocFX (if not already installed)
dotnet tool install -g docfx

# 2. Build the solution to generate XML documentation
cd .. && dotnet build FlowRight.sln --configuration Release

# 3. Generate documentation
cd docs && docfx

# 4. Serve locally (optional)
docfx serve _site
```

## 📁 Structure

```
docs/
├── docfx.json              # DocFX configuration
├── filterConfig.yml        # API filtering rules
├── toc.yml                 # Top-level navigation
├── index.md                # Documentation home page
├── api/
│   └── index.md            # API reference landing page
├── articles/
│   ├── index.md            # Articles overview
│   ├── getting-started.md  # Getting started guide
│   ├── migration-guide.md  # Migration guide
│   └── best-practices.md   # Best practices guide
└── _site/                  # Generated documentation (after build)
    ├── index.html          # Home page
    ├── api/                # API reference HTML
    └── articles/           # Article HTML
```

## ⚙️ Configuration

### DocFX Configuration (`docfx.json`)
- **Metadata Generation**: Extracts API metadata from all FlowRight projects
- **Content Processing**: Processes Markdown articles and API documentation
- **Template**: Uses modern DocFX template with search functionality
- **Filtering**: Includes only FlowRight namespaces, excludes system types

### API Filtering (`filterConfig.yml`)
- Includes: `FlowRight.*` namespaces and types
- Excludes: `System.*`, `Microsoft.*`, and build artifacts

### Navigation (`toc.yml`)
- Home page
- Articles section with guides
- API reference with comprehensive coverage

## 🔧 Customization

### Adding New Articles
1. Create a new `.md` file in `articles/`
2. Add entry to `articles/toc.yml`
3. Rebuild documentation

### Modifying Templates
The configuration uses the default DocFX modern template. To customize:
1. Extract template: `docfx template export default`
2. Modify template files
3. Update `docfx.json` template reference

### Updating Branding
Modify the `globalMetadata` section in `docfx.json`:
```json
"globalMetadata": {
  "_appName": "FlowRight",
  "_appTitle": "FlowRight - Result Pattern Library for .NET",
  "_appFooter": "Your custom footer text"
}
```

## 🌐 Hosting

The generated `_site` directory contains static HTML files that can be hosted anywhere:

### GitHub Pages
```yaml
# .github/workflows/docs.yml
- name: Build Documentation
  run: ./docs/build-docs.sh
- name: Deploy to GitHub Pages
  uses: peaceiris/actions-gh-pages@v3
  with:
    github_token: ${{ secrets.GITHUB_TOKEN }}
    publish_dir: ./docs/_site
```

### Azure Static Web Apps
```yaml
build:
  commands:
    - ./docs/build-docs.sh
  output: docs/_site
```

### Docker
```dockerfile
FROM nginx:alpine
COPY docs/_site /usr/share/nginx/html
EXPOSE 80
```

## 🐛 Troubleshooting

### Common Issues

**DocFX not found**
```bash
dotnet tool install -g docfx
# Ensure ~/.dotnet/tools is in PATH
```

**Missing XML documentation warnings**
- Ensure projects build successfully with `GenerateDocumentationFile` enabled
- Check `Directory.Build.props` has `<GenerateDocumentationFile>true</GenerateDocumentationFile>`

**API not appearing in documentation**
- Verify namespace is included in `filterConfig.yml`
- Check that types are `public` or `internal` with appropriate attributes

**Build failures**
```bash
# Clean and rebuild
dotnet clean && dotnet build --configuration Release
cd docs && docfx --force
```

### Performance Optimization
- Use `--cleanupCacheHistory` in DocFX for clean builds
- Enable incremental builds in CI/CD pipelines
- Consider parallel builds for large documentation sets

## 📈 Metrics

The build system tracks documentation coverage:
- **API Coverage**: All public types documented
- **XML Documentation**: Comprehensive summaries, parameters, and examples
- **Cross-references**: Automated linking between related types
- **Search Index**: Full-text search across all content

## 🤝 Contributing

When adding new public APIs:
1. Add comprehensive XML documentation
2. Include usage examples in `<example>` tags
3. Rebuild documentation to verify coverage
4. Update related articles if needed

---

*Generated with DocFX v2.78.3 | FlowRight v1.0.0-preview.1*