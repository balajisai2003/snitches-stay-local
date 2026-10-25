#!/usr/bin/env bash
# ==============================================================================
# verify-phase0.sh
# Verifies the developer environment and Phase 0 container infrastructure.
# ==============================================================================

set -euo pipefail

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================================="
echo " Personal Document Organizer: Phase 0 Verification"
echo "=========================================================="
echo ""

check_tool() {
    local tool=$1
    local min_version=$2
    if command -v "$tool" &> /dev/null; then
        echo -e "[${GREEN}OK${NC}] $tool is installed: $($tool --version 2>&1 | head -n 1)"
    else
        echo -e "[${RED}MISSING${NC}] $tool is not installed. (Required: $min_version)"
    fi
}

echo "1. Checking Developer Tooling..."
check_tool "dotnet" ".NET 10 LTS"
check_tool "docker" "Docker Desktop 24+"
check_tool "node" "Node.js LTS (20+)"
check_tool "git" "Git 2.40+"

echo ""
echo "2. Checking Docker Daemon..."
if docker info &> /dev/null; then
    echo -e "[${GREEN}OK${NC}] Docker daemon is running."
else
    echo -e "[${YELLOW}WARNING${NC}] Docker daemon is not responding. (Ensure Docker Desktop is running if you intend to launch containers)."
fi

echo ""
echo "3. Checking Phase 0 Containers (if running)..."

check_http_endpoint() {
    local name=$1
    local url=$2
    if curl -s -f -o /dev/null "$url"; then
        echo -e "[${GREEN}OK${NC}] $name is accessible at $url"
    else
        echo -e "[${YELLOW}PENDING${NC}] $name is not reachable at $url (may not be started yet)."
    fi
}

check_http_endpoint "RabbitMQ Management UI" "http://localhost:15672"
check_http_endpoint "Jaeger UI" "http://localhost:16686"
check_http_endpoint "Main App Health" "http://localhost:5050/health"
check_http_endpoint "Processing Health" "http://localhost:5051/health"

echo ""
echo "=========================================================="
echo " Verification check completed."
echo "=========================================================="
