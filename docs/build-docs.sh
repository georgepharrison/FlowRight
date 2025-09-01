#!/bin/bash
set -e

echo "🔨 Building FlowRight API Documentation"
echo "======================================"

# Check if DocFX is installed
if ! command -v docfx &> /dev/null; then
    echo "❌ DocFX not found. Installing DocFX globally..."
    dotnet tool install -g docfx
    echo "✅ DocFX installed successfully"
fi

# Build the solution to generate XML documentation
echo ""
echo "📦 Building FlowRight solution..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(dirname "$SCRIPT_DIR")"
cd "$SOLUTION_DIR"
dotnet build FlowRight.sln --configuration Release --no-restore
echo "✅ Solution built successfully"

# Generate API documentation
echo ""
echo "📖 Generating API documentation with DocFX..."
cd "$SCRIPT_DIR"
docfx

# Check if generation was successful
if [ -d "_site" ]; then
    echo ""
    echo "✅ Documentation generated successfully!"
    echo "📂 Output directory: docs/_site"
    echo "🌐 Open docs/_site/index.html to view the documentation"
    echo ""
    echo "📊 Documentation Statistics:"
    echo "   - HTML files: $(find _site -name "*.html" | wc -l)"
    echo "   - API pages: $(find _site/api -name "*.html" | wc -l)"
    echo "   - Article pages: $(find _site/articles -name "*.html" | wc -l)"
    echo ""
    echo "🚀 To serve documentation locally, run:"
    echo "   docfx serve docs/_site"
else
    echo "❌ Documentation generation failed"
    exit 1
fi