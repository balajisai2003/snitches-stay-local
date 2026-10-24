#!/usr/bin/env bash
# ==============================================================================
# process-file.sh
# Submits a file directly to the standalone Processing Service and polls for result.
# Usage: ./scripts/process-file.sh <path-to-file> [processing-url]
# ==============================================================================

set -euo pipefail

FILE_PATH="${1:-}"
PROCESSING_URL="${2:-http://localhost:5001}"

if [ -z "$FILE_PATH" ]; then
    echo "Usage: $0 <path-to-file> [processing-url]"
    exit 1
fi

if [ ! -f "$FILE_PATH" ]; then
    echo "Error: File '$FILE_PATH' not found."
    exit 1
fi

echo "1. Submitting '$FILE_PATH' to standalone Processing Service at $PROCESSING_URL..."

# Submit file to POST /jobs
RESPONSE=$(curl -s -X POST "$PROCESSING_URL/jobs" \
    -F "file=@$FILE_PATH")

JOB_ID=$(echo "$RESPONSE" | grep -o '"jobId":"[^"]*"' | cut -d':' -f2 | tr -d '"' || true)

if [ -z "$JOB_ID" ]; then
    echo "Failed to submit job. Response: $RESPONSE"
    exit 1
fi

echo "Job created successfully with ID: $JOB_ID"
echo "2. Polling status from GET $PROCESSING_URL/jobs/$JOB_ID..."

STATUS="Queued"
MAX_ATTEMPTS=30
ATTEMPT=0

while [ "$STATUS" != "Completed" ] && [ "$STATUS" != "Failed" ]; do
    ATTEMPT=$((ATTEMPT + 1))
    if [ "$ATTEMPT" -gt "$MAX_ATTEMPTS" ]; then
        echo "Timed out waiting for job to finish."
        exit 1
    fi

    sleep 1
    POLL_RESPONSE=$(curl -s "$PROCESSING_URL/jobs/$JOB_ID")
    STATUS=$(echo "$POLL_RESPONSE" | grep -o '"state":"[^"]*"' | cut -d':' -f2 | tr -d '"' || echo "Unknown")
    echo "   Attempt $ATTEMPT: Current state is '$STATUS'..."
done

echo ""
echo "=========================================================="
echo "Final Job Result: $STATUS"
echo "=========================================================="
echo "$POLL_RESPONSE"

