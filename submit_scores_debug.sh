#!/bin/bash

echo "🔍 Debug: Submitting scores with detailed error reporting..."
echo ""

# API endpoints
LOGIN_URL="http://localhost:5088/auth/login"
SUBMIT_URL="http://localhost:5088/leaderboard/submit"

# Test with just 1 user first
username="test_user_1"
current_timestamp=$(date +%s)

echo "1️⃣ Testing login for: $username"

# Login
login_response=$(curl -s -X POST "$LOGIN_URL" \
    -H "Content-Type: application/json" \
    -d "{
        \"username\": \"$username\",
        \"password\": \"TestPassword123\"
    }")

echo "Login response: $login_response"
echo ""

# Extract JWT
jwt_token=$(echo "$login_response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$jwt_token" ]; then
    echo "❌ Failed to get JWT token"
    exit 1
fi

echo "2️⃣ Testing score submission..."

# Test single score submission
idempotency_key="debug_test_$(date +%s)"

submit_response=$(curl -s -X POST "$SUBMIT_URL" \
    -H "Authorization: Bearer $jwt_token" \
    -H "Content-Type: application/json" \
    -H "X-Timestamp: $current_timestamp" \
    -H "Idempotency-Key: $idempotency_key" \
    -d '{
        "score": 1000,
        "gameMode": "Classic",
        "playerLevel": 10,
        "trophyCount": 25
    }')

echo "Submit response: $submit_response"
echo ""

# Check if successful
if echo "$submit_response" | grep -q '"success":true'; then
    echo "✅ Score submission successful!"
else
    echo "❌ Score submission failed!"
    echo "Error details: $(echo "$submit_response" | grep -o '"message":"[^"]*"' | cut -d'"' -f4)"
fi
